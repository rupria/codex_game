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

      var jokerAward = new JokerAwardCheatState();
      tests.Check(
        !jokerAward.IsGuaranteed
          && jokerAward.EffectiveAwardPercent == 10
          && jokerAward.SetGuaranteed(true)
          && jokerAward.IsGuaranteed
          && jokerAward.EffectiveAwardPercent == 100
          && !jokerAward.SetGuaranteed(true)
          && jokerAward.SetGuaranteed(false)
          && jokerAward.EffectiveAwardPercent == 10,
        "The Joker award cheat must toggle between the default 10% and guaranteed 100% without stacking.");

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
      tests.Check(
        ContainsJoker(PokerCheatPresetCatalog.Create(PokerCheatPreset.PlayerJoker).PlayerCards)
          && ContainsJoker(PokerCheatPresetCatalog.Create(PokerCheatPreset.AiJoker).AiCards)
          && !ContainsJoker(PokerCheatPresetCatalog.Create(PokerCheatPreset.PlayerJokerIneligible).PlayerCards)
          && !ContainsJoker(PokerCheatPresetCatalog.Create(PokerCheatPreset.PlayerJokerNotAwarded).PlayerCards)
          && !ContainsJoker(PokerCheatPresetCatalog.Create(PokerCheatPreset.AiJokerIneligible).AiCards)
          && !ContainsJoker(PokerCheatPresetCatalog.Create(PokerCheatPreset.AiJokerNotAwarded).AiCards),
        "QA presets must cover player/AI Joker ineligible, not-awarded, and awarded downstream states.");

      var itemPresetsPassed = true;
      var itemPresetsDeterministic = true;
      foreach (ItemQaPreset preset in Enum.GetValues(typeof(ItemQaPreset)))
      {
        var first = ItemQaPresetRunner.Run(preset, 441_082, 2, "PokerItems");
        var second = ItemQaPresetRunner.Run(preset, 441_082, 2, "PokerItems");
        itemPresetsPassed &= first.Passed;
        itemPresetsDeterministic &= first.Passed == second.Passed
          && first.PlayerHand == second.PlayerHand
          && first.AiHand == second.AiHand
          && first.PublicCards == second.PublicCards
          && first.Actual == second.Actual;
        tests.Check(first.Passed, preset + " item QA preset failed: " + first.Summary);
      }
      tests.Check(
        itemPresetsPassed && itemPresetsDeterministic,
        "Every 0.1.2.5 item QA preset must pass once and repeat deterministically for the same seed.");
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

    private static bool ContainsJoker(IReadOnlyList<Card> cards)
    {
      for (var index = 0; index < cards.Count; index++)
      {
        if (cards[index].IsJoker) return true;
      }
      return false;
    }
  }
}
