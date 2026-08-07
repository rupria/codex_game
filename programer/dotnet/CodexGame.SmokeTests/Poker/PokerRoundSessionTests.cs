using System;
using CodexGame.Application.Poker;
using CodexGame.Core.Battle;
using CodexGame.Core.Cards;
using CodexGame.Core.Distribution;
using CodexGame.Core.Halli;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;

namespace CodexGame.SmokeTests.Poker
{
  internal static class PokerRoundSessionTests
  {
    public static void Run(TestHarness tests)
    {
      var distribution = new PrivateCardDistributionResult(
        HalliStageWinner.Player,
        1,
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Ace),
          C(CardSuit.Diamonds, CardRank.Ace),
          C(CardSuit.Clubs, CardRank.King)
        }),
        Array.AsReadOnly(new[]
        {
          C(CardSuit.Spades, CardRank.Queen),
          C(CardSuit.Diamonds, CardRank.Jack),
          C(CardSuit.Clubs, CardRank.Ten)
        }),
        C(CardSuit.Hearts, CardRank.Seven),
        Array.AsReadOnly(Array.Empty<Card>()));
      var session = new PokerRoundSession();
      session.Begin(
        C(CardSuit.Clubs, CardRank.Two),
        distribution,
        BattleHealth.Initial,
        PokerRuleSet.Development);

      var concealed = session.GetSnapshot();
      tests.Check(concealed.VisibleAiPrivateCards.Count == 0, "AI private cards must stay concealed before prediction.");
      tests.Check(concealed.PlayerPrivateCards.Count == 3 && concealed.PublicCards.Count == 2,
        "Poker snapshot should expose the player's three private and two public cards.");

      var result = session.Resolve(PredictionChoice.PlayerWins);
      var revealed = session.GetSnapshot();
      tests.Check(result.Comparison.Winner == PokerWinner.Player, "The pair of aces should win this fixture.");
      tests.Check(result.Prediction.IsCorrect, "Prediction correctness should be recorded independently.");
      tests.Check(result.Damage.After.Player == 3 && result.Damage.After.Ai == 2,
        "Only the poker loser should take one HP damage.");
      tests.Check(revealed.VisibleAiPrivateCards.Count == 3, "AI private cards should reveal after resolution.");
      tests.CheckThrows<InvalidOperationException>(
        () => session.Resolve(PredictionChoice.PlayerLoses),
        "A poker prediction must resolve only once.");
    }

    private static Card C(CardSuit suit, CardRank rank)
    {
      return new Card(suit, rank, 1);
    }
  }
}
