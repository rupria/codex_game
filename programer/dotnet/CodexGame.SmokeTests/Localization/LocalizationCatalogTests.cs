using System;
using System.Collections.Generic;
using System.IO;
using CodexGame.Application.Playable;
using CodexGame.Presentation.Localization;

namespace CodexGame.SmokeTests.Localization
{
  internal static class LocalizationCatalogTests
  {
    public static void Run(TestHarness tests)
    {
      var csvPath = Path.Combine(AppContext.BaseDirectory, "Localization", "ui_strings.csv");
      var warnings = new List<string>();
      var catalog = LocalizationCatalog.Parse(File.ReadAllText(csvPath), warnings.Add);
      tests.Check(
        catalog.Count == LocalizationCatalog.RequiredKeyCount,
        "The runtime localization catalog must validate all 137 keys.");
      tests.Check(
        catalog.Get("UI_MAIN_START", "ko") == "시작"
          && catalog.Get("UI_MAIN_START", "en") == "START",
        "The same Key must switch immediately between Korean and English values.");
      tests.Check(
        catalog.Get(
          "UI_HUD_STAGE",
          "ko",
          new LocalizationArgument("stage", 3)) == "스테이지 3",
        "Named localization placeholders must resolve without changing language-specific order.");
      tests.Check(
        catalog.Get(
          "UI_HUD_REWARDS",
          "ko",
          new LocalizationArgument("bullets", 4)) == "총알 4"
          && catalog.Get(
            "UI_STAGE_CLEAR",
            "en",
            new LocalizationArgument("reward", 3)) == "STAGE CLEAR · BULLETS +3",
        "Public reward strings must use bullets and the settled stage reward.");
      tests.Check(
        catalog.Get("UI_BAR_TITLE", "ko") == "바"
          && catalog.Get("UI_TRANSITION_OPEN_SALOON_DOOR", "en")
            == "PUSHING THROUGH THE SALOON DOORS",
        "Bar and tavern-transition strings must load from the 137-key catalog.");
      tests.Check(
        catalog.Get("UI_GUIDE_PAGE_FLOW_TITLE", "ko") == "목표와 게임 흐름"
          && catalog.Get("UI_GUIDE_PAGE_RESULT_TITLE", "en") == "PREDICTION & VICTORY"
          && catalog.Get(
            "UI_GUIDE_PAGE_INDICATOR",
            "ko",
            new LocalizationArgument("page", 2),
            new LocalizationArgument("total", 4)) == "2 / 4",
        "The four-page guide Keys must load from the same ko/en runtime catalog.");

      var status = new LocalizedStatus(
        "STATUS_HALLI_DISTRIBUTING",
        new LocalizedStatusArgument("step", "2"),
        new LocalizedStatusArgument("actor", "UI_ACTOR_AI", true),
        new LocalizedStatusArgument("side", "UI_SIDE_LEFT", true));
      tests.Check(
        catalog.Get(status, "ko") == "카드 2/4 배분 중: AI 왼쪽.",
        "Application status arguments that are Keys must localize before sentence formatting.");

      var firstMissing = catalog.Get("UI_NOT_REGISTERED", "ko");
      var secondMissing = catalog.Get("UI_NOT_REGISTERED", "en");
      tests.Check(
        firstMissing == "[MISSING:UI_NOT_REGISTERED]"
          && secondMissing == firstMissing
          && warnings.Count == 1,
        "Missing Keys must fall back to a visible marker and warn only once.");

      tests.CheckThrows<FormatException>(
        () => LocalizationCatalog.Parse("Key,ko,en\nA,{value},{other}\n"),
        "A ko/en placeholder mismatch must fail validation before build.");
    }
  }
}
