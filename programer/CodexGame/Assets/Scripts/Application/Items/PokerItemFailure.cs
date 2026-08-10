namespace CodexGame.Application.Items
{
  public enum PokerItemFailure
  {
    None = 0,
    WrongPhase = 1,
    ItemNotOwned = 2,
    InvalidTarget = 3,
    CandidatePoolExhausted = 4,
    DuplicateCardIdentity = 5,
    InvalidCandidate = 6,
    HealthAlreadyFull = 7
  }
}
