using System;

namespace CodexGame.Application.Development
{
  public sealed class ItemQaPresetResult
  {
    public ItemQaPresetResult(
      ItemQaPreset preset,
      int stageNumber,
      string state,
      long seed,
      string playerHand,
      string aiHand,
      string publicCards,
      string items,
      string expected,
      string actual,
      bool passed)
    {
      Preset = preset;
      StageNumber = stageNumber;
      State = state ?? throw new ArgumentNullException(nameof(state));
      Seed = seed;
      PlayerHand = playerHand ?? throw new ArgumentNullException(nameof(playerHand));
      AiHand = aiHand ?? throw new ArgumentNullException(nameof(aiHand));
      PublicCards = publicCards ?? throw new ArgumentNullException(nameof(publicCards));
      Items = items ?? throw new ArgumentNullException(nameof(items));
      Expected = expected ?? throw new ArgumentNullException(nameof(expected));
      Actual = actual ?? throw new ArgumentNullException(nameof(actual));
      Passed = passed;
    }

    public ItemQaPreset Preset { get; }
    public int StageNumber { get; }
    public string State { get; }
    public long Seed { get; }
    public string PlayerHand { get; }
    public string AiHand { get; }
    public string PublicCards { get; }
    public string Items { get; }
    public string Expected { get; }
    public string Actual { get; }
    public bool Passed { get; }

    public string Summary =>
      $"{(Passed ? "PASS" : "FAIL")} stage={StageNumber} state={State} seed={Seed} "
      + $"hand={PlayerHand} public={PublicCards} items={Items} "
      + $"expected={Expected} actual={Actual}";
  }
}
