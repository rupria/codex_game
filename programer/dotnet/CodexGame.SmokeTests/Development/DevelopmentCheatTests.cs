using System;
using System.Collections.Generic;
using CodexGame.Application.Development;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;

namespace CodexGame.SmokeTests.Development
{
  internal static class DevelopmentCheatTests
  {
    public static void Run(TestHarness tests)
    {
      var history = new DevelopmentCheatHistory();
      for (var index = 0; index < 25; index++)
      {
        history.Record(index, "test", index.ToString(), "ok");
      }
      tests.Check(
        history.CheatUsed
          && history.Snapshot().Count == DevelopmentCheatHistory.MaximumEntries
          && history.Snapshot()[0].TimestampMicroseconds == 5,
        "Development cheats must set a watermark flag and retain only the latest 20 commands.");

      var presetsValid = true;
      foreach (PokerCheatPreset preset in Enum.GetValues(typeof(PokerCheatPreset)))
      {
        var setup = PokerCheatPresetCatalog.Create(preset);
        presetsValid &= setup.PlayerCards.Count == 3
          && setup.AiCards.Count == 3
          && setup.PublicCards.Count == 2
          && Unique(setup.PlayerCards, setup.AiCards, setup.PublicCards);
        try
        {
          PokerComparer.Compare(
            setup.PlayerCards,
            setup.AiCards,
            setup.PublicCards,
            PokerRuleSet.Development);
        }
        catch (Exception)
        {
          presetsValid = false;
        }
      }
      tests.Check(
        presetsValid,
        "Every guarded poker cheat preset must create eight unique and evaluable cards.");
    }

    private static bool Unique(params IReadOnlyList<Card>[] groups)
    {
      var ids = new HashSet<CardId>();
      for (var group = 0; group < groups.Length; group++)
      {
        for (var index = 0; index < groups[group].Count; index++)
        {
          if (!ids.Add(groups[group][index].Id)) return false;
        }
      }
      return true;
    }
  }
}
