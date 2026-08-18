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
    public const int PairAssistMinimumCombinedHealth = 5;
    public const int PairAssistFillPercent = 85;

    public const long SimultaneousBellThresholdMicroseconds = 33_300;
    public const long BellInputTimeoutMicroseconds = 30_000_000;
    public const long CardRevealMotionMinimumMicroseconds = 180_000;
    public const long CardRevealMotionRangeMicroseconds = 40_000;
    public const long CardRevealGapMinimumMicroseconds = 60_000;
    public const long CardRevealGapRangeMicroseconds = 40_000;
    public const int StageItemRestrictionActivationPercent = 20;
    public const int StageItemRestrictionMinimumUses = 1;
    public const int StageItemRestrictionMaximumUses = 2;
    public const int BarShopRerollCost = 1;
    public const int BarShopMaximumRerolls = 2;
    public const int PredictionInsuranceCharges = 2;

    public const long StageEntryPresentationMicroseconds = 6_000_000;
    public const long ThreeCallEntryPresentationMicroseconds = 5_000_000;
    public const long ThreeCallToSelectionPresentationMicroseconds = 2_000_000;
    public const long HalliResultLockMicroseconds = 2_000_000;
    public const long PrivateSelectionTimeoutMicroseconds = 60_000_000;
    public const long PokerHandConfirmationTimeoutMicroseconds = 120_000_000;
    public const long PredictionTimeoutMicroseconds = 120_000_000;
    public const long PokerResultCardRevealMicroseconds = 1_000_000;
    public const long PokerResultOutcomeMicroseconds = 1_000_000;
    public const long PokerResultAnnouncementMicroseconds =
      PokerResultCardRevealMicroseconds + PokerResultOutcomeMicroseconds;
    public const long PlayerJokerPresentationMicroseconds = 1_000_000;
    public const long PlayerJokerFrontHighlightMicroseconds = 500_000;
    public const long AiJokerShowdownHighlightMicroseconds = 500_000;
    public const long ReloadItemPresentationMicroseconds = 800_000;
    public const long BottomDealItemPresentationMicroseconds = 1_000_000;
    public const long HypeManItemPresentationMicroseconds = 800_000;
    public const long HealthRecoveryItemPresentationMicroseconds = 500_000;
    public const long WildInkItemPresentationMicroseconds = 650_000;
    public const long BarrelItemPresentationMicroseconds = 450_000;
    public const long BarrelDefensePresentationMicroseconds = 550_000;
    public const long PredictionInsuranceItemPresentationMicroseconds = 450_000;
    public const long PredictionInsuranceActivationPresentationMicroseconds = 400_000;
    public const long PredictionInsuranceActivationChargeCommitMicroseconds = 240_000;
    public const long MercenaryItemPresentationMicroseconds = 900_000;
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
    public const long AiFastReactionMaximumMicroseconds = 2_000_000;
    public const long AiMidReactionMaximumMicroseconds = 5_000_000;
    public const long AiMaximumReactionMicroseconds = 10_000_000;

    public const int AiCorrectBellPercent = 60;
    public const int AiWrongBellPercent = 20;
    public const int AiValidBellMissPercent = 20;
    public const double AiValidBellMissProbability = 0.20;
    public const int AiFastReactionWeightPercent = 60;
    public const int AiMidReactionWeightPercent = 30;
    public const int AiSlowReactionWeightPercent = 10;
    public const int AiFastConditionalMissPercent = 10;
    public const int AiMidConditionalMissPercent = 30;
    public const int AiSlowConditionalMissPercent = 50;
    public const int AiNonMissCorrectPercent = 75;
    public const int AiStageOneReactionMultiplierPercent = 100;
    public const int AiStageTwoReactionMultiplierPercent = 95;
    public const int AiStageThreeReactionMultiplierPercent = 90;
  }
}
