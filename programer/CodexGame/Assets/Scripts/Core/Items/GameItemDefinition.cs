using System;

namespace CodexGame.Core.Items
{
  public sealed class GameItemDefinition
  {
    public GameItemDefinition(
      GameItemId id,
      string code,
      string localizationNameKey,
      string localizationDescriptionKey,
      string iconKey,
      int price,
      GameItemTargetMode targetMode,
      GameItemEffectType effectType,
      GameItemUseTiming useTiming,
      string presentationKey,
      int configuredMagnitude = 0,
      int shopWeight = 1)
    {
      if (!Enum.IsDefined(typeof(GameItemId), id)) throw new ArgumentOutOfRangeException(nameof(id));
      if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Item code is required.", nameof(code));
      if (string.IsNullOrWhiteSpace(localizationNameKey))
      {
        throw new ArgumentException("Item localization key is required.", nameof(localizationNameKey));
      }
      if (string.IsNullOrWhiteSpace(localizationDescriptionKey))
      {
        throw new ArgumentException(
          "Item description localization key is required.",
          nameof(localizationDescriptionKey));
      }
      if (string.IsNullOrWhiteSpace(iconKey)) throw new ArgumentException("Item icon key is required.", nameof(iconKey));
      if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
      if (!Enum.IsDefined(typeof(GameItemTargetMode), targetMode))
      {
        throw new ArgumentOutOfRangeException(nameof(targetMode));
      }
      if (!Enum.IsDefined(typeof(GameItemEffectType), effectType))
      {
        throw new ArgumentOutOfRangeException(nameof(effectType));
      }
      if (!Enum.IsDefined(typeof(GameItemUseTiming), useTiming))
      {
        throw new ArgumentOutOfRangeException(nameof(useTiming));
      }
      if (string.IsNullOrWhiteSpace(presentationKey))
      {
        throw new ArgumentException("Item presentation key is required.", nameof(presentationKey));
      }
      if (configuredMagnitude < 0) throw new ArgumentOutOfRangeException(nameof(configuredMagnitude));
      if (shopWeight < 1) throw new ArgumentOutOfRangeException(nameof(shopWeight));

      Id = id;
      Code = code;
      LocalizationNameKey = localizationNameKey;
      LocalizationDescriptionKey = localizationDescriptionKey;
      IconKey = iconKey;
      Price = price;
      TargetMode = targetMode;
      EffectType = effectType;
      UseTiming = useTiming;
      PresentationKey = presentationKey;
      ConfiguredMagnitude = configuredMagnitude;
      ShopWeight = shopWeight;
    }

    public GameItemId Id { get; }
    public string Code { get; }
    public string LocalizationNameKey { get; }
    public string LocalizationDescriptionKey { get; }
    public string IconKey { get; }
    public int Price { get; }
    public GameItemTargetMode TargetMode { get; }
    public GameItemEffectType EffectType { get; }
    public GameItemUseTiming UseTiming { get; }
    public string PresentationKey { get; }
    public int ConfiguredMagnitude { get; }
    public int ShopWeight { get; }
  }

  public enum GameItemTargetMode
  {
    None = 0,
    PlayerCard = 1,
    PlayerCardAndSuit = 2,
    PlayerAndAiCardPair = 3
  }

  public enum GameItemEffectType
  {
    ExchangeOne = 0,
    ChooseReplacement = 1,
    RevealAiCard = 2,
    RecoverHealth = 3,
    OverrideSuit = 4,
    PreventShowdownDamage = 5,
    InsurePrediction = 6,
    ExchangeBothSides = 7
  }

  public enum GameItemUseTiming
  {
    None = 0,
    BeforePublicCards = 1,
    DuringPrivateSelection = 2,
    AfterPublicCardsAndPrivateSelectionBeforePrediction = 3,
    DuringPrediction = 4
  }

  public static class GameItemUseTimingPolicy
  {
    public static bool IsUsable(
      GameItemDefinition definition,
      GameItemUseTiming currentTiming)
    {
      if (definition == null) throw new ArgumentNullException(nameof(definition));
      return definition.UseTiming != GameItemUseTiming.None
        && definition.UseTiming == currentTiming;
    }

    public static string LocalizationKey(GameItemUseTiming timing)
    {
      switch (timing)
      {
        case GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction:
          return "UI_ITEM_USE_TIMING_BEFORE_PREDICTION";
        default:
          return "UI_ITEM_ACTION_FAILED";
      }
    }
  }
}
