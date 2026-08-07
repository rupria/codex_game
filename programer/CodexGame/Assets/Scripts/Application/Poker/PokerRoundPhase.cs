namespace CodexGame.Application.Poker
{
  public enum PokerRoundPhase
  {
    NotStarted = 0,
    ItemWindow = 1,
    AwaitingPrediction = 2,
    ResultPending = 3,
    Resolved = 4
  }
}
