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
    public const string BoardArtPath = "Assets/Art/Prototype/Board/board_layout_wip.png";

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
      PlayableCardArtSet cardArtSet = PlayableCardArtLoader.Load();
      view.Configure(boardTexture, cardArtSet);
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
        options = BuildOptions.Development
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
  }
}
