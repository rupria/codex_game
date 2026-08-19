using CodexGame.Application.Items;

namespace CodexGame.Presentation.Views
{
  internal static class PokerItemActionAvailability
  {
    public static bool CanConfirmHand(PokerItemSnapshot snapshot)
    {
      if (snapshot == null) throw new System.ArgumentNullException(nameof(snapshot));
      return CanConfirmHand(snapshot.Phase, snapshot.UsePresentation.IsActive);
    }

    public static bool CanConfirmHand(PokerItemPhase phase, bool usePresentationActive)
    {
      return phase == PokerItemPhase.AwaitingActions && !usePresentationActive;
    }
  }
}
