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
      int coinCount,
      long inactivityRemainingMicroseconds,
      PlayableTransitionSnapshot transition,
      PrototypeHalliSnapshot? halli,
      PrivateCardSelectionSnapshot? selection,
      PokerRoundSnapshot? poker)
    {
      Phase = phase;
      StageNumber = stageNumber;
      CombatRoundNumber = combatRoundNumber;
      Health = health;
      CoinCount = coinCount;
      InactivityRemainingMicroseconds = inactivityRemainingMicroseconds;
      Transition = transition;
      Halli = halli;
      Selection = selection;
      Poker = poker;
    }

    public PlayableGamePhase Phase { get; }
    public int StageNumber { get; }
    public int CombatRoundNumber { get; }
    public BattleHealth Health { get; }
    public int CoinCount { get; }
    public long InactivityRemainingMicroseconds { get; }
    public PlayableTransitionSnapshot Transition { get; }
    public PrototypeHalliSnapshot? Halli { get; }
    public PrivateCardSelectionSnapshot? Selection { get; }
    public PokerRoundSnapshot? Poker { get; }
  }
}
