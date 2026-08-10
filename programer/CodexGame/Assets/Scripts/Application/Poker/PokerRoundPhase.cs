namespace CodexGame.Application.Poker
{
  public enum PokerRoundPhase
  {
    NotStarted = 0,
    PlayerJokerPresentation = 1,
    AwaitingPlayerJokerChoice = 2,
    AwaitingPrediction = 3,
    ResultPending = 4,
    Resolved = 5
  }
}
