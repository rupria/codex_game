using System;

namespace CodexGame.Application.Development
{
  public sealed class CheatCommandEntry
  {
    public CheatCommandEntry(long timestampMicroseconds, string command, string input, string result)
    {
      if (timestampMicroseconds < 0) throw new ArgumentOutOfRangeException(nameof(timestampMicroseconds));
      if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException("Command is required.", nameof(command));
      TimestampMicroseconds = timestampMicroseconds;
      Command = command;
      Input = input ?? string.Empty;
      Result = result ?? string.Empty;
    }

    public long TimestampMicroseconds { get; }
    public string Command { get; }
    public string Input { get; }
    public string Result { get; }
  }
}
