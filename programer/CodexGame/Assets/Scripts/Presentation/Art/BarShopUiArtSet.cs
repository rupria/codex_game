using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class BarShopProductIconBinding
  {
    [SerializeField] private string _iconKey;
    [SerializeField] private Texture2D _texture;

    public string IconKey => _iconKey;
    public Texture2D Texture => _texture;

    public BarShopProductIconBinding()
    {
    }

    public BarShopProductIconBinding(string iconKey, Texture2D texture)
    {
      if (string.IsNullOrWhiteSpace(iconKey))
      {
        throw new ArgumentException("An icon key is required.", nameof(iconKey));
      }

      _iconKey = iconKey;
      _texture = texture != null
        ? texture
        : throw new ArgumentNullException(nameof(texture));
    }
  }

  [Serializable]
  public sealed class BarShopUiArtSet
  {
    [SerializeField] private Texture2D _background;
    [SerializeField] private Texture2D _slotFrame;
    [SerializeField] private Texture2D _rerollIdle;
    [SerializeField] private Texture2D _rerollHover;
    [SerializeField] private Texture2D _rerollPressed;
    [SerializeField] private Texture2D _rerollDisabled;
    [SerializeField] private Texture2D _continueIdle;
    [SerializeField] private Texture2D _continueHover;
    [SerializeField] private Texture2D _continuePressed;
    [SerializeField] private Texture2D _bulletPanel;
    [SerializeField] private Texture2D _healthPanel;
    [SerializeField] private List<BarShopProductIconBinding> _productIcons =
      new List<BarShopProductIconBinding>();

    public Texture2D Background => _background;
    public Texture2D SlotFrame => _slotFrame;
    public Texture2D RerollIdle => _rerollIdle;
    public Texture2D RerollHover => _rerollHover;
    public Texture2D RerollPressed => _rerollPressed;
    public Texture2D RerollDisabled => _rerollDisabled;
    public Texture2D ContinueIdle => _continueIdle;
    public Texture2D ContinueHover => _continueHover;
    public Texture2D ContinuePressed => _continuePressed;
    public Texture2D BulletPanel => _bulletPanel;
    public Texture2D HealthPanel => _healthPanel;

    public BarShopUiArtSet()
    {
    }

    public BarShopUiArtSet(
      Texture2D background,
      Texture2D slotFrame,
      Texture2D rerollIdle,
      Texture2D rerollHover,
      Texture2D rerollPressed,
      Texture2D rerollDisabled,
      Texture2D continueIdle,
      Texture2D continueHover,
      Texture2D continuePressed,
      Texture2D bulletPanel,
      Texture2D healthPanel,
      IReadOnlyList<BarShopProductIconBinding> productIcons)
    {
      _background = RequireTexture(background, nameof(background));
      _slotFrame = RequireTexture(slotFrame, nameof(slotFrame));
      _rerollIdle = RequireTexture(rerollIdle, nameof(rerollIdle));
      _rerollHover = RequireTexture(rerollHover, nameof(rerollHover));
      _rerollPressed = RequireTexture(rerollPressed, nameof(rerollPressed));
      _rerollDisabled = RequireTexture(rerollDisabled, nameof(rerollDisabled));
      _continueIdle = RequireTexture(continueIdle, nameof(continueIdle));
      _continueHover = RequireTexture(continueHover, nameof(continueHover));
      _continuePressed = RequireTexture(continuePressed, nameof(continuePressed));
      _bulletPanel = RequireTexture(bulletPanel, nameof(bulletPanel));
      _healthPanel = RequireTexture(healthPanel, nameof(healthPanel));
      _productIcons = productIcons != null
        ? new List<BarShopProductIconBinding>(productIcons)
        : throw new ArgumentNullException(nameof(productIcons));
    }

    public Texture2D FindProductIcon(string iconKey)
    {
      if (string.IsNullOrEmpty(iconKey) || _productIcons == null) return null;
      for (var index = 0; index < _productIcons.Count; index++)
      {
        var binding = _productIcons[index];
        if (binding != null
          && string.Equals(binding.IconKey, iconKey, StringComparison.Ordinal))
        {
          return binding.Texture;
        }
      }
      return null;
    }

    private static Texture2D RequireTexture(Texture2D texture, string parameterName)
    {
      return texture != null
        ? texture
        : throw new ArgumentNullException(parameterName);
    }
  }
}
