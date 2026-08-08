using System;
using System.IO;
using CodexGame.Bootstrap;
using CodexGame.Presentation.Art;
using CodexGame.Presentation.Views;
using UnityEditor;
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
      "Assets/Art/Prototype/Board/halli_western_saloon_background.png";
    private const string UiArtRoot = "Assets/Art/Prototype/UI/";
    private const string HalliUiArtRoot = UiArtRoot + "Halli_0_1_0/";
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
        LoadTexture(HalliUiArtRoot + "ai_acquired_status_panel.png"));
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
      view.Configure(
        boardTexture,
        cardArtSet,
        halliUiArtSet,
        guideUiArtSet,
        useSceneBackdrop: true,
        useIntroArtLayout: true);
      var presentationRig = gameObject.AddComponent<TableScenePresentationRig>();
      var backdropShader = AssetDatabase.LoadAssetAtPath<Shader>(BackdropShaderPath);
      if (backdropShader == null)
      {
        throw new InvalidOperationException($"Missing backdrop shader at {BackdropShaderPath}");
      }
      presentationRig.Configure(camera, view, boardTexture, introTexture, backdropShader);
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

      try
      {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        BuildWebGl(BuildOptions.None);
      }
      finally
      {
        PlayerSettings.WebGL.compressionFormat = previousCompressionFormat;
        PlayerSettings.WebGL.decompressionFallback = previousDecompressionFallback;
      }
    }

    private static void BuildWebGl(BuildOptions buildOptions)
    {
      CreateScene();
      var output = Environment.GetEnvironmentVariable("CODEX_GAME_WEBGL_OUTPUT");

      if (string.IsNullOrWhiteSpace(output))
      {
        output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Builds", "WebGLDev"));
      }

      Directory.CreateDirectory(output);
      var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
      {
        scenes = new[] { ScenePath },
        locationPathName = output,
        target = BuildTarget.WebGL,
        options = buildOptions
      });

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
  }
}
