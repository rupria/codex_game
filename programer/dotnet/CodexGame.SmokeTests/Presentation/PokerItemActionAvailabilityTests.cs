using System;
using CodexGame.Application.Items;
using CodexGame.Core.Cards;
using CodexGame.Core.Items;
using CodexGame.Presentation.Views;

namespace CodexGame.SmokeTests.Presentation
{
  internal static class PokerItemActionAvailabilityTests
  {
    public static void Run(TestHarness tests)
    {
      var itemLimitExhausted = new PokerItemSnapshot(
        PokerItemPhase.AwaitingActions,
        GameItemUseTiming.AfterPublicCardsAndPrivateSelectionBeforePrediction,
        Array.Empty<GameItemId>(),
        Array.Empty<Card>(),
        Array.Empty<Card>(),
        Array.Empty<Card>(),
        Array.Empty<Card>(),
        PokerItemFailure.StageUseLimitReached,
        new StageItemRestrictionSnapshot(
          stageNumber: 1,
          wasAssessed: true,
          isActive: true,
          useLimit: 1,
          usedCount: 1));

      tests.Check(
        itemLimitExhausted.StageRestriction?.IsExhausted == true
          && PokerItemActionAvailability.CanConfirmHand(itemLimitExhausted),
        "Hand confirmation must remain available when the stage item-use limit is exhausted.");
      tests.Check(
        !PokerItemActionAvailability.CanConfirmHand(
          PokerItemPhase.AwaitingActions,
          true),
        "Hand confirmation must stay locked during an active item-use presentation.");
      tests.Check(
        !PokerItemActionAvailability.CanConfirmHand(
          PokerItemPhase.AwaitingBottomDealChoice,
          false)
          && !PokerItemActionAvailability.CanConfirmHand(
            PokerItemPhase.Completed,
            false),
        "Hand confirmation must be limited to the awaiting-actions phase.");
    }
  }
}
