using System;
using System.IO;
using CodexGame.Bootstrap;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Views;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodexGame.Editor
{
  public static class PlayableDevSceneBuilder
  {
    public const string ScenePath = "Assets/Scenes/PlayableDev.unity";
    public const string BoardArtPath =
      "Assets/Art/Prototype/Board/halli_western_round_table_unlit.png";
    private const string UiArtRoot = "Assets/Art/Prototype/UI/";
    private const string HalliUiArtRoot = UiArtRoot + "Halli_0_1_0/";
    private const string HalliUi021ArtRoot = UiArtRoot + "Halli_0_2_1/";
    private const string HalliUi037ArtRoot = UiArtRoot + "Halli_0_3_7/";
    private const string PokerUiArtRoot = UiArtRoot + "Poker_0_2_2/";
    private const string PokerUi034ArtRoot = UiArtRoot + "Poker_0_3_4/";
    private const string PokerUi036ArtRoot = UiArtRoot + "Poker_0_3_6/";
    private const string PokerUi037ArtRoot = UiArtRoot + "Poker_0_3_7/";
    private const string GameplayUi012ArtRoot = UiArtRoot + "Gameplay_0_1_2/";
    private const string BarShopUiArtRoot = UiArtRoot + "BarShop_0_3_0/";
    private const string BarShopUi034ArtRoot = UiArtRoot + "BarShop_0_3_4/";
    private const string BarShopUi038ArtRoot = UiArtRoot + "BarShop_0_3_8/";
    private const string EconomyUi012ArtRoot = UiArtRoot + "Economy_0_1_2/";
    private const string StageTransitionUiArtRoot = UiArtRoot + "StageTransition_0_3_1/";
    private const string IntroArtPath = HalliUiArtRoot + "start_screen_background.png";
    private const string BackdropShaderPath = "Assets/Shaders/RuntimeBackdropLit.shader";

    [MenuItem("Codex Game/Playable Dev/Create Scene")]
    public static void CreateScene()
    {
      EnsureScenesFolder();
      var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

      var cameraObject = new GameObject("Main Camera");
      cameraObject.tag = "MainCamera";
      var camera = cameraObject.AddComponent<Camera>();
      camera.clearFlags = CameraClearFlags.SolidColor;
      camera.backgroundColor = new Color(0.035f, 0.055f, 0.08f, 1f);
      camera.transform.position = new Vector3(0f, 0f, -10f);

      var gameObject = new GameObject("CodexGame.PlayableDev");
      var boardTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(BoardArtPath);

      if (boardTexture == null)
      {
        throw new FileNotFoundException("Playable board art was not found.", BoardArtPath);
      }

      var view = gameObject.AddComponent<PlayableDevView>();
      var introTexture = LoadTexture(IntroArtPath);
      PlayableCardArtSet cardArtSet = PlayableCardArtLoader.Load();
      var halliUiArtSet = new HalliUiArtSet(
        LoadTexture(UiArtRoot + "bell_idle.png"),
        LoadTexture(UiArtRoot + "bell_hover.png"),
        LoadTexture(UiArtRoot + "bell_pressed.png"),
        LoadTexture(UiArtRoot + "bell_wrong.png"),
        LoadTexture(HalliUiArtRoot + "bell_correct.png"),
        LoadTexture(HalliUiArtRoot + "bell_disabled.png"),
        LoadTexture(UiArtRoot + "public_card_locked_slot.png"),
        LoadTexture(UiArtRoot + "flip_timer.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_idle.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_hover.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_pressed.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_disabled.png"),
        LoadTexture(HalliUiArtRoot + "player_acquired_tray.png"),
        LoadTexture(HalliUiArtRoot + "ai_acquired_status_panel.png"),
        LoadTexture(HalliUi021ArtRoot + "round_win_pip_player_empty_32_0_2_1.png"),
        LoadTexture(HalliUi021ArtRoot + "round_win_pip_player_filled_32_0_2_1.png"),
        LoadTexture(HalliUi021ArtRoot + "round_win_pip_ai_empty_32_0_2_1.png"),
        LoadTexture(HalliUi021ArtRoot + "round_win_pip_ai_filled_32_0_2_1.png"),
        sharedPileRailIdle: LoadTexture(
          HalliUi037ArtRoot + "halli_shared_pile_rail_idle_140x136_0_3_7.png"),
        sharedPileRailPlayerActive: LoadTexture(
          HalliUi037ArtRoot + "halli_shared_pile_rail_player_active_140x136_0_3_7.png"),
        sharedPileRailAiActive: LoadTexture(
          HalliUi037ArtRoot + "halli_shared_pile_rail_ai_active_140x136_0_3_7.png"));
      var guideUiArtSet = new GuideUiArtSet(
        LoadTexture(HalliUiArtRoot + "guide_modal_background.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_flow_art.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_halli_art.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_cards_art.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_result_art.png"),
        LoadTexture(HalliUiArtRoot + "guide_nav_button_idle.png"),
        LoadTexture(HalliUiArtRoot + "guide_nav_button_hover.png"),
        LoadTexture(HalliUiArtRoot + "guide_nav_button_disabled.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_indicator_plate.png"));
      var healthUiArtSet = new HealthUiArtSet(
        LoadTexture(PokerUiArtRoot + "hp_heart_player_filled_24_0_2_2.png"),
        LoadTexture(PokerUiArtRoot + "hp_heart_player_empty_24_0_2_2.png"),
        LoadTexture(PokerUiArtRoot + "hp_heart_ai_filled_24_0_2_2.png"),
        LoadTexture(PokerUiArtRoot + "hp_heart_ai_empty_24_0_2_2.png"));
      var pokerUiArtSet = new PokerUiArtSet(
        LoadTexture(PokerUiArtRoot + "poker_predict_win_idle_64_0_2_2.png"),
        LoadTexture(PokerUiArtRoot + "poker_predict_win_hover_64_0_2_2.png"),
        LoadTexture(PokerUiArtRoot + "poker_predict_lose_idle_64_0_2_2.png"),
        LoadTexture(PokerUiArtRoot + "poker_predict_lose_hover_64_0_2_2.png"),
        LoadTexture(UiArtRoot + "item_slot.png"));
      var pokerItemUiArtSet = new PokerItemUiArtSet(
        LoadTexture(PokerUi034ArtRoot + "poker_item_crate_closed_160x160_0_3_4.png"),
        LoadTexture(PokerUi034ArtRoot + "poker_item_crate_open_empty_160x160_0_3_4.png"),
        LoadTexture(PokerUi034ArtRoot + "poker_item_crate_open_filled_160x160_0_3_4.png"),
        LoadTexture(PokerUi036ArtRoot + "poker_item_popup_frame_560x300_0_3_6.png"),
        LoadTexture(PokerUi036ArtRoot + "poker_item_inventory_tray_388x92_0_3_6.png"),
        LoadTexture(GameplayUi012ArtRoot + "inventory_slot_72_idle_0_1_2.png"),
        LoadTexture(GameplayUi012ArtRoot + "inventory_slot_72_hover_0_1_2.png"),
        LoadTexture(GameplayUi012ArtRoot + "inventory_slot_72_selected_0_1_2.png"),
        LoadTexture(GameplayUi012ArtRoot + "inventory_slot_72_disabled_0_1_2.png"),
        LoadTexture(GameplayUi012ArtRoot + "item_reload_64_0_1_2.png"),
        LoadTexture(GameplayUi012ArtRoot + "item_bottom_deal_64_0_1_2.png"),
        LoadTexture(GameplayUi012ArtRoot + "item_hype_man_64_0_1_2.png"),
        LoadTexture(GameplayUi012ArtRoot + "item_heal_tonic_64_0_1_2.png"),
        LoadTexture(PokerUi037ArtRoot + "poker_item_select_panel_640x336_0_3_7.png"),
        LoadTexture(PokerUi037ArtRoot + "poker_item_detail_panel_376x112_0_3_7.png"),
        LoadTexture(PokerUi037ArtRoot + "poker_item_action_button_idle_172x44_0_3_7.png"),
        LoadTexture(PokerUi037ArtRoot + "poker_item_action_button_hover_172x44_0_3_7.png"),
        LoadTexture(PokerUi037ArtRoot + "poker_item_action_button_disabled_172x44_0_3_7.png"));
      var barShopUiArtSet = new BarShopUiArtSet(
        LoadTexture(BarShopUiArtRoot + "bar_shop_background_unlit_960x540_0_3_0.png"),
        LoadTexture(BarShopUiArtRoot + "bar_shop_product_slot_230x210_0_3_0.png"),
        LoadTexture(BarShopUi034ArtRoot + "bar_shop_reroll_idle_180x56_0_3_4.png"),
        LoadTexture(BarShopUi034ArtRoot + "bar_shop_reroll_hover_180x56_0_3_4.png"),
        LoadTexture(BarShopUi034ArtRoot + "bar_shop_reroll_pressed_180x56_0_3_4.png"),
        LoadTexture(BarShopUi034ArtRoot + "bar_shop_reroll_disabled_180x56_0_3_4.png"),
        LoadTexture(BarShopUi034ArtRoot + "bar_shop_continue_idle_200x56_0_3_4.png"),
        LoadTexture(BarShopUi034ArtRoot + "bar_shop_continue_hover_200x56_0_3_4.png"),
        LoadTexture(BarShopUi034ArtRoot + "bar_shop_continue_pressed_200x56_0_3_4.png"),
        LoadTexture(BarShopUiArtRoot + "bar_shop_ammo_panel_200x58_0_3_0.png"),
        LoadTexture(BarShopUiArtRoot + "bar_shop_hp_panel_200x58_0_3_0.png"),
        new[]
        {
          new BarShopProductIconBinding(
            "bar_shop.item.dummy_01",
            LoadTexture(BarShopUiArtRoot + "bar_shop_dummy_item_01_64_0_3_0.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.dummy_02",
            LoadTexture(BarShopUiArtRoot + "bar_shop_dummy_item_02_64_0_3_0.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.dummy_03",
            LoadTexture(BarShopUiArtRoot + "bar_shop_dummy_item_03_64_0_3_0.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.dummy_04",
            LoadTexture(BarShopUiArtRoot + "bar_shop_dummy_item_04_64_0_3_0.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.dummy_05",
            LoadTexture(BarShopUiArtRoot + "bar_shop_dummy_item_05_64_0_3_0.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.dummy_06",
            LoadTexture(BarShopUiArtRoot + "bar_shop_dummy_item_06_64_0_3_0.png"))
        },
        ammoPouch: LoadTexture(
          BarShopUi034ArtRoot + "bar_shop_ammo_pouch_180x150_0_3_4.png"),
        bulletTossSheet: LoadTexture(
          BarShopUi034ArtRoot + "bar_shop_bullet_toss_spin_384x64_0_3_4.png"),
        ammoPouchEmpty: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_ammo_pouch_empty_180x150_0_3_8.png"),
        ammoPouchBullet: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_ammo_pouch_bullet_24x40_0_3_8.png"),
        ammoPouchHandCover: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_ammo_pouch_hand_cover_220x180_0_3_8.png"),
        bulletCoinFlipSheet: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_bullet_coin_flip_glint_8f_512x64_0_3_8.png"),
        bulletPourSheet: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_bullet_pour_table_8f_1280x120_0_3_8.png"));
      var stageTransitionUiArtSet = new StageTransitionUiArtSet(
        LoadTexture(StageTransitionUiArtRoot
          + "stage_exit_background_closed_unlit_960x540_0_3_1.png"),
        LoadTexture(StageTransitionUiArtRoot
          + "stage_exit_background_open_unlit_960x540_0_3_1.png"),
        LoadNumberedTextures(
          StageTransitionUiArtRoot,
          "stage_exit_door_left_{0:00}_128x210_0_3_1.png",
          4),
        LoadNumberedTextures(
          StageTransitionUiArtRoot,
          "stage_exit_door_right_{0:00}_128x210_0_3_1.png",
          4),
        LoadNumberedTextures(
          StageTransitionUiArtRoot,
          "stage_exit_walk_dust_{0:00}_96x64_0_3_1.png",
          4),
        LoadTexture(StageTransitionUiArtRoot
          + "stage_exit_walk_vignette_960x540_0_3_1.png"),
        LoadTexture(StageTransitionUiArtRoot
          + "stage_transition_fade_black_16_0_3_1.png"),
        LoadNumberedTextures(
          StageTransitionUiArtRoot,
          "stage_transition_loading_{0:00}_64_0_3_1.png",
          8));
      var economyUiArtSet = new EconomyUiArtSet(
        LoadOptionalTexture(EconomyUi012ArtRoot + "currency_base_icon_48_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "currency_temporary_icon_48_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "currency_price_icon_24_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "currency_base_panel_160x58_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "currency_temporary_panel_160x58_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "stage_reward_base_frame_240x96_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "stage_reward_temporary_frame_240x96_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "shop_exit_warning_icon_48_0_1_2.png"));
      view.Configure(
        boardTexture,
        cardArtSet,
        halliUiArtSet,
        guideUiArtSet,
        introTexture,
        healthUiArtSet,
        pokerUiArtSet,
        useSceneBackdrop: true,
        useIntroArtLayout: true,
        barShopUiArtSet: barShopUiArtSet,
        stageTransitionUiArtSet: stageTransitionUiArtSet,
        pokerItemUiArtSet: pokerItemUiArtSet,
        economyUiArtSet: economyUiArtSet);
      var presentationRig = gameObject.AddComponent<TableScenePresentationRig>();
      var backdropShader = AssetDatabase.LoadAssetAtPath<Shader>(BackdropShaderPath);
      if (backdropShader == null)
      {
        throw new InvalidOperationException($"Missing backdrop shader at {BackdropShaderPath}");
      }
      presentationRig.Configure(
        camera,
        view,
        boardTexture,
        introTexture,
        barShopUiArtSet.Background,
        backdropShader);
      gameObject.AddComponent<PlayableDevGameController>();

      if (!EditorSceneManager.SaveScene(scene, ScenePath))
      {
        throw new InvalidOperationException($"Failed to save scene: {ScenePath}");
      }

      EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      Debug.Log($"Playable dev scene created and registered: {ScenePath}");
    }

    [MenuItem("Codex Game/Playable Dev/Build WebGL Development")]
    public static void BuildWebGlDevelopment()
    {
      BuildWebGl(BuildOptions.Development);
    }

    [MenuItem("Codex Game/Playable Dev/Build WebGL Cloudflare Preview")]
    public static void BuildWebGlCloudflarePreview()
    {
      var previousCompressionFormat = PlayerSettings.WebGL.compressionFormat;
      var previousDecompressionFallback = PlayerSettings.WebGL.decompressionFallback;
      var previousDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.WebGL);

      try
      {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.SetScriptingDefineSymbols(
          NamedBuildTarget.WebGL,
          AddDefine(previousDefines, "ENABLE_GAMEPLAY_CHEATS"));
        BuildWebGl(BuildOptions.None);
      }
      finally
      {
        PlayerSettings.WebGL.compressionFormat = previousCompressionFormat;
        PlayerSettings.WebGL.decompressionFallback = previousDecompressionFallback;
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.WebGL, previousDefines);
      }
    }

    private static string AddDefine(string defines, string requiredDefine)
    {
      var entries = (defines ?? string.Empty).Split(';');
      for (var index = 0; index < entries.Length; index++)
      {
        if (string.Equals(entries[index], requiredDefine, StringComparison.Ordinal)) return defines;
      }
      return string.IsNullOrWhiteSpace(defines)
        ? requiredDefine
        : defines + ";" + requiredDefine;
    }

    private static void BuildWebGl(BuildOptions buildOptions)
    {
      CreateScene();
      var output = Environment.GetEnvironmentVariable("CODEX_GAME_WEBGL_OUTPUT");
      var buildName = Environment.GetEnvironmentVariable("CODEX_GAME_BUILD_NAME");

      if (string.IsNullOrWhiteSpace(output))
      {
        output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Builds", "WebGLDev"));
      }

      Directory.CreateDirectory(output);
      var previousProductName = PlayerSettings.productName;
      if (!string.IsNullOrWhiteSpace(buildName)) PlayerSettings.productName = buildName;
      BuildReport report;
      try
      {
        report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
          scenes = new[] { ScenePath },
          locationPathName = output,
          target = BuildTarget.WebGL,
          options = buildOptions
        });
      }
      finally
      {
        PlayerSettings.productName = previousProductName;
      }

      if (report.summary.result != BuildResult.Succeeded)
      {
        throw new InvalidOperationException(
          $"WebGL build failed: {report.summary.result}, errors={report.summary.totalErrors}");
      }

      Debug.Log($"PLAYABLE_WEBGL_BUILD={output}");
    }

    private static void EnsureScenesFolder()
    {
      if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
      {
        AssetDatabase.CreateFolder("Assets", "Scenes");
      }
    }

    private static Texture2D LoadTexture(string path)
    {
      var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      if (texture == null)
      {
        throw new FileNotFoundException("Playable UI art was not found.", path);
      }
      return texture;
    }

    private static Texture2D LoadOptionalTexture(string path)
    {
      return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Texture2D[] LoadNumberedTextures(
      string root,
      string filePattern,
      int count)
    {
      var textures = new Texture2D[count];
      for (var index = 0; index < count; index++)
      {
        textures[index] = LoadTexture(root + string.Format(filePattern, index + 1));
      }
      return textures;
    }
  }
}
