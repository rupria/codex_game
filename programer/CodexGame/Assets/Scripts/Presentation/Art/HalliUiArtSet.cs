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
    [SerializeField] private Texture2D _playerWinPipEmpty;
    [SerializeField] private Texture2D _playerWinPipFilled;
    [SerializeField] private Texture2D _aiWinPipEmpty;
    [SerializeField] private Texture2D _aiWinPipFilled;
    [SerializeField] private Texture2D _ropeBody;
    [SerializeField] private Texture2D _ropeCharCap;
    [SerializeField] private Texture2D _ropeFlame;
    [SerializeField] private Texture2D _ropeExplosion;
    [SerializeField] private Texture2D _sharedPileRailIdle;
    [SerializeField] private Texture2D _sharedPileRailPlayerActive;
    [SerializeField] private Texture2D _sharedPileRailAiActive;
    [SerializeField] private Texture2D _playerOnlyAcquiredTray;
    [SerializeField] private Texture2D _aiThinkingSheet;

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
      Texture2D aiAcquiredStatusPanel,
      Texture2D playerWinPipEmpty = null,
      Texture2D playerWinPipFilled = null,
      Texture2D aiWinPipEmpty = null,
      Texture2D aiWinPipFilled = null,
      Texture2D ropeBody = null,
      Texture2D ropeCharCap = null,
      Texture2D ropeFlame = null,
      Texture2D ropeExplosion = null,
      Texture2D sharedPileRailIdle = null,
      Texture2D sharedPileRailPlayerActive = null,
      Texture2D sharedPileRailAiActive = null,
      Texture2D playerOnlyAcquiredTray = null,
      Texture2D aiThinkingSheet = null)
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
      _playerWinPipEmpty = playerWinPipEmpty;
      _playerWinPipFilled = playerWinPipFilled;
      _aiWinPipEmpty = aiWinPipEmpty;
      _aiWinPipFilled = aiWinPipFilled;
      _ropeBody = ropeBody;
      _ropeCharCap = ropeCharCap;
      _ropeFlame = ropeFlame;
      _ropeExplosion = ropeExplosion;
      _sharedPileRailIdle = sharedPileRailIdle;
      _sharedPileRailPlayerActive = sharedPileRailPlayerActive;
      _sharedPileRailAiActive = sharedPileRailAiActive;
      _playerOnlyAcquiredTray = playerOnlyAcquiredTray;
      _aiThinkingSheet = aiThinkingSheet;
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
    public Texture2D PlayerAcquiredTray => _playerOnlyAcquiredTray ?? _playerAcquiredTray;
    public Texture2D AiAcquiredStatusPanel => _aiAcquiredStatusPanel;
    public Texture2D PlayerWinPipEmpty => _playerWinPipEmpty;
    public Texture2D PlayerWinPipFilled => _playerWinPipFilled;
    public Texture2D AiWinPipEmpty => _aiWinPipEmpty;
    public Texture2D AiWinPipFilled => _aiWinPipFilled;
    public Texture2D RopeBody => _ropeBody;
    public Texture2D RopeCharCap => _ropeCharCap;
    public Texture2D RopeFlame => _ropeFlame;
    public Texture2D RopeExplosion => _ropeExplosion;
    public Texture2D SharedPileRailIdle => _sharedPileRailIdle;
    public Texture2D SharedPileRailPlayerActive => _sharedPileRailPlayerActive;
    public Texture2D SharedPileRailAiActive => _sharedPileRailAiActive;
    public Texture2D AiThinkingSheet => _aiThinkingSheet;
    public bool UsesPlayerOnlyLowerHud => _playerOnlyAcquiredTray != null;

    public bool HasRoundWinPips => _playerWinPipEmpty != null
      && _playerWinPipFilled != null
      && _aiWinPipEmpty != null
      && _aiWinPipFilled != null;

    public bool HasRopeTimerArt => _ropeBody != null
      && _ropeCharCap != null
      && _ropeFlame != null
      && _ropeExplosion != null;

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
