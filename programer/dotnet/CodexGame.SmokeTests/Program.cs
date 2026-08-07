using CodexGame.SmokeTests;
using CodexGame.SmokeTests.Cards;
using CodexGame.SmokeTests.Halli;
using CodexGame.SmokeTests.Playable;

var tests = new TestHarness();

CardDeckTests.Run(tests);
CardStorageTests.Run(tests);
BellTimingTests.Run(tests);
HalliRuleTests.Run(tests);
HalliStageTests.Run(tests);
PrototypeHalliSessionTests.Run(tests);

return tests.Complete();
