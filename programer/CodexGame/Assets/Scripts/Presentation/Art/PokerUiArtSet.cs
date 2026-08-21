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
    [SerializeField] private Texture2D _predictionResultEmpty;
    [SerializeField] private Texture2D _predictionResultFilled;
    [SerializeField] private Texture2D _playerPredictionDisabled;
    [SerializeField] private Texture2D _aiPredictionDisabled;
    [SerializeField] private Texture2D _predictionTitlePlate;
    [SerializeField] private Texture2D _predictionStageEmblem;
    [SerializeField] private Texture2D _insuranceRemainingIcon;
    [SerializeField] private Texture2D _predictionSuccessIcon;
    [SerializeField] private Texture2D _resultContinueIdle;
    [SerializeField] private Texture2D _resultContinueHover;

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
      Texture2D aiPredictionSelected = null,
      Texture2D predictionResultEmpty = null,
      Texture2D predictionResultFilled = null,
      Texture2D playerPredictionDisabled = null,
      Texture2D aiPredictionDisabled = null,
      Texture2D predictionTitlePlate = null,
      Texture2D predictionStageEmblem = null,
      Texture2D insuranceRemainingIcon = null,
      Texture2D predictionSuccessIcon = null,
      Texture2D resultContinueIdle = null,
      Texture2D resultContinueHover = null)
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
      _predictionResultEmpty = predictionResultEmpty;
      _predictionResultFilled = predictionResultFilled;
      _playerPredictionDisabled = playerPredictionDisabled;
      _aiPredictionDisabled = aiPredictionDisabled;
      _predictionTitlePlate = predictionTitlePlate;
      _predictionStageEmblem = predictionStageEmblem;
      _insuranceRemainingIcon = insuranceRemainingIcon;
      _predictionSuccessIcon = predictionSuccessIcon;
      _resultContinueIdle = resultContinueIdle;
      _resultContinueHover = resultContinueHover;
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
    public Texture2D PredictionResultEmpty => _predictionResultEmpty;
    public Texture2D PredictionResultFilled => _predictionResultFilled;
    public Texture2D PlayerPredictionDisabled => _playerPredictionDisabled ?? PlayerPredictionIdle;
    public Texture2D AiPredictionDisabled => _aiPredictionDisabled ?? AiPredictionIdle;
    public Texture2D PredictionTitlePlate => _predictionTitlePlate;
    public Texture2D PredictionStageEmblem => _predictionStageEmblem;
    public Texture2D InsuranceRemainingIcon => _insuranceRemainingIcon;
    public Texture2D PredictionSuccessIcon => _predictionSuccessIcon;
    public Texture2D ResultContinueIdle => _resultContinueIdle;
    public Texture2D ResultContinueHover => _resultContinueHover;

    public bool IsComplete => _winIdle != null
      && _winHover != null
      && _loseIdle != null
      && _loseHover != null
      && _itemSlot != null;
  }
}
