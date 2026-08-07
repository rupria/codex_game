using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class HalliUiArtSet
  {
    [SerializeField] private Texture2D _bellIdle;
    [SerializeField] private Texture2D _bellHover;
    [SerializeField] private Texture2D _bellPressed;
    [SerializeField] private Texture2D _bellWrong;
    [SerializeField] private Texture2D _publicCardLockedSlot;
    [SerializeField] private Texture2D _flipTimer;

    public HalliUiArtSet(
      Texture2D bellIdle,
      Texture2D bellHover,
      Texture2D bellPressed,
      Texture2D bellWrong,
      Texture2D publicCardLockedSlot,
      Texture2D flipTimer)
    {
      _bellIdle = bellIdle ?? throw new ArgumentNullException(nameof(bellIdle));
      _bellHover = bellHover ?? throw new ArgumentNullException(nameof(bellHover));
      _bellPressed = bellPressed ?? throw new ArgumentNullException(nameof(bellPressed));
      _bellWrong = bellWrong ?? throw new ArgumentNullException(nameof(bellWrong));
      _publicCardLockedSlot = publicCardLockedSlot
        ?? throw new ArgumentNullException(nameof(publicCardLockedSlot));
      _flipTimer = flipTimer ?? throw new ArgumentNullException(nameof(flipTimer));
    }

    public Texture2D BellIdle => _bellIdle;
    public Texture2D BellHover => _bellHover;
    public Texture2D BellPressed => _bellPressed;
    public Texture2D BellWrong => _bellWrong;
    public Texture2D PublicCardLockedSlot => _publicCardLockedSlot;
    public Texture2D FlipTimer => _flipTimer;

    public bool IsComplete => _bellIdle != null
      && _bellHover != null
      && _bellPressed != null
      && _bellWrong != null
      && _publicCardLockedSlot != null
      && _flipTimer != null;
  }
}
