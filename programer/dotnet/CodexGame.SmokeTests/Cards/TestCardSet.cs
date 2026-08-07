using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.SmokeTests.Cards
{
  internal static class TestCardSet
  {
    public static IReadOnlyList<Card> Create()
    {
      return CardSetFactory.CreateStandard52(new CyclingSkullPolicy());
    }

    private sealed class CyclingSkullPolicy : ICardSkullPolicy
    {
      public int ResolveSkullCount(CardSuit suit, CardRank rank)
      {
        return (((int)suit * CardId.RanksPerSuit) + (int)rank) % 3 + 1;
      }
    }
  }
}
