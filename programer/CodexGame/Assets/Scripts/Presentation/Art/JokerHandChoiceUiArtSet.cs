using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class JokerHandChoiceUiArtSet
  {
    [SerializeField] private Texture2D _dim;
    [SerializeField] private Texture2D _panel;
    [SerializeField] private Texture2D _optionIdle;
    [SerializeField] private Texture2D _optionHover;
    [SerializeField] private Texture2D _optionSelected;
    [SerializeField] private Texture2D _optionDisabled;

    public JokerHandChoiceUiArtSet(
      Texture2D dim,
      Texture2D panel,
      Texture2D optionIdle,
      Texture2D optionHover,
      Texture2D optionSelected,
      Texture2D optionDisabled)
    {
      _dim = dim ?? throw new ArgumentNullException(nameof(dim));
      _panel = panel ?? throw new ArgumentNullException(nameof(panel));
      _optionIdle = optionIdle ?? throw new ArgumentNullException(nameof(optionIdle));
      _optionHover = optionHover ?? throw new ArgumentNullException(nameof(optionHover));
      _optionSelected = optionSelected ?? throw new ArgumentNullException(nameof(optionSelected));
      _optionDisabled = optionDisabled ?? throw new ArgumentNullException(nameof(optionDisabled));
    }

    public Texture2D Dim => _dim;
    public Texture2D Panel => _panel;

    public Texture2D GetOptionTexture(bool enabled, bool hovered, bool selected)
    {
      if (!enabled) return _optionDisabled;
      if (selected) return _optionSelected;
      return hovered ? _optionHover : _optionIdle;
    }

    public bool IsComplete => _dim != null
      && _panel != null
      && _optionIdle != null
      && _optionHover != null
      && _optionSelected != null
      && _optionDisabled != null;
  }
}

