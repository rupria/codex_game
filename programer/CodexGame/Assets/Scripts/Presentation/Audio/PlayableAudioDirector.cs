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
    private const string BundledSampleRoot = "Audio/UnityOpenProject1/";
    private const int SfxVoiceCount = 4;

    private sealed class CueProfile
    {
      public readonly AudioClip[] Clips;
      public readonly float PitchMin;
      public readonly float PitchMax;
      public readonly float VolumeMin;
      public readonly float VolumeMax;
      public readonly float PanSpread;
      public readonly float Cooldown;

      public int LastIndex = -1;
      public float NextAllowedTime;

      public CueProfile(
        AudioClip[] clips,
        float pitchMin,
        float pitchMax,
        float volumeMin,
        float volumeMax,
        float panSpread,
        float cooldown)
      {
        Clips = clips;
        PitchMin = pitchMin;
        PitchMax = pitchMax;
        VolumeMin = volumeMin;
        VolumeMax = volumeMax;
        PanSpread = panSpread;
        Cooldown = cooldown;
      }
    }

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
    [SerializeField] private AudioClip _predictionLock;

    [Header("Music (optional)")]
    [SerializeField] private AudioClip _musicLoop;

    private PlayableDevView _view;
    private PlayableGameSnapshot _lastSnapshot;
    private AudioSource[] _sfxVoices;
    private AudioSource _musicSource;
    private int _nextSfxVoice;

    private CueProfile _uiSelectCue;
    private CueProfile _uiConfirmCue;
    private CueProfile _uiBackCue;
    private CueProfile _uiErrorCue;
    private CueProfile _cardFlipCue;
    private CueProfile _bellCue;
    private CueProfile _itemCue;
    private CueProfile _rerollCue;
    private CueProfile _purchaseCue;
    private CueProfile _damageCue;
    private CueProfile _winCue;
    private CueProfile _loseCue;
    private CueProfile _transitionCue;
    private CueProfile _predictionLockCue;

    private void Awake()
    {
      LoadBundledSamples();
      BuildCueProfiles();
      EnsureListener();
      EnsureSources();
    }

    private void Start()
    {
      BeginMusicIfAvailable();
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
      AudioClip musicLoop = null,
      AudioClip predictionLock = null)
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
      _predictionLock = predictionLock;
      BuildCueProfiles();
    }

    private void LoadBundledSamples()
    {
      _uiSelect = LoadIfMissing(_uiSelect, "Interface_08");
      _uiConfirm = LoadIfMissing(_uiConfirm, "Interface_06");
      _uiBack = LoadIfMissing(_uiBack, "Interface_07");
      _uiError = LoadIfMissing(_uiError, "AttackLanding_Rock");
      _cardFlip = LoadIfMissing(_cardFlip, "PhoenixChick_Flap");
      _bell = LoadIfMissing(_bell, "Env_Pots_09");
      _itemUse = LoadIfMissing(_itemUse, "Grabbing_01");
      _reroll = LoadIfMissing(_reroll, "Swing_Cane_01");
      _purchase = LoadIfMissing(_purchase, "Grabbing_01");
      _damage = LoadIfMissing(_damage, "AttackLanding_Rock");
      _win = LoadIfMissing(_win, "Interface_05");
      _lose = LoadIfMissing(_lose, "Interface_07");
      _transition = LoadIfMissing(_transition, "Interface_05");
    }

    private static AudioClip LoadIfMissing(AudioClip current, string resourceName)
    {
      return current != null
        ? current
        : Resources.Load<AudioClip>(BundledSampleRoot + resourceName);
    }

    private void BuildCueProfiles()
    {
      _uiSelectCue = CreateCue(0.98f, 1.05f, 0.68f, 0.88f, 0.04f, 0.035f,
        _uiSelect, _uiConfirm, _uiBack);
      _uiConfirmCue = CreateCue(0.98f, 1.04f, 0.78f, 0.96f, 0.025f, 0.06f,
        _uiConfirm, _uiSelect);
      _uiBackCue = CreateCue(0.95f, 1.01f, 0.72f, 0.9f, 0.025f, 0.06f,
        _uiBack, _uiSelect);
      _uiErrorCue = CreateCue(0.9f, 0.98f, 0.82f, 1f, 0.015f, 0.1f,
        _uiError);

      _cardFlipCue = CreateCue(0.91f, 1.09f, 0.68f, 0.94f, 0.1f, 0.055f,
        _cardFlip, _uiSelect, _uiConfirm);
      _bellCue = CreateCue(0.94f, 1.04f, 0.88f, 1f, 0.035f, 0.08f,
        _bell);
      _itemCue = CreateCue(0.93f, 1.07f, 0.72f, 0.98f, 0.08f, 0.07f,
        _itemUse, _reroll, _uiConfirm);
      _rerollCue = CreateCue(0.91f, 1.08f, 0.72f, 0.96f, 0.08f, 0.1f,
        _reroll, _itemUse);
      _purchaseCue = CreateCue(0.98f, 1.06f, 0.82f, 1f, 0.04f, 0.09f,
        _purchase, _uiConfirm);
      _damageCue = CreateCue(0.9f, 1.02f, 0.86f, 1f, 0.045f, 0.12f,
        _damage, _bell);
      _winCue = CreateCue(1f, 1f, 0.92f, 1f, 0f, 0.2f,
        _win);
      _loseCue = CreateCue(1f, 1f, 0.92f, 1f, 0f, 0.2f,
        _lose);
      _transitionCue = CreateCue(0.94f, 1.04f, 0.76f, 0.96f, 0.07f, 0.15f,
        _transition, _reroll);
      _predictionLockCue = CreateCue(1f, 1f, 0.92f, 1f, 0f, 0.12f,
        _predictionLock != null ? _predictionLock : _uiSelect);
    }

    private static CueProfile CreateCue(
      float pitchMin,
      float pitchMax,
      float volumeMin,
      float volumeMax,
      float panSpread,
      float cooldown,
      params AudioClip[] clips)
    {
      return new CueProfile(
        CompactDistinct(clips),
        pitchMin,
        pitchMax,
        volumeMin,
        volumeMax,
        panSpread,
        cooldown);
    }

    private static AudioClip[] CompactDistinct(AudioClip[] clips)
    {
      if (clips == null || clips.Length == 0) return new AudioClip[0];

      var count = 0;
      var compact = new AudioClip[clips.Length];
      foreach (var clip in clips)
      {
        if (clip == null || Contains(compact, count, clip)) continue;
        compact[count++] = clip;
      }

      if (count == compact.Length) return compact;
      var result = new AudioClip[count];
      for (var i = 0; i < count; i++) result[i] = compact[i];
      return result;
    }

    private static bool Contains(AudioClip[] clips, int count, AudioClip candidate)
    {
      for (var i = 0; i < count; i++)
      {
        if (clips[i] == candidate) return true;
      }
      return false;
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
        if (healthDropped && !IsTerminalPhase(snapshot.Phase)) Play(_damageCue);
      }

      _lastSnapshot = snapshot;
    }

    public void PlayError()
    {
      Play(_uiErrorCue);
    }

    private void HandleAdvance()
    {
      Play(_lastSnapshot != null && _lastSnapshot.Phase == PlayableGamePhase.Halli
        ? _cardFlipCue
        : _uiConfirmCue);
    }

    private void HandleConfirm()
    {
      Play(_uiConfirmCue);
    }

    private void HandleBack()
    {
      Play(_uiBackCue);
    }

    private void HandleBell()
    {
      Play(_bellCue);
    }

    private void HandleSelect(CardId _)
    {
      Play(_uiSelectCue);
    }

    private void HandlePrediction(PredictionChoice _)
    {
      Play(_predictionLockCue);
    }

    private void HandleJokerHand(PokerHandCategory _)
    {
      Play(_uiSelectCue);
    }

    private void HandleItemCard(CardId _)
    {
      Play(_itemCue);
    }

    private void HandleWildInk(CardId _, CardSuit __)
    {
      Play(_itemCue);
    }

    private void HandleItem()
    {
      Play(_itemCue);
    }

    private void HandleReroll()
    {
      Play(_rerollCue);
    }

    private void HandlePurchase(int _)
    {
      Play(_purchaseCue);
    }

    private void PlayPhaseCue(PlayableGamePhase phase)
    {
      switch (phase)
      {
        case PlayableGamePhase.StageWon:
        case PlayableGamePhase.RunWon:
          Play(_winCue);
          break;
        case PlayableGamePhase.BattleFinished:
          Play(_loseCue);
          break;
        case PlayableGamePhase.NextStageTransition:
          Play(_transitionCue);
          break;
      }
    }

    private static bool IsTerminalPhase(PlayableGamePhase phase)
    {
      return phase == PlayableGamePhase.StageWon
        || phase == PlayableGamePhase.BattleFinished
        || phase == PlayableGamePhase.RunWon;
    }

    private void Play(CueProfile cue)
    {
      if (cue == null || cue.Clips.Length == 0) return;

      var now = Time.unscaledTime;
      if (now < cue.NextAllowedTime) return;
      cue.NextAllowedTime = now + cue.Cooldown;

      var index = Random.Range(0, cue.Clips.Length);
      if (cue.Clips.Length > 1 && index == cue.LastIndex)
      {
        index = (index + Random.Range(1, cue.Clips.Length)) % cue.Clips.Length;
      }
      cue.LastIndex = index;

      EnsureSources();
      BeginMusicIfAvailable();

      var voice = _sfxVoices[_nextSfxVoice];
      _nextSfxVoice = (_nextSfxVoice + 1) % _sfxVoices.Length;
      voice.Stop();
      voice.pitch = Random.Range(cue.PitchMin, cue.PitchMax);
      voice.volume = _sfxVolume * Random.Range(cue.VolumeMin, cue.VolumeMax);
      voice.panStereo = Random.Range(-cue.PanSpread, cue.PanSpread);
      voice.PlayOneShot(cue.Clips[index]);
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
      if (_sfxVoices == null || _sfxVoices.Length != SfxVoiceCount)
      {
        _sfxVoices = new AudioSource[SfxVoiceCount];
        for (var i = 0; i < _sfxVoices.Length; i++)
        {
          var source = gameObject.AddComponent<AudioSource>();
          source.playOnAwake = false;
          source.loop = false;
          source.spatialBlend = 0f;
          _sfxVoices[i] = source;
        }
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
