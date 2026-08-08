using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public sealed class HalliField
  {
    private readonly PileState _leftPile = new PileState();
    private readonly PileState _rightPile = new PileState();

    public Card? Expose(PileSide side, Card card)
    {
      return GetPile(side).Expose(card);
    }

    public IReadOnlyList<Card> GetExposedCards(PileSide side)
    {
      return GetPile(side).ExposedCards;
    }

    public IReadOnlyList<Card> Clear(PileSide side)
    {
      return GetPile(side).Clear();
    }

    public void MoveTopToBottom(PileSide side)
    {
      GetPile(side).MoveTopToBottom();
    }

    private PileState GetPile(PileSide side)
    {
      switch (side)
      {
        case PileSide.Left:
          return _leftPile;
        case PileSide.Right:
          return _rightPile;
        default:
          throw new ArgumentOutOfRangeException(nameof(side));
      }
    }
  }
}
