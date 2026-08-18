using System;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
using CodexGame.Core.Poker;
#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
using CodexGame.Application.Development;
using CodexGame.Core.Items;
#endif
using CodexGame.Presentation.Views;
using UnityEngine;

namespace CodexGame.Bootstrap
{
  [DisallowMultipleComponent]
  public sealed class PlayableDevGameController : MonoBehaviour
  {
    private PlayableGameSession _session;
    private PlayableDevView _view;
    private long _seedSequence;

    private void Awake()
    {
      _session = new PlayableGameSession();
      _view = GetComponent<PlayableDevView>();
      if (_view == null) _view = gameObject.AddComponent<PlayableDevView>();

      _view.StartRequested += StartNew;
      _view.StageEntrySkipRequested += SkipStageEntry;
      _view.AdvanceRequested += Advance;
      _view.LeftBellRequested += RingLeft;
      _view.RightBellRequested += RingRight;
      _view.PrivateCardToggleRequested += TogglePrivateCard;
      _view.PrivateCardsConfirmRequested += ConfirmPrivateCards;
      _view.PredictionRequested += Predict;
      _view.JokerHandRequested += ChooseJokerHand;
      _view.ReloadItemRequested += UseReload;
      _view.BottomDealRequested += BeginBottomDeal;
      _view.BottomDealChoiceRequested += ChooseBottomDeal;
      _view.BottomDealCancelRequested += CancelBottomDeal;
      _view.HypeManItemRequested += UseHypeMan;
      _view.HealthRecoveryItemRequested += UseHealthRecovery;
      _view.WildInkItemRequested += UseWildInk;
      _view.BarrelItemRequested += UseBarrel;
      _view.PredictionInsuranceItemRequested += UsePredictionInsurance;
      _view.MercenaryItemRequested += UseMercenary;
      _view.ItemsConfirmRequested += ConfirmItems;
      _view.BarShopRerollRequested += RerollBarShop;
      _view.BarShopPurchaseRequested += PurchaseBarShop;
      _view.MainRequested += ReturnToMain;
      _view.InactivityAcknowledgedRequested += AcknowledgeInactivityReturn;
#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
      _view.CheatStagePassRequested += CheatStagePass;
      _view.CheatGrantItemRequested += CheatGrantItem;
      _view.CheatPokerPresetRequested += CheatPokerPreset;
      _view.CheatItemQaPresetRequested += CheatItemQaPreset;
#endif
    }

    private void Start()
    {
      Present();
    }

    private void Update()
    {
      var now = Now();
      if (_session.Phase == PlayableGamePhase.NextStageTransition
        && _view.IsNextStagePresentationReady)
      {
        _session.MarkNextStageLoadComplete(now);
      }
      _session.Tick(now);
      Present();
    }

    private void OnDestroy()
    {
      if (_view == null) return;
      _view.StartRequested -= StartNew;
      _view.StageEntrySkipRequested -= SkipStageEntry;
      _view.AdvanceRequested -= Advance;
      _view.LeftBellRequested -= RingLeft;
      _view.RightBellRequested -= RingRight;
      _view.PrivateCardToggleRequested -= TogglePrivateCard;
      _view.PrivateCardsConfirmRequested -= ConfirmPrivateCards;
      _view.PredictionRequested -= Predict;
      _view.JokerHandRequested -= ChooseJokerHand;
      _view.ReloadItemRequested -= UseReload;
      _view.BottomDealRequested -= BeginBottomDeal;
      _view.BottomDealChoiceRequested -= ChooseBottomDeal;
      _view.BottomDealCancelRequested -= CancelBottomDeal;
      _view.HypeManItemRequested -= UseHypeMan;
      _view.HealthRecoveryItemRequested -= UseHealthRecovery;
      _view.WildInkItemRequested -= UseWildInk;
      _view.BarrelItemRequested -= UseBarrel;
      _view.PredictionInsuranceItemRequested -= UsePredictionInsurance;
      _view.MercenaryItemRequested -= UseMercenary;
      _view.ItemsConfirmRequested -= ConfirmItems;
      _view.BarShopRerollRequested -= RerollBarShop;
      _view.BarShopPurchaseRequested -= PurchaseBarShop;
      _view.MainRequested -= ReturnToMain;
      _view.InactivityAcknowledgedRequested -= AcknowledgeInactivityReturn;
#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
      _view.CheatStagePassRequested -= CheatStagePass;
      _view.CheatGrantItemRequested -= CheatGrantItem;
      _view.CheatPokerPresetRequested -= CheatPokerPreset;
      _view.CheatItemQaPresetRequested -= CheatItemQaPreset;
#endif
    }

    private void StartNew()
    {
      _session.StartNewBattle(Now(), NextSeed());
      Present();
    }

    private void SkipStageEntry()
    {
      _session.SkipStageEntry(Now());
      Present();
    }

    private void Advance()
    {
      _session.Advance(Now(), NextSeed());
      Present();
    }

    private void RingLeft()
    {
      _session.Ring(PileSide.Left, Now());
      Present();
    }

    private void RingRight()
    {
      _session.Ring(PileSide.Right, Now());
      Present();
    }

    private void TogglePrivateCard(CardId cardId)
    {
      _session.TogglePrivateCard(cardId, Now());
      Present();
    }

    private void ConfirmPrivateCards()
    {
      _session.ConfirmPrivateCards(Now());
      Present();
    }

    private void Predict(PredictionChoice choice)
    {
      _session.Predict(choice, Now());
      Present();
    }

    private void ChooseJokerHand(PokerHandCategory category)
    {
      _session.ChooseJokerHand(category, Now());
      Present();
    }

    private void RerollBarShop()
    {
      _session.RerollBarShop(Now());
      Present();
    }

    private void PurchaseBarShop(int slotIndex)
    {
      _session.PurchaseBarShopSlot(slotIndex, Now());
      Present();
    }

    private void UseReload(CardId target)
    {
      _session.UseReload(target, Now());
      Present();
    }

    private void BeginBottomDeal(CardId target)
    {
      _session.BeginBottomDeal(target, Now());
      Present();
    }

    private void ChooseBottomDeal(CardId candidate)
    {
      _session.ChooseBottomDeal(candidate, Now());
      Present();
    }

    private void CancelBottomDeal()
    {
      _session.CancelBottomDeal(Now());
      Present();
    }

    private void UseHypeMan()
    {
      _session.UseHypeMan(Now());
      Present();
    }

    private void UseHealthRecovery()
    {
      _session.UseHealthRecovery(Now());
      Present();
    }

    private void UseWildInk(CardId target, CardSuit effectiveSuit)
    {
      _session.UseWildInk(target, effectiveSuit, Now());
      Present();
    }

    private void UseBarrel()
    {
      _session.UseBarrel(Now());
      Present();
    }

    private void UsePredictionInsurance()
    {
      _session.UsePredictionInsurance(Now());
      Present();
    }

    private void UseMercenary(CardId target)
    {
      _session.UseMercenary(target, Now());
      Present();
    }

    private void ConfirmItems()
    {
      _session.ConfirmItems(Now());
      Present();
    }

    private void ReturnToMain()
    {
      _session.ReturnToMain();
      Present();
    }

    private void AcknowledgeInactivityReturn()
    {
      _session.AcknowledgeInactivityReturn(Now());
      Present();
    }

#if UNITY_EDITOR || ENABLE_GAMEPLAY_CHEATS
    private void CheatStagePass()
    {
      _session.CheatCompleteStage(Now());
      Present();
    }

    private void CheatGrantItem(GameItemId itemId)
    {
      _session.CheatGrantItem(itemId, Now());
      Present();
    }

    private void CheatPokerPreset(PokerCheatPreset preset)
    {
      var setup = PokerCheatPresetCatalog.Create(preset);
      _session.CheatSetPokerCards(
        setup.PlayerCards,
        setup.AiCards,
        setup.PublicCards,
        Now());
      Present();
    }

    private void CheatItemQaPreset(ItemQaPreset preset)
    {
      _session.CheatRunItemQaPreset(preset, Now());
      Present();
    }
#endif

    private void Present()
    {
      var now = Now();
      _view.Present(_session.GetSnapshot(now));
    }

    private long NextSeed()
    {
      _seedSequence++;
      return DateTime.UtcNow.Ticks ^ (_seedSequence << 20);
    }

    private static GameTimestamp Now()
    {
      return new GameTimestamp((long)(Time.realtimeSinceStartupAsDouble * 1_000_000d));
    }
  }
}
