namespace CodexGame.Core.Halli
{
  public enum HalliAiBellResolution
  {
    Miss = 0,
    FieldChanged = 1,
    FlipCancelled = 2,
    PlayerInputFirst = 3,
    SimultaneousPlayerPriority = 4,
    AiInputFirst = 5,
    BellTimeout = 6,
    RoundFinished = 7
  }
}
