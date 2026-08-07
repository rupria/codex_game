using CodexGame.Core.Cards;
using CodexGame.Core.Halli;

var failures = new List<string>();

Check(
  ReactionResolver.Resolve(0.5, 0.5) == ReactionWinner.Player,
  "Simultaneous bell input must favor the player.");
Check(
  ReactionResolver.Resolve(0.8, 0.4) == ReactionWinner.Ai,
  "The earlier AI input must win.");

var spades1 = new HalliCard(CardSuit.Spades, 1);
var spades2 = new HalliCard(CardSuit.Spades, 2);
var hearts1 = new HalliCard(CardSuit.Hearts, 1);
var hearts2 = new HalliCard(CardSuit.Hearts, 2);
var clubs1 = new HalliCard(CardSuit.Clubs, 1);
var clubs2 = new HalliCard(CardSuit.Clubs, 2);
var diamonds3 = new HalliCard(CardSuit.Diamonds, 3);
var clubs3 = new HalliCard(CardSuit.Clubs, 3);

Check(
  SkullAcquisitionResolver.Resolve(spades1, spades2) == AcquisitionKind.Both,
  "Same-suit skull 1 + 2 must acquire both cards.");
Check(
  SkullAcquisitionResolver.Resolve(hearts2, hearts1) == AcquisitionKind.Both,
  "Same-suit skull 2 + 1 must acquire both cards.");
Check(
  SkullAcquisitionResolver.Resolve(spades1, hearts2) == AcquisitionKind.None,
  "Different-suit skull 1 + 2 must not acquire cards.");
Check(
  SkullAcquisitionResolver.Resolve(clubs2, hearts1) == AcquisitionKind.None,
  "Different-suit skull 2 + 1 must not acquire cards.");
Check(
  SkullAcquisitionResolver.Resolve(spades1, hearts1) == AcquisitionKind.None,
  "Skull 1 + 1 must not acquire cards regardless of suit.");
Check(
  SkullAcquisitionResolver.Resolve(diamonds3, clubs1) == AcquisitionKind.LeftOnly,
  "Skull 3 + 1 must acquire only the skull-3 card regardless of suit.");
Check(
  SkullAcquisitionResolver.Resolve(clubs2, diamonds3) == AcquisitionKind.RightOnly,
  "Skull 2 + 3 must acquire only the skull-3 card regardless of suit.");
Check(
  SkullAcquisitionResolver.Resolve(null, diamonds3) == AcquisitionKind.RightOnly,
  "A lone skull-3 card must be acquired.");
Check(
  SkullAcquisitionResolver.Resolve(null, spades2) == AcquisitionKind.None,
  "A lone non-skull-3 card must not be acquired.");
Check(
  SkullAcquisitionResolver.Resolve(diamonds3, clubs3) == AcquisitionKind.Unspecified,
  "The unresolved skull 3 + 3 case must not be guessed.");

CheckThrows<ArgumentOutOfRangeException>(
  () => _ = new HalliCard(CardSuit.Spades, 0),
  "Skull count below 1 must be rejected.");
CheckThrows<ArgumentOutOfRangeException>(
  () => _ = new HalliCard(CardSuit.Spades, 4),
  "Skull count above 3 must be rejected.");

if (failures.Count == 0)
{
  Console.WriteLine("All CodexGame core smoke tests passed.");
  return 0;
}

foreach (var failure in failures)
{
  Console.Error.WriteLine($"FAIL: {failure}");
}

return 1;

void Check(bool condition, string message)
{
  if (!condition)
  {
    failures.Add(message);
  }
}

void CheckThrows<TException>(Action action, string message)
  where TException : Exception
{
  try
  {
    action();
    failures.Add(message);
  }
  catch (TException)
  {
  }
}
