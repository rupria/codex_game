using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class JokerRevealUiArtSet
  {
    [SerializeField] private Texture2D _focusVignette;
    [SerializeField] private Texture2D _arcTrailSheet;
    [SerializeField] private Texture2D _gunsightRingSheet;
    [SerializeField] private Texture2D _muzzleFlashSheet;
    [SerializeField] private Texture2D _cardGlintSheet;
    [SerializeField] private Texture2D _settleGlintSheet;

    public JokerRevealUiArtSet(
      Texture2D focusVignette,
      Texture2D arcTrailSheet,
      Texture2D gunsightRingSheet,
      Texture2D muzzleFlashSheet,
      Texture2D cardGlintSheet,
      Texture2D settleGlintSheet)
    {
      _focusVignette = Require(focusVignette, nameof(focusVignette));
      _arcTrailSheet = Require(arcTrailSheet, nameof(arcTrailSheet));
      _gunsightRingSheet = Require(gunsightRingSheet, nameof(gunsightRingSheet));
      _muzzleFlashSheet = Require(muzzleFlashSheet, nameof(muzzleFlashSheet));
      _cardGlintSheet = Require(cardGlintSheet, nameof(cardGlintSheet));
      _settleGlintSheet = Require(settleGlintSheet, nameof(settleGlintSheet));
    }

    public Texture2D FocusVignette => _focusVignette;
    public Texture2D ArcTrailSheet => _arcTrailSheet;
    public Texture2D GunsightRingSheet => _gunsightRingSheet;
    public Texture2D MuzzleFlashSheet => _muzzleFlashSheet;
    public Texture2D CardGlintSheet => _cardGlintSheet;
    public Texture2D SettleGlintSheet => _settleGlintSheet;
    public bool IsComplete => _focusVignette != null
      && _arcTrailSheet != null
      && _gunsightRingSheet != null
      && _muzzleFlashSheet != null
      && _cardGlintSheet != null
      && _settleGlintSheet != null;

    private static Texture2D Require(Texture2D texture, string parameterName)
    {
      return texture != null
        ? texture
        : throw new ArgumentNullException(parameterName);
    }
  }
}
