using CodexGame.Application.Distribution;
using CodexGame.Application.Poker;
using CodexGame.Application.Shop;
using CodexGame.Application.Items;
using CodexGame.Application.Development;
using CodexGame.Core.Battle;
using CodexGame.Core.Rewards;
using CodexGame.Core.Items;
using System;
using System.Collections.Generic;

namespace CodexGame.Application.Playable
{
  public sealed class PlayableGameSnapshot
  {
    public PlayableGameSnapshot(
      PlayableGamePhase phase,
      int stageNumber,
      int combatRoundNumber,
      BattleHealth health,
      int baseBulletCount,
      int temporaryBulletCount,
      int lastStageReward,
      int lastStageBaseReward,
      int lastStageBonusReward,
      int predictionSuccessCount,
      IReadOnlyList<GameItemId> inventory,
      bool cheatUsed,
      IReadOnlyList<CheatCommandEntry> cheatHistory,
      long inactivityRemainingMicroseconds,
      bool inactivityReturnPending,
      StageItemRestrictionSnapshot stageItemRestriction,
      PlayableTransitionSnapshot transition,
      NextStageTransitionSnapshot? nextStageTransition,
      PrototypeHalliSnapshot? halli,
      PrivateCardSelectionSnapshot? selection,
      PokerItemSnapshot? pokerItems,
      PokerRoundSnapshot? poker,
      BarShopSnapshot? barShop)
    {
      Phase = phase;
      StageNumber = stageNumber;
      CombatRoundNumber = combatRoundNumber;
      Health = health;
      if (baseBulletCount < 0) throw new ArgumentOutOfRangeException(nameof(baseBulletCount));
      if (temporaryBulletCount < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(temporaryBulletCount));
      }
      BaseBulletCount = baseBulletCount;
      TemporaryBulletCount = temporaryBulletCount;
      LastStageReward = lastStageReward;
      LastStageBaseReward = lastStageBaseReward;
      LastStageBonusReward = lastStageBonusReward;
      PredictionSuccessCount = predictionSuccessCount;
      Inventory = Copy(inventory);
      CheatUsed = cheatUsed;
      CheatHistory = Copy(cheatHistory);
      InactivityRemainingMicroseconds = inactivityRemainingMicroseconds;
      InactivityReturnPending = inactivityReturnPending;
      StageItemRestriction = stageItemRestriction
        ?? throw new ArgumentNullException(nameof(stageItemRestriction));
      Transition = transition;
      NextStageTransition = nextStageTransition;
      Halli = halli;
      Selection = selection;
      PokerItems = pokerItems;
      Poker = poker;
      BarShop = barShop;
    }

    public PlayableGamePhase Phase { get; }
    public int StageNumber { get; }
    public int CombatRoundNumber { get; }
    public BattleHealth Health { get; }
    public int BaseBulletCount { get; }
    public int TemporaryBulletCount { get; }
    public int BulletCount => BaseBulletCount + TemporaryBulletCount;
    public int LastStageReward { get; }
    public int LastStageBaseReward { get; }
    public int LastStageBonusReward { get; }
    public int PredictionSuccessCount { get; }
    public IReadOnlyList<GameItemId> Inventory { get; }
    public bool CheatUsed { get; }
    public IReadOnlyList<CheatCommandEntry> CheatHistory { get; }
    public long InactivityRemainingMicroseconds { get; }
    public bool InactivityReturnPending { get; }
    public StageItemRestrictionSnapshot StageItemRestriction { get; }
    public PlayableTransitionSnapshot Transition { get; }
    public NextStageTransitionSnapshot? NextStageTransition { get; }
    public PrototypeHalliSnapshot? Halli { get; }
    public PrivateCardSelectionSnapshot? Selection { get; }
    public PokerItemSnapshot? PokerItems { get; }
    public PokerRoundSnapshot? Poker { get; }
    public BarShopSnapshot? BarShop { get; }

    private static IReadOnlyList<GameItemId> Copy(IReadOnlyList<GameItemId> source)
    {
      if (source == null) throw new ArgumentNullException(nameof(source));
      var result = new GameItemId[source.Count];
      for (var index = 0; index < result.Length; index++) result[index] = source[index];
      return Array.AsReadOnly(result);
    }

    private static IReadOnlyList<CheatCommandEntry> Copy(IReadOnlyList<CheatCommandEntry> source)
    {
      if (source == null) throw new ArgumentNullException(nameof(source));
      var result = new CheatCommandEntry[source.Count];
      for (var index = 0; index < result.Length; index++) result[index] = source[index];
      return Array.AsReadOnly(result);
    }
  }
}
