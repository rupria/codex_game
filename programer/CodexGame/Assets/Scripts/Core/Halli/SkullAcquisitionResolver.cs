using CodexGame.Core.Cards;

namespace CodexGame.Core.Halli
{
  public static class SkullAcquisitionResolver
  {
    public static AcquisitionKind Resolve(Card? leftCard, Card? rightCard)
    {
      if (!leftCard.HasValue && !rightCard.HasValue)
      {
        return AcquisitionKind.None;
      }

      if (!leftCard.HasValue)
      {
        return rightCard.HasValue && rightCard.Value.SkullCount == 3
          ? AcquisitionKind.RightOnly
          : AcquisitionKind.None;
      }

      if (!rightCard.HasValue)
      {
        return leftCard.HasValue && leftCard.Value.SkullCount == 3
          ? AcquisitionKind.LeftOnly
          : AcquisitionKind.None;
      }

      var left = leftCard.Value;
      var right = rightCard.Value;

      if (left.SkullCount == 3 && right.SkullCount == 3)
      {
        return AcquisitionKind.Unspecified;
      }

      if (left.SkullCount == 3)
      {
        return AcquisitionKind.LeftOnly;
      }

      if (right.SkullCount == 3)
      {
        return AcquisitionKind.RightOnly;
      }

      if (left.SkullCount + right.SkullCount == 3 && left.Suit == right.Suit)
      {
        return AcquisitionKind.Both;
      }

      return AcquisitionKind.None;
    }
  }
}
