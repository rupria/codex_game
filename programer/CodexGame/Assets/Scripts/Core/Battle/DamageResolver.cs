using System;
using CodexGame.Core.Poker;

namespace CodexGame.Core.Battle
{
  public static class DamageResolver
  {
    public const int PokerLossDamage = 1;

    public static BattleDamageResult ApplyPokerLoss(
      BattleHealth health,
      PokerWinner pokerWinner)
    {
      return ApplyPokerLoss(health, pokerWinner, false);
    }

    public static BattleDamageResult ApplyPokerLoss(
      BattleHealth health,
      PokerWinner pokerWinner,
      bool preventPlayerDamage)
    {
      if (!Enum.IsDefined(typeof(PokerWinner), pokerWinner))
      {
        throw new ArgumentOutOfRangeException(nameof(pokerWinner));
      }

      if (health.IsBattleOver)
      {
        throw new InvalidOperationException("Damage cannot be applied after the battle ends.");
      }

      var after = pokerWinner == PokerWinner.Player
        ? new BattleHealth(health.Player, Math.Max(0, health.Ai - PokerLossDamage))
        : preventPlayerDamage
          ? health
        : new BattleHealth(Math.Max(0, health.Player - PokerLossDamage), health.Ai);
      return new BattleDamageResult(
        pokerWinner,
        health,
        after,
        preventPlayerDamage && pokerWinner == PokerWinner.Ai ? 0 : PokerLossDamage);
    }
  }
}
