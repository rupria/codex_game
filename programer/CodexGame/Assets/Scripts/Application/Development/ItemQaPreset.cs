namespace CodexGame.Application.Development
{
  public enum ItemQaPreset
  {
    WildInkCompletesFlush = 0,
    WildInkCompletesStraightFlush = 1,
    WildInkRejectsJoker = 2,
    WildInkRejectsSameSuit = 3,
    WildInkLocksExchange = 4,
    BarrelBlocksNormalLoss = 5,
    BarrelDoesNotTriggerOnWin = 6,
    BarrelExcludedFromHandTimeout = 7,
    InsuranceCorrectPreservesCharge = 8,
    InsuranceCorrectsTwoWrong = 9,
    InsuranceCorrectsTwoSkipped = 10,
    InsuranceDoesNotCorrectThirdFailure = 11,
    InsuranceExcludedFromHandTimeout = 12,
    MercenaryNormalExchange = 13,
    MercenaryPreservesDominantSuit = 14,
    MercenaryNoReplacementPair = 15,
    MercenaryAiJokerHidden = 16,
    MercenaryRepeatSeed = 17,
    RestrictionZeroUses = 18,
    RestrictionOneUse = 19,
    RestrictionTwoUses = 20,
    FourNewItemsCombined = 21,
    CardPoolIntegrity = 22
  }
}
