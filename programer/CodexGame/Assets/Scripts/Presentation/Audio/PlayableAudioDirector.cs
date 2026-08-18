using CodexGame.Application.Playable;
using CodexGame.Core.Cards;
using CodexGame.Core.Poker;
using CodexGame.Core.Rewards;
using CodexGame.Presentation.Views;
using UnityEngine;

namespace CodexGame.Presentation.Audio
{
  [DisallowMultipleComponent]
  public sealed class PlayableAudioDirector : MonoBehaviour
  {
    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.35f;

    [Header("UI")]
    [SerializeField] private AudioClip _uiSelect;
    [SerializeField] private AudioClip _uiConfirm;
    [SerializeField] private AudioClip _uiBack;
    [SerializeField] private AudioClip _uiError;

    [Header("Gameplay")]
    [SerializeField] private AudioClip _cardFlip;
    [SerializeField] private AudioClip _bell;
    [SerializeField] private AudioClip _itemUse;
    [SerializeField] private AudioClip _reroll;
    [SerializeField] private AudioClip _purchase;
    [SerializeField] private AudioClip _damage;
    [SerializeField] private AudioClip _win;
    [SerializeField] private AudioClip _lose;
    [SerializeField] private AudioClip _transition;

    [Header("Music (optional)")]
    [SerializeField] private AudioClip _musicLoop;

    private PlayableDevView _view;
    private PlayableGameSnapshot _lastSnapshot;
    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    private void Awake()
    {
      EnsureListener();
      EnsureSources();
    }

    private void OnDestroy()
    {
      Unbind();
    }

    public void Configure(
      AudioClip uiSelect,
      AudioClip uiConfirm,
      AudioClip uiBack,
      AudioClip uiError,
      AudioClip cardFlip,
      AudioClip bell,
      AudioClip itemUse,
      AudioClip reroll,
      AudioClip purchase,
      AudioClip damage,
      AudioClip win,
      AudioClip lose,
      AudioClip transition,
      AudioClip musicLoop = null)
    {
      _uiSelect = uiSelect;
      _uiConfirm = uiConfirm;
      _uiBack = uiBack;
      _uiError = uiError;
      _cardFlip = cardFlip;
      _bell = bell;
      _itemUse = itemUse;
      _reroll = reroll;
      _purchase = purchase;
      _damage = damage;
      _win = win;
      _lose = lose;
      _transition = transition;
      _musicLoop = musicLoop;
    }

    public void Bind(PlayableDevView view)
    {
      if (_view == view) return;
      Unbind();
      _view = view;
      if (_view == null) return;

      _view.StartRequested += HandleConfirm;
      _view.StageEntrySkipRequested += HandleConfirm;
      _view.AdvanceRequested += HandleAdvance;
      _view.LeftBellRequested += HandleBell;
      _view.RightBellRequested += HandleBell;
      _view.PrivateCardToggleRequested += HandleSelect;
      _view.PrivateCardsConfirmRequested += HandleConfirm;
      _view.PredictionRequested += HandlePrediction;
      _view.JokerHandRequested += HandleJokerHand;
      _view.ReloadItemRequested += HandleItemCard;
      _view.BottomDealRequested += HandleItemCard;
      _view.BottomDealChoiceRequested += HandleItemCard;
      _view.HypeManItemRequested += HandleItem;
      _view.HealthRecoveryItemRequested += HandleItem;
      _view.WildInkItemRequested += HandleWildInk;
      _view.BarrelItemRequested += HandleItem;
      _view.PredictionInsuranceItemRequested += HandleItem;
      _view.MercenaryItemRequested += HandleItemCard;
      _view.ItemsConfirmRequested += HandleConfirm;
      _view.BarShopRerollRequested += HandleReroll;
      _view.BarShopPurchaseRequested += HandlePurchase;
      _view.MainRequested += HandleBack;
      _view.InactivityAcknowledgedRequested += HandleConfirm;
    }

    public void Unbind()
    {
      if (_view == null) return;

      _view.StartRequested -= HandleConfirm;
      _view.StageEntrySkipRequested -= HandleConfirm;
      _view.AdvanceRequested -= HandleAdvance;
      _view.LeftBellRequested -= HandleBell;
      _view.RightBellRequested -= HandleBell;
      _view.PrivateCardToggleRequested -= HandleSelect;
      _view.PrivateCardsConfirmRequested -= HandleConfirm;
      _view.PredictionRequested -= HandlePrediction;
      _view.JokerHandRequested -= HandleJokerHand;
      _view.ReloadItemRequested -= HandleItemCard;
      _view.BottomDealRequested -= HandleItemCard;
      _view.BottomDealChoiceRequested -= HandleItemCard;
      _view.HypeManItemRequested -= HandleItem;
      _view.HealthRecoveryItemRequested -= HandleItem;
      _view.WildInkItemRequested -= HandleWildInk;
      _view.BarrelItemRequested -= HandleItem;
      _view.PredictionInsuranceItemRequested -= HandleItem;
      _view.MercenaryItemRequested -= HandleItemCard;
      _view.ItemsConfirmRequested -= HandleConfirm;
      _view.BarShopRerollRequested -= HandleReroll;
      _view.BarShopPurchaseRequested -= HandlePurchase;
      _view.MainRequested -= HandleBack;
      _view.InactivityAcknowledgedRequested -= HandleConfirm;
      _view = null;
    }

    public void Present(PlayableGameSnapshot snapshot)
    {
      if (snapshot == null) return;

      if (_lastSnapshot != null)
      {
        var phaseChanged = snapshot.Phase != _lastSnapshot.Phase;
        if (phaseChanged)
        {
          PlayPhaseCue(snapshot.Phase);
        }

        var healthDropped = snapshot.Health.Player < _lastSnapshot.Health.Player
          || snapshot.Health.Ai < _lastSnapshot.Health.Ai;
        if (healthDropped && !IsTerminalPhase(snapshot.Phase)) Play(_damage);
      }

      _lastSnapshot = snapshot;
    }

    public void PlayError()
    {
      Play(_uiError);
    }

    private void HandleAdvance()
    {
      Play(_lastSnapshot != null && _lastSnapshot.Phase == PlayableGamePhase.Halli
        ? _cardFlip
        : _uiConfirm);
    }

    private void HandleConfirm()
    {
      Play(_uiConfirm);
    }

    private void HandleBack()
    {
      Play(_uiBack);
    }

    private void HandleBell()
    {
      Play(_bell);
    }

    private void HandleSelect(CardId _)
    {
      Play(_uiSelect);
    }

    private void HandlePrediction(PredictionChoice _)
    {
      Play(_uiSelect);
    }

    private void HandleJokerHand(PokerHandCategory _)
    {
      Play(_uiSelect);
    }

    private void HandleItemCard(CardId _)
    {
      Play(_itemUse);
    }

    private void HandleWildInk(CardId _, CardSuit __)
    {
      Play(_itemUse);
    }

    private void HandleItem()
    {
      Play(_itemUse);
    }

    private void HandleReroll()
    {
      Play(_reroll);
    }

    private void HandlePurchase(int _)
    {
      Play(_purchase);
    }

    private void PlayPhaseCue(PlayableGamePhase phase)
    {
      switch (phase)
      {
        case PlayableGamePhase.StageWon:
        case PlayableGamePhase.RunWon:
          Play(_win);
          break;
        case PlayableGamePhase.BattleFinished:
          Play(_lose);
          break;
        case PlayableGamePhase.NextStageTransition:
          Play(_transition);
          break;
      }
    }

    private static bool IsTerminalPhase(PlayableGamePhase phase)
    {
      return phase == PlayableGamePhase.StageWon
        || phase == PlayableGamePhase.BattleFinished
        || phase == PlayableGamePhase.RunWon;
    }

    private void Play(AudioClip clip)
    {
      if (clip == null) return;
      EnsureSources();
      BeginMusicIfAvailable();
      _sfxSource.pitch = Random.Range(0.97f, 1.03f);
      _sfxSource.PlayOneShot(clip, _sfxVolume);
    }

    private void BeginMusicIfAvailable()
    {
      if (_musicLoop == null || _musicSource.isPlaying) return;
      _musicSource.clip = _musicLoop;
      _musicSource.volume = _musicVolume;
      _musicSource.Play();
    }

    private void EnsureSources()
    {
      if (_sfxSource == null)
      {
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f;
      }

      if (_musicSource == null)
      {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;
      }
    }

    private static void EnsureListener()
    {
      if (FindAnyObjectByType<AudioListener>() != null) return;
      var mainCamera = Camera.main;
      if (mainCamera != null) mainCamera.gameObject.AddComponent<AudioListener>();
    }
  }
}
