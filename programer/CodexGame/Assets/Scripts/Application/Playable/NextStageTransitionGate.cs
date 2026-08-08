namespace CodexGame.Application.Playable
{
  public sealed class NextStageTransitionGate
  {
    private bool _pending;
    private bool _consumed;
    private long _seed;

    public bool TryRequest(long nextStageSeed)
    {
      if (_pending || _consumed) return false;
      _seed = nextStageSeed;
      _pending = true;
      return true;
    }

    public bool TryConsume(out long nextStageSeed)
    {
      if (!_pending || _consumed)
      {
        nextStageSeed = 0;
        return false;
      }

      nextStageSeed = _seed;
      _pending = false;
      _consumed = true;
      return true;
    }

    public void Reset()
    {
      _pending = false;
      _consumed = false;
      _seed = 0;
    }
  }
}
