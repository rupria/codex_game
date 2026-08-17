using System;
using CodexGame.Core.Poker;

namespace CodexGame.Core.Battle
{
  public sealed class BattleDamageResult
  {
    public BattleDamageResult(
      PokerWinner pokerWinner,
      BattleHealth before,
      BattleHealth after,
      int damage)
    {
      if (!Enum.IsDefined(typeof(PokerWinner), pokerWinner))
      {
        throw new ArgumentOutOfRangeException(nameof(pokerWinner));
      }

      if (damage < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(damage));
      }

      PokerWinner = pokerWinner;
      Before = before;
      After = after;
      Damage = damage;
    }

    public PokerWinner PokerWinner { get; }
    public BattleHealth Before { get; }
    public BattleHealth After { get; }
    public int Damage { get; }
  }
}
