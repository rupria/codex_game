namespace CodexGame.Application.Playable
{
  public sealed class PlayableTransitionSnapshot
  {
    public PlayableTransitionSnapshot(
      PlayableTransitionKind kind,
      long remainingMicroseconds,
      float progress)
    {
      Kind = kind;
      RemainingMicroseconds = remainingMicroseconds;
      Progress = progress;
    }

    public PlayableTransitionKind Kind { get; }
    public long RemainingMicroseconds { get; }
    public float Progress { get; }
  }
}
