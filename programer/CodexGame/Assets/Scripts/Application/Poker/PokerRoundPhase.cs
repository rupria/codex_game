namespace CodexGame.Application.Poker
{
  public enum PokerRoundPhase
  {
    NotStarted = 0,
    // Value 1 was the retired public item window. Keep later values stable.
    AwaitingPrediction = 2,
    ResultPending = 3,
    Resolved = 4
  }
}
