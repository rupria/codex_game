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
    private const string HalliUi034ArtRoot = UiArtRoot + "Halli_0_3_4/";
    private const string HalliUi037ArtRoot = UiArtRoot + "Halli_0_3_7/";
    private const string HalliUi039ArtRoot = UiArtRoot + "Halli_0_3_9/";
    private const string HalliAlertsUi054ArtRoot = UiArtRoot + "HalliAlerts_0_5_4/";
    private const string PokerUi034ArtRoot = UiArtRoot + "Poker_0_3_4/";
    private const string PokerUi036ArtRoot = UiArtRoot + "Poker_0_3_6/";
    private const string PokerUi037ArtRoot = UiArtRoot + "Poker_0_3_7/";
    private const string GameplayUi012ArtRoot = UiArtRoot + "Gameplay_0_1_2/";
    private const string BarShopUiArtRoot = UiArtRoot + "BarShop_0_3_0/";
    private const string BarShopUi034ArtRoot = UiArtRoot + "BarShop_0_3_4/";
    private const string BarShopUi038ArtRoot = UiArtRoot + "BarShop_0_3_8/";
    private const string EconomyUi012ArtRoot = UiArtRoot + "Economy_0_1_2/";
    private const string StageRewardUi055ArtRoot = UiArtRoot + "StageReward_0_5_5/";
    private const string PresentationUi0124ArtRoot = UiArtRoot + "Presentation_0_1_2_4/";
    private const string TextlessCurrencyUi040ArtRoot = UiArtRoot + "Textless_Currency_0_4_0/";
    private const string StageOpponentUi0124ArtRoot = UiArtRoot + "StageOpponents_0_1_2_4/";
    private const string IconOverhaulUi050ArtRoot = UiArtRoot + "IconOverhaul_0_5_0/";
    private const string IconOverhaulUi051ArtRoot = UiArtRoot + "IconOverhaul_0_5_1/";
    private const string ItemExpansionUi052ArtRoot = UiArtRoot + "ItemExpansion_0_5_2/";
    private const string PokerResultUi056ArtRoot = UiArtRoot + "PokerResultLabel_0_5_6/";
    private const string ShopItemIconsUi056ArtRoot = UiArtRoot + "ShopItemIcons_0_5_6/";
    private const string GuideNavUi060ArtRoot = UiArtRoot + "GuideNav_0_6_0/";
    private const string MainMenuUi058ArtRoot = UiArtRoot + "MainMenu_0_5_8/";
    private const string JokerRevealUi054ArtRoot = UiArtRoot + "JokerReveal_0_5_4/";
    private const string JokerHandChoiceUi060ArtRoot = UiArtRoot + "JokerHandChoice_0_6_0/";
    private const string PrivateSelectionUi055ArtRoot = UiArtRoot + "PrivateSelection_0_5_5/";
    private const string PokerItemActionUi060ArtRoot = UiArtRoot + "PokerItemAction_0_6_0/";
    private const string PokerPredictionCleanUi061ArtRoot =
      UiArtRoot + "PokerPredictionClean_0_6_1/";
    private const string ThreeCallEntryUi060ArtRoot = UiArtRoot + "ThreeCallEntry_0_6_0/";
    private const string StageTransitionUiArtRoot = UiArtRoot + "StageTransition_0_3_1/";
    private const string IntroArtPath =
      MainMenuUi058ArtRoot + "start_screen_background_960x540_0_5_8.png";
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
        LoadTexture(IconOverhaulUi050ArtRoot + "community_lock_locked_48_0_5_0.png"),
        LoadTexture(UiArtRoot + "flip_timer.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_idle.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_hover.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_pressed.png"),
        LoadTexture(HalliUiArtRoot + "flip_deck_disabled.png"),
        LoadTexture(HalliUiArtRoot + "player_acquired_tray.png"),
        LoadTexture(HalliUiArtRoot + "ai_acquired_status_panel.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "round_win_badge_player_empty_32_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "round_win_badge_player_filled_32_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "round_win_badge_ai_empty_32_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "round_win_badge_ai_filled_32_0_5_0.png"),
        ropeBody: LoadTexture(
          HalliUi039ArtRoot + "halli_rope_braided_body_258x16_0_3_9.png"),
        ropeCharCap: LoadTexture(
          HalliUi039ArtRoot + "halli_rope_burn_char_cap_24x16_0_3_9.png"),
        ropeFlame: LoadTexture(
          HalliAlertsUi054ArtRoot + "halli_rope_contact_flame_6f_288x48_0_5_4.png"),
        ropeExplosion: LoadTexture(
          HalliAlertsUi054ArtRoot + "halli_rope_terminal_burst_8f_768x96_0_5_4.png"),
        sharedPileRailIdle: LoadTexture(
          HalliUi037ArtRoot + "halli_shared_pile_rail_idle_140x136_0_3_7.png"),
        sharedPileRailPlayerActive: LoadTexture(
          HalliUi037ArtRoot + "halli_shared_pile_rail_player_active_140x136_0_3_7.png"),
        sharedPileRailAiActive: LoadTexture(
          HalliUi037ArtRoot + "halli_shared_pile_rail_ai_active_140x136_0_3_7.png"),
        playerOnlyAcquiredTray: LoadTexture(
          HalliUi034ArtRoot + "player_acquired_tray_open_378x130_0_3_4.png"),
        aiThinkingSheet: LoadTexture(
          IconOverhaulUi050ArtRoot + "ai_thinking_cylinder_western_8f_384x48_0_5_0.png"),
        ropeScorch: LoadTexture(
          HalliAlertsUi054ArtRoot + "halli_rope_terminal_scorch_32x24_0_5_4.png"),
        lastFiveCountdownSheet: LoadTexture(
          HalliAlertsUi054ArtRoot + "halli_last_five_countdown_sheet_5f_480x96_0_5_4.png"),
        lastFiveCountdownPlate: LoadTexture(
          HalliAlertsUi054ArtRoot + "halli_last_five_countdown_plate_96_0_5_4.png"));
      var guideUiArtSet = new GuideUiArtSet(
        LoadTexture(HalliUiArtRoot + "guide_modal_background.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_flow_art.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_halli_art.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_cards_art.png"),
        LoadTexture(HalliUiArtRoot + "guide_page_result_art.png"),
        LoadTexture(GuideNavUi060ArtRoot + "guide_nav_rail_960x104_0_6_0.png"),
        LoadTexture(GuideNavUi060ArtRoot + "guide_page_indicator_plate_132x38_0_6_0.png"),
        new GuideNavButtonArtSet(
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_previous_idle_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_previous_hover_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_previous_pressed_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_previous_disabled_56x58_0_6_0.png")),
        new GuideNavButtonArtSet(
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_next_idle_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_next_hover_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_next_pressed_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_next_disabled_56x58_0_6_0.png")),
        new GuideNavButtonArtSet(
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_close_idle_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_close_hover_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_close_pressed_56x58_0_6_0.png"),
          LoadTexture(GuideNavUi060ArtRoot + "guide_nav_close_disabled_56x58_0_6_0.png")));
      var mainMenuUiArtSet = new MainMenuUiArtSet(
        introTexture,
        LoadTexture(MainMenuUi058ArtRoot + "main_menu_duel_crest_300x190_0_5_8.png"),
        new MainMenuButtonArtSet(
          LoadTexture(MainMenuUi058ArtRoot + "main_menu_start_idle_380x84_0_5_8.png"),
          LoadTexture(MainMenuUi058ArtRoot + "main_menu_start_hover_380x84_0_5_8.png"),
          LoadTexture(MainMenuUi058ArtRoot + "main_menu_start_pressed_380x84_0_5_8.png")),
        new MainMenuButtonArtSet(
          LoadTexture(MainMenuUi058ArtRoot + "main_menu_guide_idle_380x84_0_5_8.png"),
          LoadTexture(MainMenuUi058ArtRoot + "main_menu_guide_hover_380x84_0_5_8.png"),
          LoadTexture(MainMenuUi058ArtRoot + "main_menu_guide_pressed_380x84_0_5_8.png")));
      var healthUiArtSet = new HealthUiArtSet(
        LoadTexture(IconOverhaulUi050ArtRoot + "hp_heart_player_filled_24_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "hp_heart_player_empty_24_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "hp_heart_ai_filled_24_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "hp_heart_ai_empty_24_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "hp_heart_player_damage_24_0_5_0.png"),
        LoadTexture(IconOverhaulUi050ArtRoot + "hp_heart_ai_damage_24_0_5_0.png"));
      var pokerUiArtSet = new PokerUiArtSet(
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_player_idle_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_player_hover_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_ai_idle_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_ai_hover_232x64_0_6_1.png"),
        LoadTexture(UiArtRoot + "item_slot.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_player_idle_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_player_hover_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_player_selected_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_ai_idle_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_ai_hover_232x64_0_6_1.png"),
        LoadTexture(PokerPredictionCleanUi061ArtRoot + "poker_prediction_ai_selected_232x64_0_6_1.png"),
        playerPredictionDisabled: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_prediction_player_disabled_232x64_0_6_1.png"),
        aiPredictionDisabled: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_prediction_ai_disabled_232x64_0_6_1.png"),
        predictionTitlePlate: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_prediction_title_plate_320x48_0_6_1.png"),
        predictionStageEmblem: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_prediction_stage_emblem_40_0_6_1.png"),
        insuranceRemainingIcon: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_insurance_remaining_icon_28_0_6_1.png"),
        predictionSuccessIcon: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_prediction_success_icon_28_0_6_1.png"),
        resultContinueIdle: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_result_continue_idle_164x44_0_6_1.png"),
        resultContinueHover: LoadTexture(
          PokerPredictionCleanUi061ArtRoot + "poker_result_continue_hover_164x44_0_6_1.png"));
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
        LoadTexture(IconOverhaulUi051ArtRoot + "item_reload_inventory_64_0_5_1.png"),
        LoadTexture(IconOverhaulUi051ArtRoot + "item_bottom_deal_inventory_64_0_5_1.png"),
        LoadTexture(IconOverhaulUi051ArtRoot + "item_hype_man_inventory_64_0_5_1.png"),
        LoadTexture(IconOverhaulUi051ArtRoot + "item_heal_tonic_inventory_64_0_5_1.png"),
        LoadTexture(PokerUi037ArtRoot + "poker_item_select_panel_640x336_0_3_7.png"),
        LoadTexture(PokerUi037ArtRoot + "poker_item_detail_panel_376x112_0_3_7.png"),
        LoadTexture(PokerItemActionUi060ArtRoot + "poker_item_action_button_idle_172x44_0_6_0.png"),
        LoadTexture(PokerItemActionUi060ArtRoot + "poker_item_action_button_hover_172x44_0_6_0.png"),
        LoadTexture(PokerItemActionUi060ArtRoot + "poker_item_action_button_disabled_172x44_0_6_0.png"),
        reloadPopupIcon: LoadTexture(IconOverhaulUi051ArtRoot + "item_reload_popup_80_0_5_1.png"),
        bottomDealPopupIcon: LoadTexture(IconOverhaulUi051ArtRoot + "item_bottom_deal_popup_80_0_5_1.png"),
        hypeManPopupIcon: LoadTexture(IconOverhaulUi051ArtRoot + "item_hype_man_popup_80_0_5_1.png"),
        healthRecoveryPopupIcon: LoadTexture(IconOverhaulUi051ArtRoot + "item_heal_tonic_popup_80_0_5_1.png"),
        communityLocked: LoadTexture(IconOverhaulUi050ArtRoot + "community_lock_locked_48_0_5_0.png"),
        communityReveal: LoadTexture(IconOverhaulUi050ArtRoot + "community_lock_reveal_48_0_5_0.png"),
        communityOpen: LoadTexture(IconOverhaulUi050ArtRoot + "community_lock_open_48_0_5_0.png"),
        wildInkDefault: LoadTexture(ItemExpansionUi052ArtRoot + "item_wild_ink_default_64_0_5_2.png"),
        wildInkHover: LoadTexture(ItemExpansionUi052ArtRoot + "item_wild_ink_hover_64_0_5_2.png"),
        wildInkSelected: LoadTexture(ItemExpansionUi052ArtRoot + "item_wild_ink_selected_64_0_5_2.png"),
        wildInkDisabled: LoadTexture(ItemExpansionUi052ArtRoot + "item_wild_ink_disabled_64_0_5_2.png"),
        wildInkPopup: LoadTexture(ItemExpansionUi052ArtRoot + "item_wild_ink_popup_80_0_5_2.png"),
        barrelDefault: LoadTexture(ItemExpansionUi052ArtRoot + "item_barrel_default_64_0_5_2.png"),
        barrelHover: LoadTexture(ItemExpansionUi052ArtRoot + "item_barrel_hover_64_0_5_2.png"),
        barrelSelected: LoadTexture(ItemExpansionUi052ArtRoot + "item_barrel_selected_64_0_5_2.png"),
        barrelDisabled: LoadTexture(ItemExpansionUi052ArtRoot + "item_barrel_disabled_64_0_5_2.png"),
        barrelPopup: LoadTexture(ItemExpansionUi052ArtRoot + "item_barrel_popup_80_0_5_2.png"),
        insuranceDefault: LoadTexture(ItemExpansionUi052ArtRoot + "item_prediction_insurance_default_64_0_5_2.png"),
        insuranceHover: LoadTexture(ItemExpansionUi052ArtRoot + "item_prediction_insurance_hover_64_0_5_2.png"),
        insuranceSelected: LoadTexture(ItemExpansionUi052ArtRoot + "item_prediction_insurance_selected_64_0_5_2.png"),
        insuranceDisabled: LoadTexture(ItemExpansionUi052ArtRoot + "item_prediction_insurance_disabled_64_0_5_2.png"),
        insurancePopup: LoadTexture(ItemExpansionUi052ArtRoot + "item_prediction_insurance_popup_80_0_5_2.png"),
        mercenaryDefault: LoadTexture(ItemExpansionUi052ArtRoot + "item_mercenary_default_64_0_5_2.png"),
        mercenaryHover: LoadTexture(ItemExpansionUi052ArtRoot + "item_mercenary_hover_64_0_5_2.png"),
        mercenarySelected: LoadTexture(ItemExpansionUi052ArtRoot + "item_mercenary_selected_64_0_5_2.png"),
        mercenaryDisabled: LoadTexture(ItemExpansionUi052ArtRoot + "item_mercenary_disabled_64_0_5_2.png"),
        mercenaryPopup: LoadTexture(ItemExpansionUi052ArtRoot + "item_mercenary_popup_80_0_5_2.png"),
        wildInkSpreadSheet: LoadTexture(ItemExpansionUi052ArtRoot + "wild_ink_spread_8f_512x64_0_5_2.png"),
        wildInkSpadeSeal: LoadTexture(ItemExpansionUi052ArtRoot + "wild_ink_suit_seal_1_32_0_5_2.png"),
        wildInkHeartSeal: LoadTexture(ItemExpansionUi052ArtRoot + "wild_ink_suit_seal_2_32_0_5_2.png"),
        wildInkClubSeal: LoadTexture(ItemExpansionUi052ArtRoot + "wild_ink_suit_seal_3_32_0_5_2.png"),
        wildInkDiamondSeal: LoadTexture(ItemExpansionUi052ArtRoot + "wild_ink_suit_seal_4_32_0_5_2.png"),
        wildInkAppliedMarker: LoadTexture(ItemExpansionUi052ArtRoot + "wild_ink_card_applied_marker_32_0_5_2.png"),
        wildInkExchangeLockedMarker: LoadTexture(ItemExpansionUi052ArtRoot + "wild_ink_exchange_locked_marker_32_0_5_2.png"),
        barrelDefenseReady: LoadTexture(ItemExpansionUi052ArtRoot + "barrel_defense_ready_64_0_5_2.png"),
        barrelDefenseBroken: LoadTexture(ItemExpansionUi052ArtRoot + "barrel_defense_broken_64_0_5_2.png"),
        barrelDefenseBreakSheet: LoadTexture(ItemExpansionUi052ArtRoot + "barrel_defense_impact_break_8f_512x64_0_5_2.png"),
        barrelHpPreservedMarker: LoadTexture(ItemExpansionUi052ArtRoot + "barrel_hp_preserved_marker_32_0_5_2.png"),
        insuranceApplySheet: LoadTexture(ItemExpansionUi052ArtRoot + "prediction_insurance_apply_6f_384x64_0_5_2.png"),
        insuranceCharges0: LoadTexture(ItemExpansionUi052ArtRoot + "prediction_insurance_charges_0_32_0_5_2.png"),
        insuranceCharges1: LoadTexture(ItemExpansionUi052ArtRoot + "prediction_insurance_charges_1_32_0_5_2.png"),
        insuranceCharges2: LoadTexture(ItemExpansionUi052ArtRoot + "prediction_insurance_charges_2_32_0_5_2.png"),
        predictionActualSuccess: LoadTexture(ItemExpansionUi052ArtRoot + "prediction_result_actual_success_32_0_5_2.png"),
        predictionInsuredSuccess: LoadTexture(ItemExpansionUi052ArtRoot + "prediction_result_insured_success_32_0_5_2.png"),
        mercenaryExchangeSheet: LoadTexture(ItemExpansionUi052ArtRoot + "mercenary_simultaneous_exchange_10f_960x96_0_5_2.png"),
        mercenaryPlayerTargetMarker: LoadTexture(ItemExpansionUi052ArtRoot + "mercenary_player_target_marker_32_0_5_2.png"),
        mercenaryAiHiddenMarker: LoadTexture(ItemExpansionUi052ArtRoot + "mercenary_ai_hidden_marker_32_0_5_2.png"));
      var pokerResultUiArtSet = new PokerResultUiArtSet(
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_success_compact_788x108_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_success_standard_788x132_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_success_expanded_788x164_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_failure_compact_788x108_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_failure_standard_788x132_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_failure_expanded_788x164_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_neutral_compact_788x108_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_neutral_standard_788x132_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_message_panel_neutral_expanded_788x164_0_5_6.png"),
        LoadTexture(PokerResultUi056ArtRoot + "poker_result_item_status_chip_360x32_0_5_6.png"));
      var barShopUiArtSet = new BarShopUiArtSet(
        LoadTexture(BarShopUiArtRoot + "bar_shop_background_unlit_960x540_0_3_0.png"),
        LoadTexture(ShopItemIconsUi056ArtRoot + "bar_shop_product_slot_190x174_0_5_6.png"),
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
            "bar_shop.item.reload",
            LoadTexture(IconOverhaulUi051ArtRoot + "item_reload_popup_80_0_5_1.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.bottom_deal",
            LoadTexture(IconOverhaulUi051ArtRoot + "item_bottom_deal_popup_80_0_5_1.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.hype_man",
            LoadTexture(IconOverhaulUi051ArtRoot + "item_hype_man_popup_80_0_5_1.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.health_recovery",
            LoadTexture(IconOverhaulUi051ArtRoot + "item_heal_tonic_popup_80_0_5_1.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.wild_ink",
            LoadTexture(ItemExpansionUi052ArtRoot + "item_wild_ink_popup_80_0_5_2.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.barrel",
            LoadTexture(ItemExpansionUi052ArtRoot + "item_barrel_popup_80_0_5_2.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.prediction_insurance",
            LoadTexture(ItemExpansionUi052ArtRoot + "item_prediction_insurance_popup_80_0_5_2.png")),
          new BarShopProductIconBinding(
            "bar_shop.item.mercenary",
            LoadTexture(ItemExpansionUi052ArtRoot + "item_mercenary_popup_80_0_5_2.png")),
          new BarShopProductIconBinding(
            "item.ammo_pouch.loose_rounds",
            LoadTexture(BarShopUi038ArtRoot + "ammo_pouch_loose_rounds_64_0_1_0.png"))
        },
        ammoPouch: LoadTexture(
          BarShopUi038ArtRoot + "ammo_pouch_loose_rounds_64_0_1_0.png"),
        bulletTossSheet: LoadTexture(
          BarShopUi034ArtRoot + "bar_shop_bullet_toss_spin_384x64_0_3_4.png"),
        ammoPouchBullet: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_ammo_pouch_bullet_24x40_0_3_8.png"),
        bulletCoinFlipSheet: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_bullet_coin_flip_glint_8f_512x64_0_3_8.png"),
        bulletPourSheet: LoadTexture(
          BarShopUi038ArtRoot + "bar_shop_bullet_pour_table_8f_1280x120_0_3_8.png"),
        itemIconPlateIdle: LoadTexture(
          ShopItemIconsUi056ArtRoot + "bar_shop_item_icon_plate_idle_88x88_0_5_6.png"),
        itemIconPlateHover: LoadTexture(
          ShopItemIconsUi056ArtRoot + "bar_shop_item_icon_plate_hover_88x88_0_5_6.png"),
        itemIconPlateDisabled: LoadTexture(
          ShopItemIconsUi056ArtRoot + "bar_shop_item_icon_plate_disabled_88x88_0_5_6.png"));
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
        LoadOptionalTexture(ShopItemIconsUi056ArtRoot + "currency_basic_bullet_western_48_0_5_6.png"),
        LoadOptionalTexture(ShopItemIconsUi056ArtRoot + "currency_temporary_cracked_round_48_0_5_6.png"),
        LoadOptionalTexture(ShopItemIconsUi056ArtRoot + "shop_price_bullet_western_28_0_5_6.png"),
        LoadOptionalTexture(TextlessCurrencyUi040ArtRoot + "battle_currency_basic_panel_112x52_0_4_0.png"),
        LoadOptionalTexture(TextlessCurrencyUi040ArtRoot + "battle_currency_temporary_panel_112x52_0_4_0.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "stage_reward_base_frame_240x96_0_1_2.png"),
        LoadOptionalTexture(EconomyUi012ArtRoot + "stage_reward_temporary_frame_240x96_0_1_2.png"),
        LoadOptionalTexture(IconOverhaulUi050ArtRoot + "shop_exit_warning_badge_western_24_0_5_0.png"),
        LoadOptionalTexture(IconOverhaulUi050ArtRoot + "currency_temporary_expire_western_8f_320x40_0_5_0.png"),
        LoadOptionalTexture(IconOverhaulUi050ArtRoot + "shop_exit_warning_pulse_western_6f_144x24_0_5_0.png"),
        stageRewardSummaryPanel: LoadTexture(
          StageRewardUi055ArtRoot + "stage_reward_summary_panel_680x360_0_5_5.png"),
        stageRewardRowFrame: LoadTexture(
          StageRewardUi055ArtRoot + "stage_reward_row_frame_304x64_0_5_5.png"),
        stageRewardContentBackground: LoadTexture(
          StageRewardUi055ArtRoot + "stage_reward_content_opaque_632x154_0_5_5.png"));
      var presentationUiArtSet = new PresentationUiArtSet(
        fileName => LoadTexture(PresentationUi0124ArtRoot + fileName),
        new[]
        {
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_01_bellman_cutout_108_0_1_2_4.png"),
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_02_rose_cutout_108_0_1_2_4.png"),
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_03_prospector_cutout_108_0_1_2_4.png"),
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_04_undertaker_cutout_108_0_1_2_4.png")
        },
        new[]
        {
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_01_bellman_portrait_108_0_1_2_4.png"),
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_02_rose_portrait_108_0_1_2_4.png"),
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_03_prospector_portrait_108_0_1_2_4.png"),
          LoadTexture(StageOpponentUi0124ArtRoot + "stage_opponent_04_undertaker_portrait_108_0_1_2_4.png")
        },
        threeCallPlaque: LoadTexture(
          ThreeCallEntryUi060ArtRoot + "three_call_entry_center_plaque_380x112_0_6_0.png"),
        threeCallBellPulseSheet: LoadTexture(
          ThreeCallEntryUi060ArtRoot + "three_call_bell_pulse_8f_512x64_0_6_0.png"),
        showdownIcon: LoadTexture(IconOverhaulUi050ArtRoot + "phase_showdown_western_64_0_5_0.png"),
        limitOne: LoadTexture(IconOverhaulUi050ArtRoot + "stage_item_limit_one_western_64_0_5_0.png"),
        limitTwo: LoadTexture(IconOverhaulUi050ArtRoot + "stage_item_limit_two_western_64_0_5_0.png"),
        usedOne: LoadTexture(IconOverhaulUi050ArtRoot + "stage_item_limit_used_one_western_64_0_5_0.png"),
        limitExhausted: LoadTexture(IconOverhaulUi050ArtRoot + "stage_item_limit_exhausted_western_64_0_5_0.png"),
        inventoryRestricted: LoadTexture(IconOverhaulUi050ArtRoot + "stage_item_inventory_restricted_western_64_0_5_0.png"),
        cardRestricted: LoadTexture(IconOverhaulUi050ArtRoot + "stage_item_card_restricted_western_64_0_5_0.png"));
      var jokerRevealUiArtSet = new JokerRevealUiArtSet(
        LoadTexture(JokerRevealUi054ArtRoot + "joker_reveal_focus_vignette_960x540_0_5_4.png"),
        LoadTexture(JokerRevealUi054ArtRoot + "joker_reveal_arc_trail_6f_576x96_0_5_4.png"),
        LoadTexture(JokerRevealUi054ArtRoot + "joker_reveal_gunsight_ring_8f_1536x192_0_5_4.png"),
        LoadTexture(JokerRevealUi054ArtRoot + "joker_reveal_muzzle_flash_8f_1280x160_0_5_4.png"),
        LoadTexture(JokerRevealUi054ArtRoot + "joker_reveal_card_glint_6f_672x156_0_5_4.png"),
        LoadTexture(JokerRevealUi054ArtRoot + "joker_reveal_settle_glint_5f_320x64_0_5_4.png"));
      var jokerHandChoiceUiArtSet = new JokerHandChoiceUiArtSet(
        LoadTexture(JokerHandChoiceUi060ArtRoot + "joker_hand_choice_dim_960x540_0_6_0.png"),
        LoadTexture(JokerHandChoiceUi060ArtRoot + "joker_hand_choice_panel_compact_600x420_0_6_0.png"),
        LoadTexture(JokerHandChoiceUi060ArtRoot + "joker_hand_option_idle_440x44_0_6_0.png"),
        LoadTexture(JokerHandChoiceUi060ArtRoot + "joker_hand_option_hover_440x44_0_6_0.png"),
        LoadTexture(JokerHandChoiceUi060ArtRoot + "joker_hand_option_selected_440x44_0_6_0.png"),
        LoadTexture(JokerHandChoiceUi060ArtRoot + "joker_hand_option_disabled_440x44_0_6_0.png"));
      var privateSelectionUiArtSet = new PrivateSelectionUiArtSet(
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_modal_dim_960x540_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_modal_panel_860x456_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_public_frame_166x198_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_candidate_idle_112x150_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_candidate_hover_112x150_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_candidate_selected_112x150_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_candidate_confirmed_112x150_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_candidate_disabled_112x150_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_confirm_idle_180x52_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_confirm_active_180x52_0_5_5.png"),
        LoadTexture(PrivateSelectionUi055ArtRoot + "private_selection_confirm_disabled_180x52_0_5_5.png"));
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
        economyUiArtSet: economyUiArtSet,
        presentationUiArtSet: presentationUiArtSet,
        pokerResultUiArtSet: pokerResultUiArtSet,
        privateSelectionUiArtSet: privateSelectionUiArtSet,
        jokerRevealUiArtSet: jokerRevealUiArtSet,
        jokerHandChoiceUiArtSet: jokerHandChoiceUiArtSet,
        mainMenuUiArtSet: mainMenuUiArtSet);
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
