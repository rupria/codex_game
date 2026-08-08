namespace CodexGame.Application.Playable
{
  public enum NextStageTransitionStep
  {
    Inactive = 0,
    ShopUiClear = 1,
    CameraTurnToExit = 2,
    WalkToDoor = 3,
    PushSwingDoors = 4,
    CrossThreshold = 5,
    FadeOutAndBeginLoad = 6,
    LoadingLoop = 7,
    NextStageFadeIn = 8,
    Complete = 9
  }
}
