using System;
using CodexGame.Core.Battle;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;

namespace CodexGame.Application.Poker
{
  public sealed class PokerRoundResult
  {
    public PokerRoundResult(
      PokerComparisonResult comparison,
      BattleDamageResult damage,
      PredictionResult prediction)
    {
      Comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
      Damage = damage ?? throw new ArgumentNullException(nameof(damage));
      Prediction = prediction ?? throw new ArgumentNullException(nameof(prediction));
    }

    public PokerComparisonResult Comparison { get; }
    public BattleDamageResult Damage { get; }
    public PredictionResult Prediction { get; }
  }
}
