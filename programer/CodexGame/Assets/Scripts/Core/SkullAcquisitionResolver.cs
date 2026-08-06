using System;

namespace CodexGame.Core
{
  public enum AcquisitionKind
  {
    None,
    Both,
    LeftOnly,
    RightOnly,
    Unspecified
  }

  public static class SkullAcquisitionResolver
  {
    public static AcquisitionKind Resolve(int? leftSkulls, int? rightSkulls)
    {
      ValidateSkulls(leftSkulls, nameof(leftSkulls));
      ValidateSkulls(rightSkulls, nameof(rightSkulls));

      if (!leftSkulls.HasValue && !rightSkulls.HasValue)
      {
        return AcquisitionKind.None;
      }

      if (leftSkulls == 3 && !rightSkulls.HasValue)
      {
        return AcquisitionKind.LeftOnly;
      }

      if (rightSkulls == 3 && !leftSkulls.HasValue)
      {
        return AcquisitionKind.RightOnly;
      }

      if (!leftSkulls.HasValue || !rightSkulls.HasValue)
      {
        return AcquisitionKind.None;
      }

      if (leftSkulls == 3 && rightSkulls == 3)
      {
        return AcquisitionKind.Unspecified;
      }

      if (leftSkulls == 3)
      {
        return AcquisitionKind.LeftOnly;
      }

      if (rightSkulls == 3)
      {
        return AcquisitionKind.RightOnly;
      }

      if (leftSkulls + rightSkulls == 3)
      {
        return AcquisitionKind.Both;
      }

      return AcquisitionKind.None;
    }

    private static void ValidateSkulls(int? skulls, string parameterName)
    {
      if (skulls.HasValue && (skulls.Value < 1 || skulls.Value > 3))
      {
        throw new ArgumentOutOfRangeException(parameterName);
      }
    }
  }
}
