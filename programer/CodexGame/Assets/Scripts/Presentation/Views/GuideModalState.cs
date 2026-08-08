namespace CodexGame.Presentation.Views
{
  internal sealed class GuideModalState
  {
    public const int PageCount = 4;

    public bool IsOpen { get; private set; }
    public int PageIndex { get; private set; }
    public bool CanMovePrevious => IsOpen && PageIndex > 0;
    public bool CanMoveNext => IsOpen && PageIndex < PageCount - 1;

    public void Open()
    {
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
