using System;
using CodexGame.Application.Playable;
using CodexGame.Core.Shared;
using CodexGame.Presentation.Views;
using UnityEngine;

namespace CodexGame.Bootstrap
{
  [DisallowMultipleComponent]
  public sealed class PlayableDevGameController : MonoBehaviour
  {
    private PrototypeHalliSession _session;
    private PlayableDevView _view;
    private long _seedSequence;

    private void Awake()
    {
      _session = new PrototypeHalliSession();
      _view = GetComponent<PlayableDevView>();

      if (_view == null)
      {
        _view = gameObject.AddComponent<PlayableDevView>();
      }

      _view.StartRequested += StartNew;
      _view.AdvanceRequested += Advance;
      _view.LeftBellRequested += RingLeft;
      _view.RightBellRequested += RingRight;
      _view.RestartRequested += StartNew;
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
      if (_view == null)
      {
        return;
      }

      _view.StartRequested -= StartNew;
      _view.AdvanceRequested -= Advance;
      _view.LeftBellRequested -= RingLeft;
      _view.RightBellRequested -= RingRight;
      _view.RestartRequested -= StartNew;
    }

    private void StartNew()
    {
      _seedSequence++;
      var seed = DateTime.UtcNow.Ticks ^ (_seedSequence << 20);
      _session.StartNew(Now(), seed);
      Present();
    }

    private void Advance()
    {
      _session.Advance(Now());
      Present();
    }

    private void Ring(PileSide side)
    {
      _session.Ring(side, Now());
      Present();
    }

    private void RingLeft()
    {
      Ring(PileSide.Left);
    }

    private void RingRight()
    {
      Ring(PileSide.Right);
    }

    private void Present()
    {
      var now = Now();
      _view.Present(_session.GetSnapshot(now));
    }

    private static GameTimestamp Now()
    {
      return new GameTimestamp((long)(Time.realtimeSinceStartupAsDouble * 1_000_000d));
    }
  }
}
