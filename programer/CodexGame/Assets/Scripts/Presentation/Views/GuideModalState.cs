namespace CodexGame.Presentation.Views
{
  internal enum GuideModalMode
  {
    MainGuide = 0,
    FirstStartTutorial = 1
  }

  internal sealed class GuideModalState
  {
    public const int PageCount = 4;

    public bool IsOpen { get; private set; }
    public int PageIndex { get; private set; }
    public GuideModalMode Mode { get; private set; }
    public bool IsFirstStartTutorial => IsOpen && Mode == GuideModalMode.FirstStartTutorial;
    public bool CanMovePrevious => IsOpen && PageIndex > 0;
    public bool CanMoveNext => IsOpen && PageIndex < PageCount - 1;

    public void Open()
    {
      OpenMainGuide();
    }

    public void OpenMainGuide()
    {
      Mode = GuideModalMode.MainGuide;
      PageIndex = 0;
      IsOpen = true;
    }

    public void OpenFirstStartTutorial()
    {
      Mode = GuideModalMode.FirstStartTutorial;
      PageIndex = 0;
      IsOpen = true;
    }

    public void Close()
    {
      IsOpen = false;
    }

    public void MovePrevious()
    {
      if (CanMovePrevious) PageIndex--;
    }

    public void MoveNext()
    {
      if (CanMoveNext) PageIndex++;
    }
  }
}
