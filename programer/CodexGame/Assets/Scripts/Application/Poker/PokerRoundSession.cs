using System;
using System.Collections.Generic;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;

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

    public PokerRoundPhase Phase { get; private set; } = PokerRoundPhase.NotStarted;

    public void Begin(
      Card firstPublicCard,
      PrivateCardDistributionResult distribution,
      BattleHealth health,
      PokerRuleSet ruleSet)
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

      // Validate all seven identities before any concealed information is presented.
      PokerComparer.Compare(_playerPrivateCards, _aiPrivateCards, _publicCards, _ruleSet);
      Phase = PokerRoundPhase.AwaitingPrediction;
    }

    public PokerRoundResult Resolve(PredictionChoice choice)
    {
      if (Phase != PokerRoundPhase.AwaitingPrediction)
      {
        throw new InvalidOperationException("A prediction can resolve only once during the poker round.");
      }

      var comparison = PokerComparer.Compare(
        _playerPrivateCards,
        _aiPrivateCards,
        _publicCards,
        _ruleSet);
      var damage = DamageResolver.ApplyPokerLoss(_health, comparison.Winner);
      var prediction = PredictionResolver.Resolve(choice, comparison.Winner);
      _result = new PokerRoundResult(comparison, damage, prediction);
      _health = damage.After;
      Phase = PokerRoundPhase.Resolved;
      return _result;
    }

    public PokerRoundSnapshot GetSnapshot()
    {
      var visibleAiCards = Phase == PokerRoundPhase.Resolved
        ? _aiPrivateCards
        : EmptyCards;
      return new PokerRoundSnapshot(
        Phase,
        _playerPrivateCards,
        visibleAiCards,
        _publicCards,
        _health,
        _result);
    }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> cards)
    {
      var copy = new Card[cards.Count];
      for (var index = 0; index < cards.Count; index++) copy[index] = cards[index];
      return Array.AsReadOnly(copy);
    }
  }
}
