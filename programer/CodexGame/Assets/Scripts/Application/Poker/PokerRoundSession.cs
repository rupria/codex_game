using System;
using System.Collections.Generic;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Poker
{
  public sealed class PokerRoundSession
  {
    private static readonly IReadOnlyList<Card> EmptyCards = Array.AsReadOnly(Array.Empty<Card>());

    private IReadOnlyList<Card> _playerPrivateCards = EmptyCards;
    private IReadOnlyList<Card> _aiPrivateCards = EmptyCards;
    private IReadOnlyList<Card> _publicCards = EmptyCards;
    private BattleHealth _health = BattleHealth.Initial;
    private PokerRuleSet _ruleSet = PokerRuleSet.Development;
    private PokerRoundResult? _result;
    private GameTimestamp _predictionDeadline;
    private GameTimestamp _resultRevealAt;
    private GameTimestamp _jokerPresentationEndsAt;
    private int _preRevealAiCardIndex = -1;
    private IReadOnlyList<JokerHandOption> _playerJokerOptions = Array.AsReadOnly(Array.Empty<JokerHandOption>());
    private PokerHandCategory? _playerJokerCategory;
    private PokerHandCategory? _aiJokerCategory;

    public PokerRoundPhase Phase { get; private set; } = PokerRoundPhase.NotStarted;
    public PokerRoundResult? Result => _result;

    public void Begin(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      BattleHealth health,
      PokerRuleSet ruleSet,
      GameTimestamp now,
      int preRevealAiCardIndex = -1)
    {
      if (distribution == null) throw new ArgumentNullException(nameof(distribution));
      if (ruleSet == null) throw new ArgumentNullException(nameof(ruleSet));
      if (!firstPublicCard.IsValid) throw new ArgumentException("The first public card is invalid.", nameof(firstPublicCard));
      if (health.IsBattleOver) throw new InvalidOperationException("A poker round cannot begin after battle end.");

      _playerPrivateCards = Copy(distribution.PlayerPrivateCards);
      _aiPrivateCards = Copy(distribution.AiPrivateCards);
      _publicCards = Array.AsReadOnly(new[] { firstPublicCard, distribution.SecondPublicCard });
      _health = health;
      _ruleSet = ruleSet;
      _result = null;
      _playerJokerCategory = null;
      _aiJokerCategory = null;
      _playerJokerOptions = Array.AsReadOnly(Array.Empty<JokerHandOption>());
      if (preRevealAiCardIndex < -1 || preRevealAiCardIndex >= _aiPrivateCards.Count)
      {
        throw new ArgumentOutOfRangeException(nameof(preRevealAiCardIndex));
      }
      _preRevealAiCardIndex = NormalizePreRevealAiCardIndex(
        _aiPrivateCards,
        preRevealAiCardIndex);

      // Validate all seven identities before any concealed information is presented.
      PokerComparer.Compare(_playerPrivateCards, _aiPrivateCards, _publicCards, _ruleSet);
      if (ContainsJoker(_aiPrivateCards))
      {
        _aiJokerCategory = PokerJokerHandResolver.ResolveStrongest(
          Join(_aiPrivateCards, _publicCards),
          _ruleSet).Category;
      }
      if (ContainsJoker(_playerPrivateCards))
      {
        _playerJokerOptions = PokerJokerHandResolver.GetLegalOptions(
          Join(_playerPrivateCards, _publicCards),
          _ruleSet);
        Phase = PokerRoundPhase.PlayerJokerPresentation;
        _jokerPresentationEndsAt = Add(now, GameRules.PlayerJokerPresentationMicroseconds);
      }
      else
      {
        BeginPrediction(now);
      }
    }

    public void Begin(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      BattleHealth health,
      PokerRuleSet ruleSet)
    {
      Begin(firstPublicCard, distribution, health, ruleSet, new GameTimestamp(0));
    }

    public bool SubmitPrediction(PredictionChoice choice, GameTimestamp now)
    {
      if (Phase != PokerRoundPhase.AwaitingPrediction
        || choice == PredictionChoice.Skipped)
      {
        return false;
      }

      BeginResult(choice, now);
      return true;
    }

    public bool SubmitPlayerJokerChoice(PokerHandCategory category, GameTimestamp now)
    {
      if (Phase != PokerRoundPhase.AwaitingPlayerJokerChoice) return false;
      for (var index = 0; index < _playerJokerOptions.Count; index++)
      {
        if (_playerJokerOptions[index].Category != category) continue;
        _playerJokerCategory = category;
        BeginPrediction(now);
        return true;
      }
      return false;
    }

    public bool Tick(GameTimestamp now)
    {
      if (Phase == PokerRoundPhase.PlayerJokerPresentation
        && now.Microseconds >= _jokerPresentationEndsAt.Microseconds)
      {
        Phase = PokerRoundPhase.AwaitingPlayerJokerChoice;
      }

      if (Phase == PokerRoundPhase.AwaitingPrediction
        && now.Microseconds >= _predictionDeadline.Microseconds)
      {
        BeginResult(PredictionChoice.Skipped, _predictionDeadline);
      }

      if (Phase == PokerRoundPhase.ResultPending
        && now.Microseconds >= _resultRevealAt.Microseconds)
      {
        Phase = PokerRoundPhase.Resolved;
        return true;
      }

      return false;
    }

    // Compatibility entry point for non-runtime callers. Runtime flow uses SubmitPrediction + Tick.
    public PokerRoundResult Resolve(PredictionChoice choice)
    {
      if (Phase == PokerRoundPhase.PlayerJokerPresentation)
      {
        Tick(_jokerPresentationEndsAt);
      }
      if (Phase == PokerRoundPhase.AwaitingPlayerJokerChoice)
      {
        if (_playerJokerOptions.Count == 0
          || !SubmitPlayerJokerChoice(_playerJokerOptions[0].Category, new GameTimestamp(0)))
        {
          throw new InvalidOperationException("The Joker hand choice could not be resolved.");
        }
      }
      if (!SubmitPrediction(choice, new GameTimestamp(0)))
      {
        throw new InvalidOperationException("A prediction can resolve only once during the poker round.");
      }
      Tick(new GameTimestamp(GameRules.PokerResultAnnouncementMicroseconds));
      return _result!;
    }

    public PokerRoundSnapshot GetSnapshot(GameTimestamp now)
    {
      var visibleAiCards = Phase == PokerRoundPhase.Resolved
        ? _aiPrivateCards
        : _preRevealAiCardIndex >= 0
          ? Array.AsReadOnly(new[] { _aiPrivateCards[_preRevealAiCardIndex] })
          : EmptyCards;
      var health = Phase == PokerRoundPhase.Resolved && _result != null
        ? _result.Damage.After
        : _health;
      var remaining = Phase == PokerRoundPhase.AwaitingPrediction
        ? Math.Max(0, _predictionDeadline.Microseconds - now.Microseconds)
        : Phase == PokerRoundPhase.PlayerJokerPresentation
          ? Math.Max(0, _jokerPresentationEndsAt.Microseconds - now.Microseconds)
        : Phase == PokerRoundPhase.ResultPending
          ? Math.Max(0, _resultRevealAt.Microseconds - now.Microseconds)
          : 0;
      return new PokerRoundSnapshot(
        Phase,
        _playerPrivateCards,
        visibleAiCards,
        _publicCards,
        health,
        remaining,
        Phase == PokerRoundPhase.Resolved ? _result : null,
        Phase == PokerRoundPhase.AwaitingPlayerJokerChoice
          ? _playerJokerOptions
          : Array.AsReadOnly(Array.Empty<JokerHandOption>()),
        _playerJokerCategory);
    }

    public PokerRoundSnapshot GetSnapshot()
    {
      return GetSnapshot(new GameTimestamp(0));
    }

    private void BeginPrediction(GameTimestamp now)
    {
      Phase = PokerRoundPhase.AwaitingPrediction;
      _predictionDeadline = Add(now, GameRules.PredictionTimeoutMicroseconds);
    }

    private void BeginResult(PredictionChoice choice, GameTimestamp now)
    {
      var comparison = PokerComparer.Compare(
        _playerPrivateCards,
        _aiPrivateCards,
        _publicCards,
        _ruleSet,
        _playerJokerCategory,
        _aiJokerCategory);
      var damage = DamageResolver.ApplyPokerLoss(_health, comparison.Winner);
      var prediction = PredictionResolver.Resolve(choice, comparison.Winner);
      _result = new PokerRoundResult(comparison, damage, prediction);
      _resultRevealAt = Add(now, GameRules.PokerResultAnnouncementMicroseconds);
      Phase = PokerRoundPhase.ResultPending;
    }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> cards)
    {
      var copy = new Card[cards.Count];
      for (var index = 0; index < cards.Count; index++) copy[index] = cards[index];
      return Array.AsReadOnly(copy);
    }

    private static bool ContainsJoker(IReadOnlyList<Card> cards)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].IsJoker) return true;
      }
      return false;
    }

    private static IReadOnlyList<Card> Join(
      IReadOnlyList<Card> privateCards,
      IReadOnlyList<Card> publicCards)
    {
      var cards = new Card[privateCards.Count + publicCards.Count];
      for (var index = 0; index < privateCards.Count; index++) cards[index] = privateCards[index];
      for (var index = 0; index < publicCards.Count; index++)
      {
        cards[privateCards.Count + index] = publicCards[index];
      }
      return Array.AsReadOnly(cards);
    }

    private static int NormalizePreRevealAiCardIndex(
      IReadOnlyList<Card> aiPrivateCards,
      int requestedIndex)
    {
      if (requestedIndex < 0 || !aiPrivateCards[requestedIndex].IsJoker) return requestedIndex;
      for (var index = 0; index < aiPrivateCards.Count; index++)
      {
        if (!aiPrivateCards[index].IsJoker) return index;
      }
      return -1;
    }

    private static GameTimestamp Add(GameTimestamp timestamp, long microseconds)
    {
      if (microseconds < 0 || timestamp.Microseconds > long.MaxValue - microseconds)
      {
        throw new ArgumentOutOfRangeException(nameof(microseconds));
      }
      return new GameTimestamp(timestamp.Microseconds + microseconds);
    }
  }
}
