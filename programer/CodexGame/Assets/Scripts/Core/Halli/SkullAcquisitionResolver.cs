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

      if (right.SkullCount == 3)
      {
        // Pile order is oldest to newest. Skull-3 is active only while it is
        // the newest exposed card, including a 3 + 3 pair.
        return AcquisitionKind.RightOnly;
      }

      if (left.SkullCount == 3)
      {
        // Once another card is exposed, the previous skull-3 becomes inert.
        return AcquisitionKind.None;
      }

      if (left.SkullCount + right.SkullCount == 3 && left.Suit == right.Suit)
      {
        return AcquisitionKind.Both;
      }

      return AcquisitionKind.None;
    }
  }
}
