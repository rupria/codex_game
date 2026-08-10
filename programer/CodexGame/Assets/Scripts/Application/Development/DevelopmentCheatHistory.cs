using System;
using System.Collections.Generic;

namespace CodexGame.Application.Development
{
  public sealed class DevelopmentCheatHistory
  {
    public const int MaximumEntries = 20;
    private readonly List<CheatCommandEntry> _entries = new List<CheatCommandEntry>();

    public bool CheatUsed { get; private set; }

    public void Record(long timestampMicroseconds, string command, string input, string result)
    {
      CheatUsed = true;
      _entries.Add(new CheatCommandEntry(timestampMicroseconds, command, input, result));
      if (_entries.Count > MaximumEntries) _entries.RemoveAt(0);
    }

    public IReadOnlyList<CheatCommandEntry> Snapshot()
    {
      return Array.AsReadOnly(_entries.ToArray());
    }

    public void Reset()
    {
      CheatUsed = false;
      _entries.Clear();
    }
  }
}
