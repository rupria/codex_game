namespace CodexGame.Application.Shop
{
  public enum BarShopPurchaseFailure
  {
    None = 0,
    InvalidSlot = 1,
    UnknownItem = 2,
    InsufficientBullets = 3,
    DuplicateItem = 4,
    InventoryFull = 5,
    InputLocked = 6
  }
}
