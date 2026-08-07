namespace CodexGame.Core.Cards
{
  public interface ICardSkullPolicy
  {
    int ResolveSkullCount(CardSuit suit, CardRank rank);
  }
}
