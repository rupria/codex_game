using CodexGame.Core.Shared;

namespace CodexGame.Core.Halli
{
  public readonly struct HalliRevealStep
  {
    public HalliRevealStep(
      int number,
      HalliActor actor,
      HalliRelativeSide relativeSide,
      PileSide physicalPile)
    {
      Number = number;
      Actor = actor;
      RelativeSide = relativeSide;
      PhysicalPile = physicalPile;
    }

    public int Number { get; }
    public HalliActor Actor { get; }
    public HalliRelativeSide RelativeSide { get; }
    public PileSide PhysicalPile { get; }
  }
}
