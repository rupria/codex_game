using System;
using System.Collections.Generic;
using CodexGame.Core.Cards;

namespace CodexGame.Application.Development
{
  public sealed class PokerCheatSetup
  {
    public PokerCheatSetup(
      IReadOnlyList<Card> playerCards,
      IReadOnlyList<Card> aiCards,
      IReadOnlyList<Card> publicCards)
    {
      PlayerCards = Copy(playerCards);
      AiCards = Copy(aiCards);
      PublicCards = Copy(publicCards);
    }

    public IReadOnlyList<Card> PlayerCards { get; }
    public IReadOnlyList<Card> AiCards { get; }
    public IReadOnlyList<Card> PublicCards { get; }

    private static IReadOnlyList<Card> Copy(IReadOnlyList<Card> source)
    {
      var copy = new Card[source.Count];
      for (var index = 0; index < copy.Length; index++) copy[index] = source[index];
      return Array.AsReadOnly(copy);
    }
  }
}
