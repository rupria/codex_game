using System;

namespace CodexGame.Presentation.Views
{
  internal enum PokerResultPanelSize
  {
    Compact,
    Standard,
    Expanded
  }

  internal readonly struct PokerResultPanelLayout
  {
    public const float Width = 788f;
    public const float MessageWidth = 686f;
    public const float ItemStatusHeight = 40f;
    private const float VerticalPadding = 32f;

    private PokerResultPanelLayout(PokerResultPanelSize size, float y, float height)
    {
      Size = size;
      Y = y;
      Height = height;
    }

    public PokerResultPanelSize Size { get; }
    public float Y { get; }
    public float Height { get; }

    public static PokerResultPanelLayout Select(
      float measuredMessageHeight,
      bool hasItemStatus)
    {
      if (measuredMessageHeight < 0f)
      {
        throw new ArgumentOutOfRangeException(nameof(measuredMessageHeight));
      }

      var requiredHeight = measuredMessageHeight
        + VerticalPadding
        + (hasItemStatus ? ItemStatusHeight : 0f);
      if (requiredHeight <= 108f)
      {
        return new PokerResultPanelLayout(PokerResultPanelSize.Compact, 190f, 108f);
      }
      if (requiredHeight <= 132f)
      {
        return new PokerResultPanelLayout(PokerResultPanelSize.Standard, 178f, 132f);
      }
      return new PokerResultPanelLayout(PokerResultPanelSize.Expanded, 160f, 164f);
    }
  }
}
