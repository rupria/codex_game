using System;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public sealed class HalliTurnOrder
  {
    private bool _playerNextIsLeft = true;
    private bool _aiNextIsLeft = true;

    public HalliActor LeadActor { get; private set; } = HalliActor.Player;

    public void SetLead(HalliActor actor)
    {
      if (!Enum.IsDefined(typeof(HalliActor), actor))
      {
        throw new ArgumentOutOfRangeException(nameof(actor));
      }

      LeadActor = actor;
    }

    public HalliActor GetFollower()
    {
      return LeadActor == HalliActor.Player ? HalliActor.Ai : HalliActor.Player;
    }

    public PileSide TakeNextPile(HalliActor actor)
    {
      if (actor == HalliActor.Player)
      {
        var pile = _playerNextIsLeft ? PileSide.Left : PileSide.Right;
        _playerNextIsLeft = !_playerNextIsLeft;
        return pile;
      }

      if (actor == HalliActor.Ai)
      {
        // AI-relative left is the player's physical right side.
        var pile = _aiNextIsLeft ? PileSide.Right : PileSide.Left;
        _aiNextIsLeft = !_aiNextIsLeft;
        return pile;
      }

      throw new ArgumentOutOfRangeException(nameof(actor));
    }
  }
}
