#nullable enable
using System;
using System.IO;
using System.Text;
using CodexGame.Application.Logging;
using CodexGame.Application.Playable;
using UnityEngine;

namespace CodexGame.Bootstrap
{
  internal sealed class HalliAiBellLocalLogWriter
  {
    private long _lastCombatRoundSeed = long.MinValue;
    private int _lastSequence;

    public void TryWrite(PlayableGameSnapshot snapshot)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || ENABLE_GAMEPLAY_CHEATS
      if (snapshot?.Halli?.LastAiBellAuditEntry == null) return;
      var halli = snapshot.Halli;
      var entry = halli.LastAiBellAuditEntry;
      if (_lastCombatRoundSeed == halli.CombatRoundSeed && _lastSequence == entry.Sequence) return;

      try
      {
        var directory = Path.Combine(UnityEngine.Application.persistentDataPath, "CodexGameLogs");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "halli_ai_bell.csv");
        var includeHeader = !File.Exists(path) || new FileInfo(path).Length == 0;
        using (var writer = new StreamWriter(path, true, new UTF8Encoding(false)))
        {
          if (includeHeader) writer.WriteLine(HalliAiBellCsvFormatter.Header);
          writer.WriteLine(HalliAiBellCsvFormatter.Format(
            DateTime.UtcNow,
            halli.CombatRoundSeed,
            entry,
            snapshot.CheatUsed,
            snapshot.CheatHistory));
        }
        _lastCombatRoundSeed = halli.CombatRoundSeed;
        _lastSequence = entry.Sequence;
      }
      catch (Exception exception)
      {
        Debug.LogWarning("Local Halli AI log could not be written: " + exception.Message);
      }
#endif
    }
  }
}
