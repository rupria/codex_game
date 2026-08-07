using System;
using CodexGame.Core.Poker;

namespace CodexGame.Core.Rewards
{
  public sealed class PredictionResult
  {
    public PredictionResult(
      PredictionChoice choice,
      PokerWinner actualWinner,
      bool isCorrect)
    {
      if (!Enum.IsDefined(typeof(PredictionChoice), choice))
      {
        throw new ArgumentOutOfRangeException(nameof(choice));
      }

      if (!Enum.IsDefined(typeof(PokerWinner), actualWinner))
      {
        throw new ArgumentOutOfRangeException(nameof(actualWinner));
      }

      Choice = choice;
      ActualWinner = actualWinner;
      IsCorrect = isCorrect;
    }

    public PredictionChoice Choice { get; }
    public PokerWinner ActualWinner { get; }
    public bool IsCorrect { get; }
  }
}
