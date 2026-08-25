namespace CodexGame.Presentation.Views
{
  internal static class PrivateSelectionPanelLayout
  {
    public const float PanelX = 50f;
    public const float PanelY = 42f;
    public const float PanelWidth = 860f;
    public const float PanelHeight = 456f;

    public const float CandidateAreaX = 270f;
    public const float CandidateAreaY = 124f;
    public const float CandidateAreaWidth = 620f;
    public const float CandidateAreaHeight = 328f;
    public const float CandidateTopY = 140f;
    public const float CandidateBottomY = 286f;
    public const float CandidateWidth = 112f;
    public const float CandidateHeight = 150f;
    public const float CandidateGapX = 20f;
    public const int CandidateColumns = 4;
    public const int MaximumCandidateCount = 5;

    public const float ConfirmVisualX = 72f;
    public const float ConfirmVisualY = 366f;
    public const float ConfirmVisualWidth = 184f;
    public const float ConfirmVisualHeight = 120f;
    public const float ConfirmHitX = 64f;
    public const float ConfirmHitY = 358f;
    public const float ConfirmHitWidth = 200f;
    public const float ConfirmHitHeight = 136f;
    public const float ConfirmTitleX = 80f;
    public const float ConfirmTitleY = 388f;
    public const float ConfirmTitleWidth = 168f;
    public const float ConfirmTitleHeight = 40f;
    public const float ConfirmProgressX = 80f;
    public const float ConfirmProgressY = 438f;
    public const float ConfirmProgressWidth = 168f;
    public const float ConfirmProgressHeight = 28f;

    public static float CandidateX(int index)
    {
      var column = index < CandidateColumns ? index : 0;
      return 326f + column * (CandidateWidth + CandidateGapX);
    }

    public static float CandidateY(int index)
    {
      return index < CandidateColumns ? CandidateTopY : CandidateBottomY;
    }
  }
}
