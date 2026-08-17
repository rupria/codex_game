using CodexGame.Core.Items;

namespace CodexGame.Application.Items
{
  public sealed class ItemUsePresentationSnapshot
  {
    public static readonly ItemUsePresentationSnapshot Inactive =
      new ItemUsePresentationSnapshot(false, null, 1f, 0);

    public ItemUsePresentationSnapshot(
      bool isActive,
      GameItemId? itemId,
      float progress,
      long remainingMicroseconds)
    {
      IsActive = isActive;
      ItemId = itemId;
      Progress = progress;
      RemainingMicroseconds = remainingMicroseconds;
    }

    public bool IsActive { get; }
    public GameItemId? ItemId { get; }
    public float Progress { get; }
    public long RemainingMicroseconds { get; }
  }
}
