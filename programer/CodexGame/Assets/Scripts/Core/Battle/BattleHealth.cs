using System;
using CodexGame.Core.Shared;

namespace CodexGame.Core.Battle
{
  public readonly struct BattleHealth
  {
    public BattleHealth(int player, int ai)
    {
      if (player < 0 || player > GameRules.StartingHealth)
      {
        throw new ArgumentOutOfRangeException(nameof(player));
      }

      if (ai < 0 || ai > GameRules.StartingHealth)
      {
        throw new ArgumentOutOfRangeException(nameof(ai));
      }

      Player = player;
      Ai = ai;
    }

    public int Player { get; }
    public int Ai { get; }
    public bool IsBattleOver => Player == 0 || Ai == 0;

    public static BattleHealth Initial => new BattleHealth(
      GameRules.StartingHealth,
      GameRules.StartingHealth);
  }
}
