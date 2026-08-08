using System;

namespace CodexGame.Presentation.Views
{
  internal readonly struct AcquiredCardFanLayout
  {
    private AcquiredCardFanLayout(int count, float startX, float step)
    {
      Count = count;
      StartX = startX;
      Step = step;
    }

    public int Count { get; }
    public float StartX { get; }
    public float Step { get; }

    public float X(int index)
    {
      if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
      return StartX + Step * index;
    }

    public static AcquiredCardFanLayout Create(
      int count,
      float areaX,
      float areaWidth,
      float cardWidth,
      float preferredStep)
    {
      if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
      if (areaWidth < 0f) throw new ArgumentOutOfRangeException(nameof(areaWidth));
      if (cardWidth <= 0f) throw new ArgumentOutOfRangeException(nameof(cardWidth));
      if (preferredStep < 0f) throw new ArgumentOutOfRangeException(nameof(preferredStep));

      if (count == 0) return new AcquiredCardFanLayout(0, areaX + areaWidth, 0f);

      var availableStep = count == 1
        ? 0f
        : Math.Max(0f, (areaWidth - cardWidth) / (count - 1));
      var step = count == 1 ? 0f : Math.Min(preferredStep, availableStep);
      var occupiedWidth = cardWidth + step * (count - 1);
      var startX = areaX + Math.Max(0f, areaWidth - occupiedWidth);
      return new AcquiredCardFanLayout(count, startX, step);
    }
  }
}
