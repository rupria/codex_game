using System;
using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Rewards;
using CodexGame.Core.Shared;
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
      _view.AdvanceRequested += Advance;
      _view.LeftBellRequested += RingLeft;
      _view.RightBellRequested += RingRight;
      _view.PrivateCardToggleRequested += TogglePrivateCard;
      _view.PrivateCardsConfirmRequested += ConfirmPrivateCards;
      _view.PredictionRequested += Predict;
      _view.MainRequested += ReturnToMain;
    }

    private void Start()
    {
      Present();
    }

    private void Update()
    {
      _session.Tick(Now());
      Present();
    }

    private void OnDestroy()
    {
      if (_view == null) return;
      _view.StartRequested -= StartNew;
      _view.AdvanceRequested -= Advance;
      _view.LeftBellRequested -= RingLeft;
      _view.RightBellRequested -= RingRight;
      _view.PrivateCardToggleRequested -= TogglePrivateCard;
      _view.PrivateCardsConfirmRequested -= ConfirmPrivateCards;
      _view.PredictionRequested -= Predict;
      _view.MainRequested -= ReturnToMain;
    }

    private void StartNew()
    {
      _session.StartNewBattle(Now(), NextSeed());
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

    private void ReturnToMain()
    {
      _session.ReturnToMain();
      Present();
    }

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
