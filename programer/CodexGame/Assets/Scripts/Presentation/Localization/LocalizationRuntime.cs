using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CodexGame.Presentation.Localization
{
  [DisallowMultipleComponent]
  public sealed class LocalizationRuntime : MonoBehaviour
  {
    private const string PlayerPrefsKey = "codex_game.ui_language";
    private const string RelativeCsvPath = "Localization/ui_strings.csv";

    public event Action Changed;
    public LocalizationCatalog Catalog { get; private set; }
    public string Language { get; private set; } = LocalizationCatalog.DefaultLanguage;
    public bool IsReady => Catalog != null;

    private void Awake()
    {
      var stored = PlayerPrefs.GetString(PlayerPrefsKey, LocalizationCatalog.DefaultLanguage);
      Language = stored == LocalizationCatalog.FallbackLanguage
        ? LocalizationCatalog.FallbackLanguage
        : LocalizationCatalog.DefaultLanguage;
    }

    private IEnumerator Start()
    {
      var path = Path.Combine(UnityEngine.Application.streamingAssetsPath, RelativeCsvPath).Replace('\\', '/');
      if (path.IndexOf("://", StringComparison.Ordinal) < 0)
      {
        path = new Uri(path).AbsoluteUri;
      }
      using (var request = UnityWebRequest.Get(path))
      {
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
          Debug.LogError("Localization CSV load failed: " + request.error);
          yield break;
        }

        try
        {
          Catalog = LocalizationCatalog.Parse(request.downloadHandler.text, Debug.LogWarning);
          Changed?.Invoke();
        }
        catch (Exception exception)
        {
          Debug.LogException(exception);
        }
      }
    }

    public void SetLanguage(string language)
    {
      var normalized = language == LocalizationCatalog.FallbackLanguage
        ? LocalizationCatalog.FallbackLanguage
        : LocalizationCatalog.DefaultLanguage;
      if (Language == normalized) return;
      Language = normalized;
      PlayerPrefs.SetString(PlayerPrefsKey, Language);
      PlayerPrefs.Save();
      Changed?.Invoke();
    }

    public string Get(string key, params LocalizationArgument[] arguments)
    {
      return Catalog == null
        ? string.Empty
        : Catalog.Get(key, Language, arguments);
    }
  }
}
