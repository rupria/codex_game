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
      Texture2D exitWarningIcon)
    {
      _baseCurrencyIcon = baseCurrencyIcon;
      _temporaryCurrencyIcon = temporaryCurrencyIcon;
      _priceIcon = priceIcon;
      _baseBalanceFrame = baseBalanceFrame;
      _temporaryBalanceFrame = temporaryBalanceFrame;
      _baseRewardFrame = baseRewardFrame;
      _temporaryRewardFrame = temporaryRewardFrame;
      _exitWarningIcon = exitWarningIcon;
    }

    public Texture2D BaseCurrencyIcon => _baseCurrencyIcon;
    public Texture2D TemporaryCurrencyIcon => _temporaryCurrencyIcon;
    public Texture2D PriceIcon => _priceIcon != null ? _priceIcon : _baseCurrencyIcon;
    public Texture2D BaseBalanceFrame => _baseBalanceFrame;
    public Texture2D TemporaryBalanceFrame => _temporaryBalanceFrame;
    public Texture2D BaseRewardFrame => _baseRewardFrame;
    public Texture2D TemporaryRewardFrame => _temporaryRewardFrame;
    public Texture2D ExitWarningIcon => _exitWarningIcon;

    public bool HasDualCurrencyIcons => _baseCurrencyIcon != null
      && _temporaryCurrencyIcon != null;
  }
}
