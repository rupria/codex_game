namespace CodexGame.Core.Shared
{
  public static class GameRules
  {
    public const int StartingHealth = 3;
    public const int HalliWinsToFinish = 3;
    public const int HalliFlipLimit = 25;
    public const int RequiredPrivateCards = 3;
    public const int ExposedCardsPerPile = 2;

    public const long SimultaneousBellThresholdMicroseconds = 33_300;
    public const long CardFlipTimeoutMicroseconds = 30_000_000;
    public const long ReviewGraceMicroseconds = 15_000_000;
    public const long WrongBellRewardSelectionTimeoutMicroseconds = 30_000_000;
    public const long PrivateSelectionTimeoutMicroseconds = 60_000_000;
    public const long AiMaximumReactionMicroseconds = 1_500_000;

    public const double AiValidBellMissProbability = 0.30;
  }
}
