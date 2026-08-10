using System;

namespace CodexGame.Core.Items
{
  public sealed class GameItemDefinition
  {
    public GameItemDefinition(
      GameItemId id,
      string code,
      string localizationNameKey,
      string iconKey,
      int price,
      int configuredMagnitude = 0)
    {
      if (!Enum.IsDefined(typeof(GameItemId), id)) throw new ArgumentOutOfRangeException(nameof(id));
      if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Item code is required.", nameof(code));
      if (string.IsNullOrWhiteSpace(localizationNameKey))
      {
        throw new ArgumentException("Item localization key is required.", nameof(localizationNameKey));
      }
      if (string.IsNullOrWhiteSpace(iconKey)) throw new ArgumentException("Item icon key is required.", nameof(iconKey));
      if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
      if (configuredMagnitude < 0) throw new ArgumentOutOfRangeException(nameof(configuredMagnitude));

      Id = id;
      Code = code;
      LocalizationNameKey = localizationNameKey;
      IconKey = iconKey;
      Price = price;
      ConfiguredMagnitude = configuredMagnitude;
    }

    public GameItemId Id { get; }
    public string Code { get; }
    public string LocalizationNameKey { get; }
    public string IconKey { get; }
    public int Price { get; }
    public int ConfiguredMagnitude { get; }
  }
}
