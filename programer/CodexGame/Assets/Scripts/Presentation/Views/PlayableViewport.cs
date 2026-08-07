using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal static class PlayableViewport
  {
    public const float Width = 960f;
    public const float Height = 540f;

    public static void Apply()
    {
      var scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
      var offset = new Vector3(
        (Screen.width - Width * scale) * 0.5f,
        (Screen.height - Height * scale) * 0.5f,
        0f);
      GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1f));
    }
  }
}
