using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class EconomyUiArtSet
  {
    [SerializeField] private Texture2D _baseCurrencyIcon;
    [SerializeField] private Texture2D _temporaryCurrencyIcon;
    [SerializeField] private Texture2D _priceIcon;
    [SerializeField] private Texture2D _baseBalanceFrame;
    [SerializeField] private Texture2D _temporaryBalanceFrame;
    [SerializeField] private Texture2D _baseRewardFrame;
    [SerializeField] private Texture2D _temporaryRewardFrame;
    [SerializeField] private Texture2D _exitWarningIcon;
    [SerializeField] private Texture2D _temporaryExpireSheet;
    [SerializeField] private Texture2D _exitWarningPulseSheet;
    [SerializeField] private Texture2D _stageRewardSummaryPanel;
    [SerializeField] private Texture2D _stageRewardContentBackground;
    [SerializeField] private Texture2D _stageRewardBaseRow;
    [SerializeField] private Texture2D _stageRewardPredictionRow;
    [SerializeField] private Texture2D _stageRewardNeutralRow;
    [SerializeField] private Texture2D _stageRewardTotalRow;
    [SerializeField] private Texture2D _stageRewardContinueIdle;
    [SerializeField] private Texture2D _stageRewardContinueHover;
    [SerializeField] private Texture2D _stageRewardContinuePressed;
    [SerializeField] private Texture2D _stageRewardContinueDisabled;

    public EconomyUiArtSet()
    {
    }

    public EconomyUiArtSet(
      Texture2D baseCurrencyIcon,
      Texture2D temporaryCurrencyIcon,
      Texture2D priceIcon,
      Texture2D baseBalanceFrame,
      Texture2D temporaryBalanceFrame,
      Texture2D baseRewardFrame,
      Texture2D temporaryRewardFrame,
      Texture2D exitWarningIcon,
      Texture2D temporaryExpireSheet = null,
      Texture2D exitWarningPulseSheet = null,
      Texture2D stageRewardSummaryPanel = null,
      Texture2D stageRewardContentBackground = null,
      Texture2D stageRewardBaseRow = null,
      Texture2D stageRewardPredictionRow = null,
      Texture2D stageRewardNeutralRow = null,
      Texture2D stageRewardTotalRow = null,
      Texture2D stageRewardContinueIdle = null,
      Texture2D stageRewardContinueHover = null,
      Texture2D stageRewardContinuePressed = null,
      Texture2D stageRewardContinueDisabled = null)
    {
      _baseCurrencyIcon = baseCurrencyIcon;
      _temporaryCurrencyIcon = temporaryCurrencyIcon;
      _priceIcon = priceIcon;
      _baseBalanceFrame = baseBalanceFrame;
      _temporaryBalanceFrame = temporaryBalanceFrame;
      _baseRewardFrame = baseRewardFrame;
      _temporaryRewardFrame = temporaryRewardFrame;
      _exitWarningIcon = exitWarningIcon;
      _temporaryExpireSheet = temporaryExpireSheet;
      _exitWarningPulseSheet = exitWarningPulseSheet;
      _stageRewardSummaryPanel = stageRewardSummaryPanel;
      _stageRewardContentBackground = stageRewardContentBackground;
      _stageRewardBaseRow = stageRewardBaseRow;
      _stageRewardPredictionRow = stageRewardPredictionRow;
      _stageRewardNeutralRow = stageRewardNeutralRow;
      _stageRewardTotalRow = stageRewardTotalRow;
      _stageRewardContinueIdle = stageRewardContinueIdle;
      _stageRewardContinueHover = stageRewardContinueHover;
      _stageRewardContinuePressed = stageRewardContinuePressed;
      _stageRewardContinueDisabled = stageRewardContinueDisabled;
    }

    public Texture2D BaseCurrencyIcon => _baseCurrencyIcon;
    public Texture2D TemporaryCurrencyIcon => _temporaryCurrencyIcon;
    public Texture2D PriceIcon => _priceIcon != null ? _priceIcon : _baseCurrencyIcon;
    public Texture2D BaseBalanceFrame => _baseBalanceFrame;
    public Texture2D TemporaryBalanceFrame => _temporaryBalanceFrame;
    public Texture2D BaseRewardFrame => _baseRewardFrame;
    public Texture2D TemporaryRewardFrame => _temporaryRewardFrame;
    public Texture2D ExitWarningIcon => _exitWarningIcon;
    public Texture2D TemporaryExpireSheet => _temporaryExpireSheet;
    public Texture2D ExitWarningPulseSheet => _exitWarningPulseSheet;
    public Texture2D StageRewardSummaryPanel => _stageRewardSummaryPanel;
    public Texture2D StageRewardContentBackground => _stageRewardContentBackground;
    public Texture2D StageRewardBaseRow => _stageRewardBaseRow ?? _baseRewardFrame;
    public Texture2D StageRewardPredictionRow => _stageRewardPredictionRow ?? _temporaryRewardFrame;
    public Texture2D StageRewardNeutralRow => _stageRewardNeutralRow;
    public Texture2D StageRewardTotalRow => _stageRewardTotalRow;
    public Texture2D StageRewardContinueIdle => _stageRewardContinueIdle;
    public Texture2D StageRewardContinueHover => _stageRewardContinueHover ?? _stageRewardContinueIdle;
    public Texture2D StageRewardContinuePressed => _stageRewardContinuePressed ?? StageRewardContinueHover;
    public Texture2D StageRewardContinueDisabled => _stageRewardContinueDisabled ?? _stageRewardContinueIdle;

    public bool HasDualCurrencyIcons => _baseCurrencyIcon != null
      && _temporaryCurrencyIcon != null;
  }
}
