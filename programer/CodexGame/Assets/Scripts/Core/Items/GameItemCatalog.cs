#nullable enable
using System;
using System.Collections.Generic;

namespace CodexGame.Core.Items
{
  public static class GameItemCatalog
  {
    private static readonly IReadOnlyList<GameItemDefinition> RegisteredItems =
      Array.AsReadOnly(new[]
      {
        new GameItemDefinition(
          GameItemId.Reload,
          "IT-01",
          "UI_ITEM_RELOAD",
          "UI_ITEM_RELOAD_DESC",
          "bar_shop.item.reload",
          1,
          GameItemTargetMode.PlayerCard,
          GameItemEffectType.ExchangeOne,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.reload"),
        new GameItemDefinition(
          GameItemId.BottomDeal,
          "IT-02",
          "UI_ITEM_BOTTOM_DEAL",
          "UI_ITEM_BOTTOM_DEAL_DESC",
          "bar_shop.item.bottom_deal",
          2,
          GameItemTargetMode.PlayerCard,
          GameItemEffectType.ChooseReplacement,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.bottom_deal"),
        new GameItemDefinition(
          GameItemId.HypeMan,
          "IT-03",
          "UI_ITEM_HYPE_MAN",
          "UI_ITEM_HYPE_MAN_DESC",
          "bar_shop.item.hype_man",
          2,
          GameItemTargetMode.None,
          GameItemEffectType.RevealAiCard,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.hype_man"),
        new GameItemDefinition(
          GameItemId.HealthRecovery,
          "IT-HP-01",
          "UI_ITEM_HEALTH_RECOVERY",
          "UI_ITEM_HEALTH_RECOVERY_DESC",
          "bar_shop.item.health_recovery",
          1,
          GameItemTargetMode.None,
          GameItemEffectType.RecoverHealth,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.health_recovery",
          1),
        new GameItemDefinition(
          GameItemId.WildInk,
          "IT-04",
          "UI_ITEM_WILD_INK",
          "UI_ITEM_WILD_INK_DESC",
          "bar_shop.item.wild_ink",
          3,
          GameItemTargetMode.PlayerCardAndSuit,
          GameItemEffectType.OverrideSuit,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.wild_ink"),
        new GameItemDefinition(
          GameItemId.Barrel,
          "IT-05",
          "UI_ITEM_BARREL",
          "UI_ITEM_BARREL_DESC",
          "bar_shop.item.barrel",
          4,
          GameItemTargetMode.None,
          GameItemEffectType.PreventShowdownDamage,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.barrel"),
        new GameItemDefinition(
          GameItemId.PredictionInsurance,
          "IT-06",
          "UI_ITEM_PREDICTION_INSURANCE",
          "UI_ITEM_PREDICTION_INSURANCE_DESC",
          "bar_shop.item.prediction_insurance",
          3,
          GameItemTargetMode.None,
          GameItemEffectType.InsurePrediction,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.prediction_insurance",
          2),
        new GameItemDefinition(
          GameItemId.Mercenary,
          "IT-07",
          "UI_ITEM_MERCENARY",
          "UI_ITEM_MERCENARY_DESC",
          "bar_shop.item.mercenary",
          4,
          GameItemTargetMode.PlayerAndAiCardPair,
          GameItemEffectType.ExchangeBothSides,
          GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
          "item.mercenary")
      });

    public static IReadOnlyList<GameItemDefinition> All => RegisteredItems;

    public static bool TryGet(GameItemId id, out GameItemDefinition? definition)
    {
      for (var index = 0; index < RegisteredItems.Count; index++)
      {
        if (RegisteredItems[index].Id == id)
        {
          definition = RegisteredItems[index];
          return true;
        }
      }

      definition = null;
      return false;
    }
  }
}
