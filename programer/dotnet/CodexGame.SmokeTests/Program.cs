using CodexGame.SmokeTests;
using CodexGame.SmokeTests.Cards;
using CodexGame.SmokeTests.Halli;

var tests = new TestHarness();

CardDeckTests.Run(tests);
CardStorageTests.Run(tests);
BellTimingTests.Run(tests);
HalliRuleTests.Run(tests);

return tests.Complete();
