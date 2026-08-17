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
        "The runtime localization catalog must validate the full 0.1.2.5 key set.");
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
        "Bar and tavern-transition strings must load from the runtime catalog.");
      tests.Check(
        catalog.Get(
          "UI_BAR_REROLL_FREE",
          "ko",
          new LocalizationArgument("remaining", 1),
          new LocalizationArgument("cost", 0)) == "무료 재추첨 1/1"
          && catalog.Get("UI_BAR_REROLL_USED", "en") == "REROLL COMPLETE"
          && catalog.Get("UI_BAR_DUMMY_ITEM_06", "ko") == "진열 상품 F"
          && !catalog.Get("UI_BAR_DUMMY_ITEM_01", "ko").Contains("미확정")
          && !catalog.Get("UI_BAR_DUMMY_ITEM_01", "ko").Contains("개발 중"),
        "The bar shop must localize its one free reroll and preview-product labels without internal wording.");
      tests.Check(
        catalog.Get("UI_ITEM_WILD_INK", "ko") == "와일드 잉크"
          && catalog.Get("UI_ITEM_BARREL", "en") == "BARREL"
          && catalog.Get("UI_ITEM_PREDICTION_INSURANCE", "ko") == "예측 보험"
          && catalog.Get("UI_ITEM_MERCENARY", "en") == "MERCENARIES"
          && catalog.Get(
            "UI_ITEM_CONFIRM_TIMER",
            "ko",
            new LocalizationArgument("seconds", 120)) == "패 확정 120초",
        "The 0.1.2.5 item names, descriptions, and hand-confirm timer must be runtime-localized.");
      tests.Check(
        catalog.Get("UI_GUIDE_PAGE_FLOW_TITLE", "ko") == "목표와 게임 흐름"
          && catalog.Get("UI_GUIDE_PAGE_RESULT_TITLE", "en") == "PREDICTION & VICTORY"
          && catalog.Get(
            "UI_GUIDE_PAGE_INDICATOR",
            "ko",
            new LocalizationArgument("page", 2),
            new LocalizationArgument("total", 4)) == "2 / 4",
        "The four-page guide Keys must load from the same ko/en runtime catalog.");
      tests.Check(
        catalog.Get("UI_HALLI_LEFT_BELL", "ko") == "←  왼쪽"
          && catalog.Get("UI_HALLI_FLIP_ONE", "en") == "↑  FLIP 1"
          && catalog.Get("UI_HALLI_RIGHT_BELL", "ko") == "오른쪽  →"
          && !catalog.Get("UI_GUIDE_PAGE_HALLI_BODY", "en").Contains("Q/E")
          && !catalog.Get("UI_GUIDE_PAGE_HALLI_BODY", "en").Contains("W or"),
        "Visible gameplay guidance must use arrow controls instead of Q/W/E labels.");
      tests.Check(
        catalog.Get("UI_POKER_PLAYER_WINS", "ko") == "승리"
          && catalog.Get("UI_POKER_PLAYER_LOSES", "ko") == "패배"
          && catalog.Get("UI_POKER_PLAYER_WINS", "en") == "WIN"
          && catalog.Get("UI_POKER_PLAYER_LOSES", "en") == "LOSE",
        "Prediction medal labels must stay short enough to render inside the round buttons.");
      tests.Check(
        catalog.Get(
          "UI_POKER_RESULT_SUMMARY",
          "ko",
          new LocalizationArgument("winner", "플레이어"),
          new LocalizationArgument("playerHand", "원 페어"),
          new LocalizationArgument("aiHand", "투 페어"))
            == "승자: 플레이어\n플레이어 원 페어 vs AI 투 페어"
          && catalog.Get("UI_PREDICTION_CORRECT", "ko") == "예측 성공!"
          && catalog.Get("UI_PREDICTION_WRONG", "ko") == "예측 실패",
        "Poker result and prediction overlays must use concise, readable messages.");

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
