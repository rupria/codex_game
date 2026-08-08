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
  }
}
