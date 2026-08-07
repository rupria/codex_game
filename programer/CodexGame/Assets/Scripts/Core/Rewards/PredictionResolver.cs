using System;
using CodexGame.Core.Poker;

namespace CodexGame.Core.Rewards
{
  public static class PredictionResolver
  {
    public static PredictionResult Resolve(
      PredictionChoice choice,
      PokerWinner actualWinner)
    {
      if (!Enum.IsDefined(typeof(PredictionChoice), choice))
      {
        throw new ArgumentOutOfRangeException(nameof(choice));
      }

      if (!Enum.IsDefined(typeof(PokerWinner), actualWinner))
      {
        throw new ArgumentOutOfRangeException(nameof(actualWinner));
      }

      var expectedWinner = choice == PredictionChoice.PlayerWins
        ? PokerWinner.Player
        : PokerWinner.Ai;
      return new PredictionResult(choice, actualWinner, expectedWinner == actualWinner);
    }
  }
}
