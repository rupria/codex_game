namespace CodexGame.Application.Playable
{
  public enum PlayableGamePhase
  {
    Intro = 0,
    HalliOpening = 1,
    Halli = 2,
    HalliTransition = 3,
    PrivateSelection = 4,
    // Value 5 was the retired public item window. Keep later values stable.
    PokerPrediction = 6,
    PokerResult = 7,
    StageWon = 8,
    BattleFinished = 9
  }
}
