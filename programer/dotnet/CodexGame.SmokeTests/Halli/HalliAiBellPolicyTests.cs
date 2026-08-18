using System;
using System.Collections.Generic;
using CodexGame.Application.Development;
using CodexGame.Application.Logging;
using CodexGame.Core.Cards;
using CodexGame.Core.Halli;
using CodexGame.Core.Shared;

namespace CodexGame.SmokeTests.Halli
{
  internal static class HalliAiBellPolicyTests
  {
    public static void Run(TestHarness tests)
    {
      var policy = new HalliAiBellPolicy();
      tests.Check(
        policy.CreateReactionDelay(new SequenceRandom(0, 0)) == 1_000_000
          && policy.CreateReactionDelay(new SequenceRandom(60, 0)) == 2_000_000
          && policy.CreateReactionDelay(new SequenceRandom(90, 5_000_000)) == 10_000_000,
        "AI reaction sampling must use the fixed 60/30/10 weighted 1-2/2-5/5-10 second bands.");
      tests.Check(
        HalliAiBellPolicy.ApplyStageMultiplier(10_000_000, 100) == 10_000_000
          && HalliAiBellPolicy.ApplyStageMultiplier(10_000_000, 95) == 9_500_000
          && HalliAiBellPolicy.ApplyStageMultiplier(10_000_000, 90) == 9_000_000
          && HalliAiBellPolicy.ApplyStageMultiplier(1_000_000, 90) == 1_000_000,
        "Stage reaction multipliers must be 100/95/90 percent with a one-second floor.");
      tests.Check(
        HalliAiBellPolicy.ConditionalMissPercent(1_500_000) == 10
          && HalliAiBellPolicy.ConditionalMissPercent(3_000_000) == 30
          && HalliAiBellPolicy.ConditionalMissPercent(8_000_000) == 50,
        "Conditional miss rates must follow the sampled reaction band.");

      var correct = policy.Decide(
        true,
        false,
        2_000_000,
        2,
        _ => 1,
        new SequenceRandom(10),
        new SequenceRandom(0, 0));
      tests.Check(
        correct.Outcome == AiBellOutcome.Correct
          && correct.BaseReactionDelayMicroseconds == 2_000_000
          && correct.StageMultiplierPercent == 95
          && correct.ReactionDelayMicroseconds == 1_900_000,
        "A non-miss AI result must retain base, multiplier, and final reaction values for logging.");

      var audit = new HalliAiBellAuditEntry(
        1,
        2,
        3,
        10,
        2_000_000,
        95,
        1_900_000,
        AiBellOutcome.Correct,
        HalliAiBellResolution.AiInputFirst,
        true);
      var csv = HalliAiBellCsvFormatter.Format(
        new DateTime(2026, 8, 19, 0, 0, 0, DateTimeKind.Utc),
        7293618,
        audit,
        true,
        Array.AsReadOnly(new[]
        {
          new CheatCommandEntry(1, "stage-pass", string.Empty, "ok"),
          new CheatCommandEntry(2, "grant-item", string.Empty, "ok")
        }));
      tests.Check(
        csv.Contains(",2000000,95,1900000,Correct,AiInputFirst,true,true,stage-pass|grant-item"),
        "Local AI telemetry must include timing, outcome, actual first input, and cheat classification without a server dependency.");
    }

    private sealed class SequenceRandom : IRandomSource
    {
      private readonly Queue<int> _values;

      public SequenceRandom(params int[] values)
      {
        _values = new Queue<int>(values);
      }

      public int NextInt(int maxExclusive)
      {
        if (_values.Count == 0) throw new InvalidOperationException("No random value remains.");
        var value = _values.Dequeue();
        if (value < 0 || value >= maxExclusive) throw new InvalidOperationException("Random value is out of range.");
        return value;
      }
    }
  }
}
