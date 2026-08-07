using CodexGame.Core.Cards;

namespace CodexGame.Application.Playable
{
  public sealed class PrototypeSkullPolicy : ICardSkullPolicy
  {
    public int ResolveSkullCount(CardSuit suit, CardRank rank)
    {
      var cardIndex = CardId.Create(suit, rank).Value;

      if (cardIndex < 18)
      {
        return 1;
      }

      return cardIndex < 35 ? 2 : 3;
    }
  }
}
