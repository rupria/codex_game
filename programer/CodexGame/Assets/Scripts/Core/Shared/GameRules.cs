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
    public const long SequentialRevealMinimumMicroseconds = 300_000;
    public const long SequentialRevealRangeMicroseconds = 200_000;
    public const long NextFlipLockMicroseconds = 1_000_000;
    public const long ReviewGraceMicroseconds = NextFlipLockMicroseconds;
    public const long WrongBellRewardInitialLockMicroseconds = 2_000_000;
    public const long WrongBellRewardSelectionTimeoutMicroseconds = 30_000_000;
    public const long WrongBellRewardResultLockMicroseconds = 2_000_000;
    public const long PrivateSelectionTimeoutMicroseconds = 60_000_000;
    public const long PredictionTimeoutMicroseconds = 60_000_000;
    public const long PokerResultAnnouncementMicroseconds = 1_000_000;
    public const long GlobalInactivityTimeoutMicroseconds = 180_000_000;
    public const long AiMinimumReactionMicroseconds = 1_000_000;
    public const long AiTypicalReactionMicroseconds = 2_000_000;
    public const long AiMaximumReactionMicroseconds = 3_000_000;

    public const int AiCorrectBellPercent = 60;
    public const int AiWrongBellPercent = 20;
    public const int AiValidBellMissPercent = 20;
    public const double AiValidBellMissProbability = 0.20;
  }
}
