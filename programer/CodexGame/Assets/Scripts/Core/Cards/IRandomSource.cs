namespace CodexGame.Core.Cards
{
  public interface IRandomSource
  {
    int NextInt(int exclusiveMax);
  }
}
