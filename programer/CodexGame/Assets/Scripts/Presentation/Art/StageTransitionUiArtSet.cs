using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class StageTransitionUiArtSet
  {
    [SerializeField] private Texture2D _exitClosedBackground;
    [SerializeField] private Texture2D _exitOpenBackground;
    [SerializeField] private Texture2D[] _leftDoorFrames = Array.Empty<Texture2D>();
    [SerializeField] private Texture2D[] _rightDoorFrames = Array.Empty<Texture2D>();
    [SerializeField] private Texture2D[] _dustFrames = Array.Empty<Texture2D>();
    [SerializeField] private Texture2D _vignette;
    [SerializeField] private Texture2D _fadeBlack;
    [SerializeField] private Texture2D[] _loadingFrames = Array.Empty<Texture2D>();

    public Texture2D ExitClosedBackground => _exitClosedBackground;
    public Texture2D ExitOpenBackground => _exitOpenBackground;
    public Texture2D[] LeftDoorFrames => _leftDoorFrames;
    public Texture2D[] RightDoorFrames => _rightDoorFrames;
    public Texture2D[] DustFrames => _dustFrames;
    public Texture2D Vignette => _vignette;
    public Texture2D FadeBlack => _fadeBlack;
    public Texture2D[] LoadingFrames => _loadingFrames;

    public bool IsComplete => _exitClosedBackground != null
      && _exitOpenBackground != null
      && HasFrames(_leftDoorFrames, 4)
      && HasFrames(_rightDoorFrames, 4)
      && HasFrames(_dustFrames, 4)
      && _vignette != null
      && _fadeBlack != null
      && HasFrames(_loadingFrames, 8);

    public StageTransitionUiArtSet()
    {
    }

    public StageTransitionUiArtSet(
      Texture2D exitClosedBackground,
      Texture2D exitOpenBackground,
      Texture2D[] leftDoorFrames,
      Texture2D[] rightDoorFrames,
      Texture2D[] dustFrames,
      Texture2D vignette,
      Texture2D fadeBlack,
      Texture2D[] loadingFrames)
    {
      _exitClosedBackground = RequireTexture(exitClosedBackground, nameof(exitClosedBackground));
      _exitOpenBackground = RequireTexture(exitOpenBackground, nameof(exitOpenBackground));
      _leftDoorFrames = RequireFrames(leftDoorFrames, 4, nameof(leftDoorFrames));
      _rightDoorFrames = RequireFrames(rightDoorFrames, 4, nameof(rightDoorFrames));
      _dustFrames = RequireFrames(dustFrames, 4, nameof(dustFrames));
      _vignette = RequireTexture(vignette, nameof(vignette));
      _fadeBlack = RequireTexture(fadeBlack, nameof(fadeBlack));
      _loadingFrames = RequireFrames(loadingFrames, 8, nameof(loadingFrames));
    }

    private static Texture2D RequireTexture(Texture2D texture, string parameterName)
    {
      return texture != null
        ? texture
        : throw new ArgumentNullException(parameterName);
    }

    private static Texture2D[] RequireFrames(
      Texture2D[] frames,
      int expectedCount,
      string parameterName)
    {
      if (!HasFrames(frames, expectedCount))
      {
        throw new ArgumentException(
          $"Exactly {expectedCount} non-null frames are required.",
          parameterName);
      }
      return (Texture2D[])frames.Clone();
    }

    private static bool HasFrames(Texture2D[] frames, int expectedCount)
    {
      if (frames == null || frames.Length != expectedCount) return false;
      for (var index = 0; index < frames.Length; index++)
      {
        if (frames[index] == null) return false;
      }
      return true;
    }
  }
}
