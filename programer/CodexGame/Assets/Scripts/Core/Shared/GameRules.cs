namespace CodexGame.Core.Shared
{
  public static class GameRules
  {
    public const int StartingHealth = 3;
    public const int HalliWinsToFinish = 3;
    public const int HalliFlipLimit = 26;
    public const int RequiredPrivateCards = 3;
    public const int ExposedCardsPerPile = 2;

    public const double CardFlipTimeoutSeconds = 30.0;
    public const double ReviewGraceSeconds = 15.0;
    public const double PrivateSelectionTimeoutSeconds = 60.0;

    public const double AiValidBellMissProbability = 0.30;
  }
}
