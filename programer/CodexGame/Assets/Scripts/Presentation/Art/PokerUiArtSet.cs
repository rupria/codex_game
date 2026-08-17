using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class PokerUiArtSet
  {
    [SerializeField] private Texture2D _winIdle;
    [SerializeField] private Texture2D _winHover;
    [SerializeField] private Texture2D _loseIdle;
    [SerializeField] private Texture2D _loseHover;
    [SerializeField] private Texture2D _itemSlot;
    [SerializeField] private Texture2D _playerPredictionIdle;
    [SerializeField] private Texture2D _playerPredictionHover;
    [SerializeField] private Texture2D _playerPredictionSelected;
    [SerializeField] private Texture2D _aiPredictionIdle;
    [SerializeField] private Texture2D _aiPredictionHover;
    [SerializeField] private Texture2D _aiPredictionSelected;

    public PokerUiArtSet(
      Texture2D winIdle,
      Texture2D winHover,
      Texture2D loseIdle,
      Texture2D loseHover,
      Texture2D itemSlot,
      Texture2D playerPredictionIdle = null,
      Texture2D playerPredictionHover = null,
      Texture2D playerPredictionSelected = null,
      Texture2D aiPredictionIdle = null,
      Texture2D aiPredictionHover = null,
      Texture2D aiPredictionSelected = null)
    {
      _winIdle = winIdle ?? throw new ArgumentNullException(nameof(winIdle));
      _winHover = winHover ?? throw new ArgumentNullException(nameof(winHover));
      _loseIdle = loseIdle ?? throw new ArgumentNullException(nameof(loseIdle));
      _loseHover = loseHover ?? throw new ArgumentNullException(nameof(loseHover));
      _itemSlot = itemSlot ?? throw new ArgumentNullException(nameof(itemSlot));
      _playerPredictionIdle = playerPredictionIdle;
      _playerPredictionHover = playerPredictionHover;
      _playerPredictionSelected = playerPredictionSelected;
      _aiPredictionIdle = aiPredictionIdle;
      _aiPredictionHover = aiPredictionHover;
      _aiPredictionSelected = aiPredictionSelected;
    }

    public Texture2D WinIdle => _winIdle;
    public Texture2D WinHover => _winHover;
    public Texture2D LoseIdle => _loseIdle;
    public Texture2D LoseHover => _loseHover;
    public Texture2D ItemSlot => _itemSlot;
    public Texture2D PlayerPredictionIdle => _playerPredictionIdle ?? _winIdle;
    public Texture2D PlayerPredictionHover => _playerPredictionHover ?? _winHover;
    public Texture2D PlayerPredictionSelected => _playerPredictionSelected ?? PlayerPredictionHover;
    public Texture2D AiPredictionIdle => _aiPredictionIdle ?? _loseIdle;
    public Texture2D AiPredictionHover => _aiPredictionHover ?? _loseHover;
    public Texture2D AiPredictionSelected => _aiPredictionSelected ?? AiPredictionHover;

    public bool IsComplete => _winIdle != null
      && _winHover != null
      && _loseIdle != null
      && _loseHover != null
      && _itemSlot != null;
  }
}
