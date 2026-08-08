namespace CodexGame.Application.Playable
{
  public sealed class NextStageTransitionSnapshot
  {
    public NextStageTransitionSnapshot(
      NextStageTransitionStep step,
      float stepProgress,
      long elapsedMicroseconds,
      bool isLoadComplete,
      bool shouldShowLoading)
    {
      Step = step;
      StepProgress = stepProgress;
      ElapsedMicroseconds = elapsedMicroseconds;
      IsLoadComplete = isLoadComplete;
      ShouldShowLoading = shouldShowLoading;
    }

    public NextStageTransitionStep Step { get; }
    public float StepProgress { get; }
    public long ElapsedMicroseconds { get; }
    public bool IsLoadComplete { get; }
    public bool ShouldShowLoading { get; }
  }
}
