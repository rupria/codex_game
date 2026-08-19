using CodexGame.Core.Shared;

namespace CodexGame.Application.Development
{
  public sealed class JokerAwardCheatState
  {
    public bool IsGuaranteed { get; private set; }

    public int EffectiveAwardPercent => IsGuaranteed
      ? 100
      : GameRules.JokerAwardPercent;

    public bool SetGuaranteed(bool enabled)
    {
      if (IsGuaranteed == enabled) return false;
      IsGuaranteed = enabled;
      return true;
    }
  }
}
