using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public sealed class WrongBellRewardSelectionSession
  {
    private static readonly IReadOnlyList<Card> EmptyCards =
      Array.AsReadOnly(Array.Empty<Card>());

    private IReadOnlyList<Card> _candidates = EmptyCards;
    private IRandomSource? _random;
    private GameTimestamp _deadline;

    public bool IsActive { get; private set; }

    public IReadOnlyList<Card> Candidates => _candidates;

    public Card? SelectedCard { get; private set; }

    public bool TimedOut { get; private set; }

    public void Begin(
      IReadOnlyList<Card> candidates,
      IRandomSource random,
      GameTimestamp now)
    {
      if (candidates == null) throw new ArgumentNullException(nameof(candidates));
      if (random == null) throw new ArgumentNullException(nameof(random));
      if (candidates.Count == 0)
      {
        throw new ArgumentException(
          "Wrong-bell reward selection needs at least one candidate.",
          nameof(candidates));
      }

      var copy = new Card[candidates.Count];
      var ids = new HashSet<CardId>();
      for (var index = 0; index < candidates.Count; index++)
      {
        if (!candidates[index].IsValid || !ids.Add(candidates[index].Id))
        {
          throw new ArgumentException(
            "Wrong-bell reward candidates must be valid and unique.",
            nameof(candidates));
        }

        copy[index] = candidates[index];
      }

      _candidates = Array.AsReadOnly(copy);
      _random = random;
      _deadline = Add(now, GameRules.WrongBellRewardSelectionTimeoutMicroseconds);
      SelectedCard = null;
      TimedOut = false;
      IsActive = true;
    }

    public bool TrySelect(CardId cardId)
    {
      if (!IsActive)
      {
        return false;
      }

      for (var index = 0; index < _candidates.Count; index++)
      {
        if (_candidates[index].Id == cardId)
        {
          Complete(_candidates[index], false);
          return true;
        }
      }

      return false;
    }

    public bool Tick(GameTimestamp now)
    {
      if (!IsActive || now.Microseconds < _deadline.Microseconds)
      {
        return false;
      }

      if (_random == null)
      {
        throw new InvalidOperationException("Wrong-bell reward random source is missing.");
      }

      Complete(_candidates[_random.NextInt(_candidates.Count)], true);
      return true;
    }

    public long GetRemainingMicroseconds(GameTimestamp now)
    {
      return IsActive
        ? Math.Max(0, _deadline.Microseconds - now.Microseconds)
        : 0;
    }

    private void Complete(Card selected, bool timedOut)
    {
      SelectedCard = selected;
      TimedOut = timedOut;
      IsActive = false;
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
