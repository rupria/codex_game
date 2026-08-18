#nullable enable
using System;
using CodexGame.Core.Shared;

namespace CodexGame.Application.Playable
{
  public sealed class NextStageTransitionGate
  {
    private bool _pending;
    private bool _consumed;
    private bool _loadComplete;
    private long _seed;
    private GameTimestamp _startedAt;
    private GameTimestamp _loadCompletedAt;

    public bool TryRequest(long nextStageSeed, GameTimestamp now)
    {
      if (_pending || _consumed) return false;
      _seed = nextStageSeed;
      _startedAt = now;
      _loadComplete = false;
      _pending = true;
      return true;
    }

    public bool MarkLoadComplete(GameTimestamp now)
    {
      if (!_pending || _consumed || _loadComplete) return false;
      _loadComplete = true;
      _loadCompletedAt = now;
      return true;
    }

    public bool IsComplete(GameTimestamp now)
    {
      return GetSnapshot(now).Step == NextStageTransitionStep.Complete;
    }

    public bool TryConsume(GameTimestamp now, out long nextStageSeed)
    {
      if (!_pending || _consumed || !IsComplete(now))
      {
        nextStageSeed = 0;
        return false;
      }

      nextStageSeed = _seed;
      _pending = false;
      _consumed = true;
      return true;
    }

    public NextStageTransitionSnapshot GetSnapshot(GameTimestamp now)
    {
      if (!_pending)
      {
        return new NextStageTransitionSnapshot(
          NextStageTransitionStep.Inactive,
          0f,
          0,
          false,
          false);
      }

      var elapsed = Math.Max(0, now.Microseconds - _startedAt.Microseconds);
      var cursor = 0L;

      var step = ResolveTimedStep(
        elapsed,
        ref cursor,
        GameRules.NextStageTransitionShopClearMicroseconds,
        NextStageTransitionStep.ShopUiClear);
      if (step != null) return step;

      step = ResolveTimedStep(
        elapsed,
        ref cursor,
        GameRules.NextStageTransitionCameraTurnMicroseconds,
        NextStageTransitionStep.CameraTurnToExit);
      if (step != null) return step;

      step = ResolveTimedStep(
        elapsed,
        ref cursor,
        GameRules.NextStageTransitionWalkMicroseconds,
        NextStageTransitionStep.WalkToDoor);
      if (step != null) return step;

      step = ResolveTimedStep(
        elapsed,
        ref cursor,
        GameRules.NextStageTransitionDoorOpenMicroseconds,
        NextStageTransitionStep.PushSwingDoors);
      if (step != null) return step;

      step = ResolveTimedStep(
        elapsed,
        ref cursor,
        GameRules.NextStageTransitionThresholdMicroseconds,
        NextStageTransitionStep.CrossThreshold);
      if (step != null) return step;

      step = ResolveTimedStep(
        elapsed,
        ref cursor,
        GameRules.NextStageTransitionFadeOutMicroseconds,
        NextStageTransitionStep.FadeOutAndBeginLoad);
      if (step != null) return step;

      if (!_loadComplete)
      {
        var blackHoldElapsed = elapsed - GameRules.NextStageTransitionFixedPreloadMicroseconds;
        return new NextStageTransitionSnapshot(
          NextStageTransitionStep.LoadingLoop,
          0f,
          elapsed,
          false,
          blackHoldElapsed >= GameRules.NextStageTransitionMinimumBlackHoldMicroseconds);
      }

      var preloadFinishedAt = _startedAt.Microseconds
        + GameRules.NextStageTransitionFixedPreloadMicroseconds;
      var fadeStartedAt = Math.Max(preloadFinishedAt, _loadCompletedAt.Microseconds);
      var fadeElapsed = Math.Max(0, now.Microseconds - fadeStartedAt);
      if (fadeElapsed < GameRules.NextStageTransitionFadeInMicroseconds)
      {
        return new NextStageTransitionSnapshot(
          NextStageTransitionStep.NextStageFadeIn,
          Progress(fadeElapsed, GameRules.NextStageTransitionFadeInMicroseconds),
          elapsed,
          true,
          false);
      }

      return new NextStageTransitionSnapshot(
        NextStageTransitionStep.Complete,
        1f,
        elapsed,
        true,
        false);
    }

    public void Reset()
    {
      _pending = false;
      _consumed = false;
      _loadComplete = false;
      _seed = 0;
      _startedAt = default;
      _loadCompletedAt = default;
    }

    private NextStageTransitionSnapshot? ResolveTimedStep(
      long elapsed,
      ref long cursor,
      long duration,
      NextStageTransitionStep step)
    {
      var stepStart = cursor;
      cursor += duration;
      if (elapsed >= cursor) return null;
      return new NextStageTransitionSnapshot(
        step,
        Progress(elapsed - stepStart, duration),
        elapsed,
        _loadComplete,
        false);
    }

    private static float Progress(long elapsed, long duration)
    {
      if (duration <= 0) return 1f;
      return Math.Min(1f, Math.Max(0f, (float)elapsed / duration));
    }
  }
}
