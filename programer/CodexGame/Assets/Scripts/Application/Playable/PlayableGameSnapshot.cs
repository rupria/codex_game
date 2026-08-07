using CodexGame.Application.Distribution;
using CodexGame.Application.Poker;
using CodexGame.Core.Battle;
using CodexGame.Core.Rewards;

namespace CodexGame.Application.Playable
{
  public sealed class PlayableGameSnapshot
  {
    public PlayableGameSnapshot(
      PlayableGamePhase phase,
      int stageNumber,
      int combatRoundNumber,
      BattleHealth health,
      int itemRewardCount,
      int coinIncreaseEventCount,
      int lastStageBaseCoinReward,
      PredictionRewardKind lastPredictionReward,
      long inactivityRemainingMicroseconds,
      PrototypeHalliSnapshot? halli,
      PrivateCardSelectionSnapshot? selection,
      PokerRoundSnapshot? poker)
    {
      Phase = phase;
      StageNumber = stageNumber;
      CombatRoundNumber = combatRoundNumber;
      Health = health;
      ItemRewardCount = itemRewardCount;
      CoinIncreaseEventCount = coinIncreaseEventCount;
      LastStageBaseCoinReward = lastStageBaseCoinReward;
      LastPredictionReward = lastPredictionReward;
      InactivityRemainingMicroseconds = inactivityRemainingMicroseconds;
      Halli = halli;
      Selection = selection;
      Poker = poker;
    }

    public PlayableGamePhase Phase { get; }
    public int StageNumber { get; }
    public int CombatRoundNumber { get; }
    public BattleHealth Health { get; }
    public int ItemRewardCount { get; }
    public int CoinIncreaseEventCount { get; }
    public int LastStageBaseCoinReward { get; }
    public PredictionRewardKind LastPredictionReward { get; }
    public long InactivityRemainingMicroseconds { get; }
    public PrototypeHalliSnapshot? Halli { get; }
    public PrivateCardSelectionSnapshot? Selection { get; }
    public PokerRoundSnapshot? Poker { get; }
  }
}
