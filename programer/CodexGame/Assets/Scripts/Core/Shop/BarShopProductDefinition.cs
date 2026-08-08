using System;

namespace CodexGame.Core.Shop
{
  public sealed class BarShopProductDefinition
  {
    public BarShopProductDefinition(
      string id,
      string localizationNameKey,
      string iconKey,
      int price,
      string effectKey,
      BarShopProductDisplayState displayState)
    {
      if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Product id is required.", nameof(id));
      if (string.IsNullOrWhiteSpace(localizationNameKey))
      {
        throw new ArgumentException("Product localization key is required.", nameof(localizationNameKey));
      }
      if (string.IsNullOrWhiteSpace(iconKey))
      {
        throw new ArgumentException("Product icon key is required.", nameof(iconKey));
      }
      if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
      if (string.IsNullOrWhiteSpace(effectKey))
      {
        throw new ArgumentException("Product effect key is required.", nameof(effectKey));
      }
      if (!Enum.IsDefined(typeof(BarShopProductDisplayState), displayState))
      {
        throw new ArgumentOutOfRangeException(nameof(displayState));
      }

      Id = id;
      LocalizationNameKey = localizationNameKey;
      IconKey = iconKey;
      Price = price;
      EffectKey = effectKey;
      DisplayState = displayState;
    }

    public string Id { get; }
    public string LocalizationNameKey { get; }
    public string IconKey { get; }
    public int Price { get; }
    public string EffectKey { get; }
    public BarShopProductDisplayState DisplayState { get; }
  }
}
