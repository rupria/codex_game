namespace CodexGame.Core.Shared
{
  public static class GameRules
  {
    public const int StartingHealth = 3;
    public const int HalliWinsToFinish = 3;
    public const int CardsPerHalliDistribution = 4;
    public const int HalliDistributionLimit = 12;
    public const int HalliFlipLimit = HalliDistributionLimit;
    public const int RequiredPrivateCards = 3;
    public const int ExposedCardsPerPile = 2;
    public const int JokerEligibilityAcquiredCards = 4;
    public const int JokerAwardPercent = 10;
    public const int InventoryCapacity = 4;
    public const int MaximumPredictionSuccessCount = 5;
    public const int BarShopSlotCount = 4;
    public const int InitialStageCount = 3;

    public const long SimultaneousBellThresholdMicroseconds = 33_300;
    public const long BellInputTimeoutMicroseconds = 30_000_000;
    public const long CardRevealMotionMinimumMicroseconds = 180_000;
    public const long CardRevealMotionRangeMicroseconds = 40_000;
    public const long CardRevealGapMinimumMicroseconds = 60_000;
    public const long CardRevealGapRangeMicroseconds = 40_000;
    public const long HalliOpeningPresentationMicroseconds = 1_200_000;
    public const long HalliClosingPresentationMicroseconds = 700_000;
    public const long HalliResultLockMicroseconds = 2_000_000;
    public const long PrivateSelectionTimeoutMicroseconds = 60_000_000;
    public const long PredictionTimeoutMicroseconds = 120_000_000;
    public const long PokerResultAnnouncementMicroseconds = 1_000_000;
    public const long PlayerJokerPresentationMicroseconds = 900_000;
    public const long BarShopPouchCoverMicroseconds = 120_000;
    public const long BarShopCoinFlipDurationMicroseconds = 500_000;
    public const long BarShopBulletPourDurationMicroseconds = 750_000;
    public const long BarShopPurchaseRejectedShakeMicroseconds = 120_000;
    public const long NextStageTransitionShopClearMicroseconds = 220_000;
    public const long NextStageTransitionCameraTurnMicroseconds = 320_000;
    public const long NextStageTransitionWalkMicroseconds = 650_000;
    public const long NextStageTransitionDoorOpenMicroseconds = 180_000;
    public const long NextStageTransitionThresholdMicroseconds = 280_000;
    public const long NextStageTransitionFadeOutMicroseconds = 250_000;
    public const long NextStageTransitionFixedPreloadMicroseconds = 1_900_000;
    public const long NextStageTransitionMinimumBlackHoldMicroseconds = 150_000;
    public const long NextStageTransitionFadeInMicroseconds = 350_000;
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
