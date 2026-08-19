using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class MainMenuButtonArtSet
  {
    [SerializeField] private Texture2D _idle;
    [SerializeField] private Texture2D _hover;
    [SerializeField] private Texture2D _pressed;

    public MainMenuButtonArtSet(Texture2D idle, Texture2D hover, Texture2D pressed)
    {
      _idle = idle ?? throw new ArgumentNullException(nameof(idle));
      _hover = hover ?? throw new ArgumentNullException(nameof(hover));
      _pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
    }

    public Texture2D GetTexture(bool hovered, bool pressed)
    {
      if (pressed) return _pressed;
      return hovered ? _hover : _idle;
    }

    public bool IsComplete => _idle != null && _hover != null && _pressed != null;
  }

  [Serializable]
  public sealed class MainMenuUiArtSet
  {
    [SerializeField] private MainMenuButtonArtSet _startButton;
    [SerializeField] private MainMenuButtonArtSet _guideButton;

    public MainMenuUiArtSet(
      MainMenuButtonArtSet startButton,
      MainMenuButtonArtSet guideButton)
    {
      _startButton = startButton ?? throw new ArgumentNullException(nameof(startButton));
      _guideButton = guideButton ?? throw new ArgumentNullException(nameof(guideButton));
    }

    public MainMenuButtonArtSet StartButton => _startButton;
    public MainMenuButtonArtSet GuideButton => _guideButton;

    public bool IsComplete => _startButton != null
      && _startButton.IsComplete
      && _guideButton != null
      && _guideButton.IsComplete;
  }
}
