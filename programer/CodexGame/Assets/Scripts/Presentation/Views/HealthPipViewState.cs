using System;

namespace CodexGame.Presentation.Views
{
  internal readonly struct HealthPipViewState
  {
    private HealthPipViewState(int filledCount, int emptyCount)
    {
      FilledCount = filledCount;
      EmptyCount = emptyCount;
    }

    public int FilledCount { get; }
    public int EmptyCount { get; }

    public static HealthPipViewState Create(int currentHealth, int maximumHealth)
    {
      if (maximumHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maximumHealth));
      var filled = Math.Max(0, Math.Min(maximumHealth, currentHealth));
      return new HealthPipViewState(filled, maximumHealth - filled);
    }
  }
}
