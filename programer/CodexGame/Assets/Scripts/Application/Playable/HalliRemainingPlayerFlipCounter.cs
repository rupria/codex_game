using System;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public static class HalliRemainingPlayerFlipCounter
  {
    public static int Calculate(
      int startedDistributionCount,
      bool currentDistributionHasSecondPlayerInput,
      int remainingDeckCards,
      bool currentPlayerInputAwaitsAiResponse)
    {
      if (startedDistributionCount < 0
        || startedDistributionCount > GameRules.HalliDistributionLimit)
      {
        throw new ArgumentOutOfRangeException(nameof(startedDistributionCount));
      }
      if (remainingDeckCards < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(remainingDeckCards));
      }

      var futureDistributionCount = GameRules.HalliDistributionLimit
        - startedDistributionCount;
      var remainingByFlow = futureDistributionCount * 2
        + (currentDistributionHasSecondPlayerInput ? 1 : 0);

      // A future player input is valid only when both the player card and its
      // AI response can be revealed. Reserve the response for a player card
      // that is already moving or face-up before applying the deck bound.
      var responseDebt = currentPlayerInputAwaitsAiResponse ? 1 : 0;
      var cardsAvailableForFutureInputs = Math.Max(0, remainingDeckCards - responseDebt);
      var remainingByDeck = cardsAvailableForFutureInputs / 2;
      return Math.Min(remainingByFlow, remainingByDeck);
    }
  }
}
