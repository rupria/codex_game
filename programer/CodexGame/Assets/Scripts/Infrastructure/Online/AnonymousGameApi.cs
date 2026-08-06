using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CodexGame.Infrastructure.Online
{
  [Serializable]
  public sealed class MatchResultRequest
  {
    public string matchId;
    public string result;
    public int stage;
    public int durationMs;
    public int inputTimeMs;
    public string contentVersion;
  }

  [Serializable]
  internal sealed class MatchResultQueue
  {
    public List<MatchResultRequest> items = new List<MatchResultRequest>();
  }

  public sealed class AnonymousGameApi : MonoBehaviour
  {
    private const string QueueKey = "codex_game.pending_matches.v1";
    private const int MaximumQueuedMatches = 20;

    [SerializeField] private string apiBasePath = "/api";
    [SerializeField] private string editorApiOrigin = "http://127.0.0.1:8788";
    [SerializeField] private int requestTimeoutSeconds = 10;

    private bool isFlushing;

    private void Start()
    {
      StartCoroutine(InitializeSessionAndFlush());
    }

    public void RecordMatch(MatchResultRequest result)
    {
      if (result == null)
      {
        throw new ArgumentNullException(nameof(result));
      }

      if (string.IsNullOrWhiteSpace(result.matchId))
      {
        result.matchId = Guid.NewGuid().ToString();
      }

      StartCoroutine(PostOrQueue(result));
    }

    public void FlushPendingMatches()
    {
      if (!isFlushing)
      {
        StartCoroutine(FlushQueue());
      }
    }

    private IEnumerator InitializeSessionAndFlush()
    {
      using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl("/session")))
      {
        request.timeout = requestTimeoutSeconds;
        yield return request.SendWebRequest();
      }

      yield return FlushQueue();
    }

    private IEnumerator PostOrQueue(MatchResultRequest result)
    {
      bool succeeded = false;
      yield return SendMatch(result, value => succeeded = value);
      if (!succeeded)
      {
        Enqueue(result);
      }
    }

    private IEnumerator FlushQueue()
    {
      isFlushing = true;
      MatchResultQueue queue = LoadQueue();

      while (queue.items.Count > 0)
      {
        bool succeeded = false;
        yield return SendMatch(queue.items[0], value => succeeded = value);
        if (!succeeded)
        {
          break;
        }

        queue.items.RemoveAt(0);
        SaveQueue(queue);
      }

      isFlushing = false;
    }

    private IEnumerator SendMatch(
      MatchResultRequest result,
      Action<bool> completed)
    {
      byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(result));
      using (UnityWebRequest request = new UnityWebRequest(
        BuildUrl("/matches"),
        UnityWebRequest.kHttpVerbPOST))
      {
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = requestTimeoutSeconds;
        yield return request.SendWebRequest();

        bool succeeded =
          request.result == UnityWebRequest.Result.Success &&
          request.responseCode >= 200 &&
          request.responseCode < 300;
        completed(succeeded);
      }
    }

    private string BuildUrl(string route)
    {
      string relativePath =
        string.Concat(apiBasePath.TrimEnd('/'), "/", route.TrimStart('/'));

      if (!string.IsNullOrEmpty(Application.absoluteURL))
      {
        return new Uri(new Uri(Application.absoluteURL), relativePath).ToString();
      }

      return string.Concat(
        editorApiOrigin.TrimEnd('/'),
        "/",
        relativePath.TrimStart('/'));
    }

    private static MatchResultQueue LoadQueue()
    {
      string json = PlayerPrefs.GetString(QueueKey, string.Empty);
      if (string.IsNullOrEmpty(json))
      {
        return new MatchResultQueue();
      }

      MatchResultQueue queue = JsonUtility.FromJson<MatchResultQueue>(json);
      return queue ?? new MatchResultQueue();
    }

    private static void Enqueue(MatchResultRequest result)
    {
      MatchResultQueue queue = LoadQueue();
      queue.items.Add(result);

      while (queue.items.Count > MaximumQueuedMatches)
      {
        queue.items.RemoveAt(0);
      }

      SaveQueue(queue);
    }

    private static void SaveQueue(MatchResultQueue queue)
    {
      if (queue.items.Count == 0)
      {
        PlayerPrefs.DeleteKey(QueueKey);
      }
      else
      {
        PlayerPrefs.SetString(QueueKey, JsonUtility.ToJson(queue));
      }

      PlayerPrefs.Save();
    }
  }
}
