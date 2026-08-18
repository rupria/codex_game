#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using CodexGame.Application.Development;
using CodexGame.Core.Halli;

namespace CodexGame.Application.Logging
{
  public static class HalliAiBellCsvFormatter
  {
    public const string Header =
      "RecordedAtUtc,CombatRoundSeed,Sequence,Stage,CombatRound,FieldOpenedUs,BaseReactionUs,StageMultiplierPercent,FinalReactionUs,PlannedOutcome,Resolution,AiInputWasFirst,CheatUsed,CheatKinds";

    public static string Format(
      DateTime recordedAtUtc,
      long combatRoundSeed,
      HalliAiBellAuditEntry entry,
      bool cheatUsed,
      IReadOnlyList<CheatCommandEntry> cheatHistory)
    {
      if (entry == null) throw new ArgumentNullException(nameof(entry));
      if (cheatHistory == null) throw new ArgumentNullException(nameof(cheatHistory));
      var values = new[]
      {
        recordedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
        combatRoundSeed.ToString(CultureInfo.InvariantCulture),
        entry.Sequence.ToString(CultureInfo.InvariantCulture),
        entry.StageNumber.ToString(CultureInfo.InvariantCulture),
        entry.CombatRoundNumber.ToString(CultureInfo.InvariantCulture),
        entry.FieldOpenedAtMicroseconds.ToString(CultureInfo.InvariantCulture),
        entry.BaseReactionMicroseconds.ToString(CultureInfo.InvariantCulture),
        entry.StageMultiplierPercent.ToString(CultureInfo.InvariantCulture),
        entry.FinalReactionMicroseconds.ToString(CultureInfo.InvariantCulture),
        entry.PlannedOutcome.ToString(),
        entry.Resolution.ToString(),
        entry.AiInputWasFirst ? "true" : "false",
        cheatUsed ? "true" : "false",
        JoinCheatKinds(cheatHistory)
      };
      for (var index = 0; index < values.Length; index++) values[index] = Escape(values[index]);
      return string.Join(",", values);
    }

    private static string JoinCheatKinds(IReadOnlyList<CheatCommandEntry> history)
    {
      var unique = new List<string>();
      for (var index = 0; index < history.Count; index++)
      {
        var command = history[index].Command;
        if (!unique.Contains(command)) unique.Add(command);
      }
      return string.Join("|", unique);
    }

    private static string Escape(string value)
    {
      if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0) return value;
      return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
  }
}
