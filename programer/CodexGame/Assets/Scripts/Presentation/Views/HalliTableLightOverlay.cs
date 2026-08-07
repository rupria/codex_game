using UnityEngine;

namespace CodexGame.Presentation.Views
{
  internal sealed class HalliTableLightOverlay
  {
    private const int TextureSize = 128;
    private Texture2D _radialTexture;

    public void Draw(float timeSeconds, float intensity = 1f)
    {
      EnsureTexture();
      var previousColor = GUI.color;

      var warmX = 480f + Mathf.Sin(timeSeconds * 0.17f) * 46f;
      var warmY = 236f + Mathf.Cos(timeSeconds * 0.13f) * 24f;
      DrawLight(
        new Rect(warmX - 330f, warmY - 210f, 660f, 420f),
        new Color(1f, 0.78f, 0.48f, 0.095f * intensity));

      var coolX = 455f + Mathf.Sin(timeSeconds * 0.11f + 1.8f) * 70f;
      var coolY = 260f + Mathf.Cos(timeSeconds * 0.09f + 0.7f) * 32f;
      DrawLight(
        new Rect(coolX - 285f, coolY - 175f, 570f, 350f),
        new Color(0.34f, 0.82f, 1f, 0.055f * intensity));

      GUI.color = previousColor;
    }

    private void DrawLight(Rect rect, Color color)
    {
      GUI.color = color;
      GUI.DrawTexture(rect, _radialTexture, ScaleMode.StretchToFill, true);
    }

    private void EnsureTexture()
    {
      if (_radialTexture != null) return;

      _radialTexture = new Texture2D(
        TextureSize,
        TextureSize,
        TextureFormat.RGBA32,
        false)
      {
        name = "Runtime Table Light",
        filterMode = FilterMode.Bilinear,
        wrapMode = TextureWrapMode.Clamp,
        hideFlags = HideFlags.HideAndDontSave
      };

      var pixels = new Color32[TextureSize * TextureSize];
      for (var y = 0; y < TextureSize; y++)
      {
        for (var x = 0; x < TextureSize; x++)
        {
          var normalizedX = ((x + 0.5f) / TextureSize - 0.5f) * 2f;
          var normalizedY = ((y + 0.5f) / TextureSize - 0.5f) * 2f;
          var distance = Mathf.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
          var falloff = 1f - Mathf.SmoothStep(0.05f, 1f, distance);
          var alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(falloff * falloff) * 255f);
          pixels[y * TextureSize + x] = new Color32(255, 255, 255, alpha);
        }
      }

      _radialTexture.SetPixels32(pixels);
      _radialTexture.Apply(false, true);
    }
  }
}
