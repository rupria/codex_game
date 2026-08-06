using CodexGame.Core;

var failures = new List<string>();

Check(
  ReactionResolver.Resolve(0.5, 0.5) == ReactionWinner.Player,
  "Simultaneous bell input must favor the player.");
Check(
  ReactionResolver.Resolve(0.8, 0.4) == ReactionWinner.Ai,
  "The earlier AI input must win.");
Check(
  SkullAcquisitionResolver.Resolve(1, 2) == AcquisitionKind.Both,
  "Skull 1 + 2 must acquire both cards.");
Check(
  SkullAcquisitionResolver.Resolve(3, 1) == AcquisitionKind.LeftOnly,
  "Skull 3 + 1 must acquire only the skull-3 card.");
Check(
  SkullAcquisitionResolver.Resolve(null, 3) == AcquisitionKind.RightOnly,
  "A lone skull-3 card must be acquired.");
Check(
  SkullAcquisitionResolver.Resolve(3, 3) == AcquisitionKind.Unspecified,
  "The unresolved skull 3 + 3 case must not be guessed.");

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
