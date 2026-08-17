namespace CodexGame.Application.Playable
{
  public enum FirstStartRequest
  {
    ShowTutorial = 0,
    StartBattle = 1
  }

  public sealed class FirstStartTutorialSession
  {
    public bool IsCompleted { get; private set; }

    public FirstStartRequest RequestStart()
    {
      return IsCompleted ? FirstStartRequest.StartBattle : FirstStartRequest.ShowTutorial;
    }

    public bool CompleteTutorial()
    {
      if (IsCompleted) return false;
      IsCompleted = true;
      return true;
    }
  }
}
