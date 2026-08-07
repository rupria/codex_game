using System;
using System.Collections.Generic;

namespace CodexGame.SmokeTests
{
  internal sealed class TestHarness
  {
    private readonly List<string> _failures = new List<string>();

    public void Check(bool condition, string message)
    {
      if (!condition)
      {
        _failures.Add(message);
      }
    }

    public void CheckThrows<TException>(Action action, string message)
      where TException : Exception
    {
      try
      {
        action();
        _failures.Add(message);
      }
      catch (TException)
      {
      }
    }

    public int Complete()
    {
      if (_failures.Count == 0)
      {
        Console.WriteLine("All CodexGame core smoke tests passed.");
        return 0;
      }

      foreach (var failure in _failures)
      {
        Console.Error.WriteLine($"FAIL: {failure}");
      }

      return 1;
    }
  }
}
