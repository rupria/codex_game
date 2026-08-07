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
    [SerializeField] private Texture2D _bellCorrect;
    [SerializeField] private Texture2D _bellDisabled;
    [SerializeField] private Texture2D _publicCardLockedSlot;
    [SerializeField] private Texture2D _flipTimer;
    [SerializeField] private Texture2D _flipDeckIdle;
    [SerializeField] private Texture2D _flipDeckHover;
    [SerializeField] private Texture2D _flipDeckPressed;
    [SerializeField] private Texture2D _flipDeckDisabled;
    [SerializeField] private Texture2D _playerAcquiredTray;
    [SerializeField] private Texture2D _aiAcquiredStatusPanel;

    public HalliUiArtSet(
      Texture2D bellIdle,
      Texture2D bellHover,
      Texture2D bellPressed,
      Texture2D bellWrong,
      Texture2D bellCorrect,
      Texture2D bellDisabled,
      Texture2D publicCardLockedSlot,
      Texture2D flipTimer,
      Texture2D flipDeckIdle,
      Texture2D flipDeckHover,
      Texture2D flipDeckPressed,
      Texture2D flipDeckDisabled,
      Texture2D playerAcquiredTray,
      Texture2D aiAcquiredStatusPanel)
    {
      _bellIdle = bellIdle ?? throw new ArgumentNullException(nameof(bellIdle));
      _bellHover = bellHover ?? throw new ArgumentNullException(nameof(bellHover));
      _bellPressed = bellPressed ?? throw new ArgumentNullException(nameof(bellPressed));
      _bellWrong = bellWrong ?? throw new ArgumentNullException(nameof(bellWrong));
      _bellCorrect = bellCorrect ?? throw new ArgumentNullException(nameof(bellCorrect));
      _bellDisabled = bellDisabled ?? throw new ArgumentNullException(nameof(bellDisabled));
      _publicCardLockedSlot = publicCardLockedSlot
        ?? throw new ArgumentNullException(nameof(publicCardLockedSlot));
      _flipTimer = flipTimer ?? throw new ArgumentNullException(nameof(flipTimer));
      _flipDeckIdle = flipDeckIdle ?? throw new ArgumentNullException(nameof(flipDeckIdle));
      _flipDeckHover = flipDeckHover ?? throw new ArgumentNullException(nameof(flipDeckHover));
      _flipDeckPressed = flipDeckPressed ?? throw new ArgumentNullException(nameof(flipDeckPressed));
      _flipDeckDisabled = flipDeckDisabled ?? throw new ArgumentNullException(nameof(flipDeckDisabled));
      _playerAcquiredTray = playerAcquiredTray
        ?? throw new ArgumentNullException(nameof(playerAcquiredTray));
      _aiAcquiredStatusPanel = aiAcquiredStatusPanel
        ?? throw new ArgumentNullException(nameof(aiAcquiredStatusPanel));
    }

    public Texture2D BellIdle => _bellIdle;
    public Texture2D BellHover => _bellHover;
    public Texture2D BellPressed => _bellPressed;
    public Texture2D BellWrong => _bellWrong;
    public Texture2D BellCorrect => _bellCorrect;
    public Texture2D BellDisabled => _bellDisabled;
    public Texture2D PublicCardLockedSlot => _publicCardLockedSlot;
    public Texture2D FlipTimer => _flipTimer;
    public Texture2D FlipDeckIdle => _flipDeckIdle;
    public Texture2D FlipDeckHover => _flipDeckHover;
    public Texture2D FlipDeckPressed => _flipDeckPressed;
    public Texture2D FlipDeckDisabled => _flipDeckDisabled;
    public Texture2D PlayerAcquiredTray => _playerAcquiredTray;
    public Texture2D AiAcquiredStatusPanel => _aiAcquiredStatusPanel;

    public bool IsComplete => _bellIdle != null
      && _bellHover != null
      && _bellPressed != null
      && _bellWrong != null
      && _bellCorrect != null
      && _bellDisabled != null
      && _publicCardLockedSlot != null
      && _flipTimer != null
      && _flipDeckIdle != null
      && _flipDeckHover != null
      && _flipDeckPressed != null
      && _flipDeckDisabled != null
      && _playerAcquiredTray != null
      && _aiAcquiredStatusPanel != null;
  }
}
