using CodexGame.Application.Distribution;
using CodexGame.Application.Poker;
using CodexGame.Core.Battle;

namespace CodexGame.Application.Playable
{
  public sealed class PlayableGameSnapshot
  {
    public PlayableGameSnapshot(
      PlayableGamePhase phase,
      int combatRoundNumber,
      BattleHealth health,
      PrototypeHalliSnapshot? halli,
      PrivateCardSelectionSnapshot? selection,
      PokerRoundSnapshot? poker)
    {
      Phase = phase;
      CombatRoundNumber = combatRoundNumber;
      Health = health;
      Halli = halli;
      Selection = selection;
      Poker = poker;
    }

    public PlayableGamePhase Phase { get; }
    public int CombatRoundNumber { get; }
    public BattleHealth Health { get; }
    public PrototypeHalliSnapshot? Halli { get; }
    public PrivateCardSelectionSnapshot? Selection { get; }
    public PokerRoundSnapshot? Poker { get; }
  }
}
