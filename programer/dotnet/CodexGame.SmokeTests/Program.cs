using CodexGame.SmokeTests;
using CodexGame.SmokeTests.Cards;
using CodexGame.SmokeTests.Distribution;
using CodexGame.SmokeTests.Halli;
using CodexGame.SmokeTests.Playable;
using CodexGame.SmokeTests.Poker;
using CodexGame.SmokeTests.Battle;
using CodexGame.SmokeTests.Localization;
using CodexGame.SmokeTests.Presentation;

var tests = new TestHarness();

CardDeckTests.Run(tests);
CardStorageTests.Run(tests);
BellTimingTests.Run(tests);
HalliRuleTests.Run(tests);
HalliStageTests.Run(tests);
PrototypeHalliSessionTests.Run(tests);
PrivateCardDistributionTests.Run(tests);
PokerEvaluatorTests.Run(tests);
PokerRoundSessionTests.Run(tests);
BattleRuleTests.Run(tests);
LocalizationCatalogTests.Run(tests);
GuideModalStateTests.Run(tests);
StageFlowPlanTests.Run(tests);

return tests.Complete();
