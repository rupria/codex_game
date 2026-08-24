namespace CodexGame.Presentation.Views
{
  internal static class PrivateSelectionPanelLayout
  {
    public const float PanelX = 50f;
    public const float PanelY = 42f;
    public const float PanelWidth = 860f;
    public const float PanelHeight = 456f;

    public const float CandidateAreaX = 270f;
    public const float CandidateAreaWidth = 620f;
    public const float CandidateTopY = 136f;
    public const float CandidateBottomY = 294f;
    public const float CandidateWidth = 112f;
    public const float CandidateHeight = 150f;
    public const float CandidateGapX = 20f;
    public const int CandidateColumns = 4;
    public const int MaximumCandidateCount = 5;

    public const float SelectionCountX = 72f;
    public const float SelectionCountY = 412f;
    public const float SelectionCountWidth = 184f;
    public const float SelectionCountHeight = 64f;

    public const float ConfirmVisualX = 580f;
    public const float ConfirmVisualY = 412f;
    public const float ConfirmVisualWidth = 280f;
    public const float ConfirmVisualHeight = 60f;
    public const float ConfirmHitX = 568f;
    public const float ConfirmHitY = 400f;
    public const float ConfirmHitWidth = 304f;
    public const float ConfirmHitHeight = 84f;

    public static float CandidateX(int index, int candidateCount)
    {
      var safeCount = candidateCount < 0
        ? 0
        : candidateCount > MaximumCandidateCount
          ? MaximumCandidateCount
          : candidateCount;
      var topCount = safeCount > CandidateColumns ? CandidateColumns : safeCount;
      if (index < CandidateColumns)
      {
        var rowWidth = topCount * CandidateWidth
          + (topCount > 0 ? topCount - 1 : 0) * CandidateGapX;
        return CandidateAreaX
          + (CandidateAreaWidth - rowWidth) * 0.5f
          + index * (CandidateWidth + CandidateGapX);
      }

      // The fifth card is aligned below the second column. This keeps the
      // single confirm action clear instead of leaving a large dead area.
      return 452f;
    }

    public static float CandidateY(int index)
    {
      return index < CandidateColumns ? CandidateTopY : CandidateBottomY;
    }
  }
}
