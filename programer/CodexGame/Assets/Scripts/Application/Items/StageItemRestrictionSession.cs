using System;
using CodexGame.Core.Cards;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Items
{
  public sealed class StageItemRestrictionSession
  {
    private int _stageNumber;
    private bool _wasAssessed;
    private bool _activatedDuringRun;
    private bool _isActive;
    private int _useLimit;
    private int _usedCount;

    public void ResetRun()
    {
      _stageNumber = 0;
      _wasAssessed = false;
      _activatedDuringRun = false;
      _isActive = false;
      _useLimit = 0;
      _usedCount = 0;
    }

    public StageItemRestrictionSnapshot EnterStage(int stageNumber, long stageSeed)
    {
      if (stageNumber < 1) throw new ArgumentOutOfRangeException(nameof(stageNumber));
      if (_stageNumber == stageNumber && _wasAssessed) return GetSnapshot();
      if (_stageNumber > stageNumber)
      {
        throw new InvalidOperationException("Stage item restriction cannot move backwards.");
      }

      _stageNumber = stageNumber;
      _wasAssessed = true;
      _isActive = false;
      _useLimit = 0;
      _usedCount = 0;

      if (stageNumber == 1
        || stageNumber > GameRules.InitialStageCount
        || _activatedDuringRun) return GetSnapshot();

      var random = DeterministicRandomFactory.Create(stageSeed, RandomChannel.StageItemRestriction);
      if (random.NextInt(100) >= GameRules.StageItemRestrictionActivationPercent)
      {
        return GetSnapshot();
      }

      _isActive = true;
      _activatedDuringRun = true;
      _useLimit = GameRules.StageItemRestrictionMinimumUses + random.NextInt(
        GameRules.StageItemRestrictionMaximumUses
          - GameRules.StageItemRestrictionMinimumUses + 1);
      return GetSnapshot();
    }

    public bool CanUse => !_isActive || _usedCount < _useLimit;

    internal void ConfigureQaOverride(int stageNumber, int useLimit)
    {
      if (stageNumber < 1) throw new ArgumentOutOfRangeException(nameof(stageNumber));
      if (useLimit < 0 || useLimit > GameRules.StageItemRestrictionMaximumUses)
      {
        throw new ArgumentOutOfRangeException(nameof(useLimit));
      }
      _stageNumber = stageNumber;
      _wasAssessed = true;
      _activatedDuringRun = true;
      _isActive = true;
      _useLimit = useLimit;
      _usedCount = 0;
    }

    public void RecordUse()
    {
      if (!CanUse)
      {
        throw new InvalidOperationException("The stage item-use limit is exhausted.");
      }
      if (_isActive) _usedCount++;
    }

    public StageItemRestrictionSnapshot GetSnapshot()
    {
      return new StageItemRestrictionSnapshot(
        _stageNumber,
        _wasAssessed,
        _isActive,
        _useLimit,
        _usedCount);
    }
  }
}
