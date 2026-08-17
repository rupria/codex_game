namespace CodexGame.Application.Items
{
  public sealed class StageItemRestrictionSnapshot
  {
    public StageItemRestrictionSnapshot(
      int stageNumber,
      bool wasAssessed,
      bool isActive,
      int useLimit,
      int usedCount)
    {
      StageNumber = stageNumber;
      WasAssessed = wasAssessed;
      IsActive = isActive;
      UseLimit = useLimit;
      UsedCount = usedCount;
    }

    public int StageNumber { get; }
    public bool WasAssessed { get; }
    public bool IsActive { get; }
    public int UseLimit { get; }
    public int UsedCount { get; }
    public int RemainingUses => IsActive ? System.Math.Max(0, UseLimit - UsedCount) : 0;
    public bool IsExhausted => IsActive && RemainingUses == 0;
  }
}
