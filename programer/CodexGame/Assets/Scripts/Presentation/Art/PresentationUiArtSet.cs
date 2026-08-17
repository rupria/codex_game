using System;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
  [Serializable]
  public sealed class PresentationUiArtSet
  {
    [SerializeField] private Texture2D _threeCallIcon;
    [SerializeField] private Texture2D _showdownIcon;
    [SerializeField] private Texture2D _entryLabelFrame;
    [SerializeField] private Texture2D _skipIdle;
    [SerializeField] private Texture2D _skipHover;
    [SerializeField] private Texture2D _skipPressed;
    [SerializeField] private Texture2D _opponentIntroFrame;
    [SerializeField] private Texture2D _tableFocusVignette;
    [SerializeField] private Texture2D _limitOne;
    [SerializeField] private Texture2D _limitTwo;
    [SerializeField] private Texture2D _usedOne;
    [SerializeField] private Texture2D _limitExhausted;
    [SerializeField] private Texture2D _inventoryRestricted;
    [SerializeField] private Texture2D _cardRestricted;
    [SerializeField] private Texture2D _penaltyLabelFrame;
    [SerializeField] private Texture2D _desaturateOverlay;
    [SerializeField] private Texture2D _focusMask;
    [SerializeField] private Texture2D _playerAcquireTrail;
    [SerializeField] private Texture2D _aiAcquireTrail;
    [SerializeField] private Texture2D _candidateIdle;
    [SerializeField] private Texture2D _candidateHover;
    [SerializeField] private Texture2D _candidateSelected;
    [SerializeField] private Texture2D _candidateConfirmed;
    [SerializeField] private Texture2D _candidateDisabled;
    [SerializeField] private Texture2D _correctBellSheet;
    [SerializeField] private Texture2D _wrongBellSheet;
    [SerializeField] private Texture2D _showdownWideFrame;
    [SerializeField] private Texture2D _showdownHandLockedFrame;
    [SerializeField] private Texture2D _bestHandHighlight;
    [SerializeField] private Texture2D _resultSummaryFrame;
    [SerializeField] private Texture2D _stageClearFrame;
    [SerializeField] private Texture2D _playerDamageFlash;
    [SerializeField] private Texture2D _aiDamageFlash;
    [SerializeField] private Texture2D[] _opponentCutouts;
    [SerializeField] private Texture2D[] _opponentPortraits;

    public PresentationUiArtSet(
      Func<string, Texture2D> load,
      Texture2D[] opponentCutouts = null,
      Texture2D[] opponentPortraits = null,
      Texture2D threeCallIcon = null,
      Texture2D showdownIcon = null,
      Texture2D limitOne = null,
      Texture2D limitTwo = null,
      Texture2D usedOne = null,
      Texture2D limitExhausted = null,
      Texture2D inventoryRestricted = null,
      Texture2D cardRestricted = null)
    {
      if (load == null) throw new ArgumentNullException(nameof(load));
      _threeCallIcon = Require(load, "phase_three_call_icon_64_0_1_2_4.png");
      _showdownIcon = Require(load, "phase_showdown_icon_64_0_1_2_4.png");
      _entryLabelFrame = Require(load, "phase_entry_label_frame_288x80_0_1_2_4.png");
      _skipIdle = Require(load, "stage_entry_skip_button_idle_120x44_0_1_2_4.png");
      _skipHover = Require(load, "stage_entry_skip_button_hover_120x44_0_1_2_4.png");
      _skipPressed = Require(load, "stage_entry_skip_button_pressed_120x44_0_1_2_4.png");
      _opponentIntroFrame = Require(load, "stage_entry_opponent_intro_frame_360x152_0_1_2_4.png");
      _tableFocusVignette = Require(load, "stage_entry_table_focus_vignette_960x540_0_1_2_4.png");
      _limitOne = Require(load, "stage_item_limit_one_64_0_1_2_4.png");
      _limitTwo = Require(load, "stage_item_limit_two_64_0_1_2_4.png");
      _usedOne = Require(load, "stage_item_limit_used_one_64_0_1_2_4.png");
      _limitExhausted = Require(load, "stage_item_limit_exhausted_64_0_1_2_4.png");
      _inventoryRestricted = Require(load, "stage_item_inventory_restricted_64_0_1_2_4.png");
      _cardRestricted = Require(load, "stage_item_card_restricted_64_0_1_2_4.png");
      _penaltyLabelFrame = Require(load, "stage_item_penalty_label_frame_320x84_0_1_2_4.png");
      _desaturateOverlay = Require(load, "phase_transition_desaturate_overlay_960x540_0_1_2_4.png");
      _focusMask = Require(load, "phase_transition_focus_mask_960x540_0_1_2_4.png");
      _playerAcquireTrail = Require(load, "card_acquire_trail_player_320x96_0_1_2_4.png");
      _aiAcquireTrail = Require(load, "card_acquire_trail_ai_320x96_0_1_2_4.png");
      _candidateIdle = Require(load, "private_card_candidate_idle_124x172_0_1_2_4.png");
      _candidateHover = Require(load, "private_card_candidate_hover_124x172_0_1_2_4.png");
      _candidateSelected = Require(load, "private_card_candidate_selected_124x172_0_1_2_4.png");
      _candidateConfirmed = Require(load, "private_card_candidate_confirmed_124x172_0_1_2_4.png");
      _candidateDisabled = Require(load, "private_card_candidate_disabled_124x172_0_1_2_4.png");
      _correctBellSheet = Require(load, "bell_correct_glint_6f_384x64_0_1_2_4.png");
      _wrongBellSheet = Require(load, "bell_wrong_impact_6f_384x64_0_1_2_4.png");
      _showdownWideFrame = Require(load, "showdown_wide_frame_760x420_0_1_2_4.png");
      _showdownHandLockedFrame = Require(load, "showdown_hand_locked_frame_420x180_0_1_2_4.png");
      _bestHandHighlight = Require(load, "result_best_hand_highlight_420x96_0_1_2_4.png");
      _resultSummaryFrame = Require(load, "result_summary_frame_720x360_0_1_2_4.png");
      _stageClearFrame = Require(load, "stage_clear_frame_560x240_0_1_2_4.png");
      _playerDamageFlash = Require(load, "hp_damage_flash_player_96_0_1_2_4.png");
      _aiDamageFlash = Require(load, "hp_damage_flash_ai_96_0_1_2_4.png");
      _opponentCutouts = opponentCutouts ?? Array.Empty<Texture2D>();
      _opponentPortraits = opponentPortraits ?? Array.Empty<Texture2D>();
      _threeCallIcon = threeCallIcon ?? _threeCallIcon;
      _showdownIcon = showdownIcon ?? _showdownIcon;
      _limitOne = limitOne ?? _limitOne;
      _limitTwo = limitTwo ?? _limitTwo;
      _usedOne = usedOne ?? _usedOne;
      _limitExhausted = limitExhausted ?? _limitExhausted;
      _inventoryRestricted = inventoryRestricted ?? _inventoryRestricted;
      _cardRestricted = cardRestricted ?? _cardRestricted;
    }

    public Texture2D ThreeCallIcon => _threeCallIcon;
    public Texture2D ShowdownIcon => _showdownIcon;
    public Texture2D EntryLabelFrame => _entryLabelFrame;
    public Texture2D SkipIdle => _skipIdle;
    public Texture2D SkipHover => _skipHover;
    public Texture2D SkipPressed => _skipPressed;
    public Texture2D OpponentIntroFrame => _opponentIntroFrame;
    public Texture2D TableFocusVignette => _tableFocusVignette;
    public Texture2D LimitOne => _limitOne;
    public Texture2D LimitTwo => _limitTwo;
    public Texture2D UsedOne => _usedOne;
    public Texture2D LimitExhausted => _limitExhausted;
    public Texture2D InventoryRestricted => _inventoryRestricted;
    public Texture2D CardRestricted => _cardRestricted;
    public Texture2D PenaltyLabelFrame => _penaltyLabelFrame;
    public Texture2D DesaturateOverlay => _desaturateOverlay;
    public Texture2D FocusMask => _focusMask;
    public Texture2D PlayerAcquireTrail => _playerAcquireTrail;
    public Texture2D AiAcquireTrail => _aiAcquireTrail;
    public Texture2D CandidateIdle => _candidateIdle;
    public Texture2D CandidateHover => _candidateHover;
    public Texture2D CandidateSelected => _candidateSelected;
    public Texture2D CandidateConfirmed => _candidateConfirmed;
    public Texture2D CandidateDisabled => _candidateDisabled;
    public Texture2D CorrectBellSheet => _correctBellSheet;
    public Texture2D WrongBellSheet => _wrongBellSheet;
    public Texture2D ShowdownWideFrame => _showdownWideFrame;
    public Texture2D ShowdownHandLockedFrame => _showdownHandLockedFrame;
    public Texture2D BestHandHighlight => _bestHandHighlight;
    public Texture2D ResultSummaryFrame => _resultSummaryFrame;
    public Texture2D StageClearFrame => _stageClearFrame;
    public Texture2D PlayerDamageFlash => _playerDamageFlash;
    public Texture2D AiDamageFlash => _aiDamageFlash;

    public Texture2D GetOpponentCutout(int stageNumber)
    {
      return AtStage(_opponentCutouts, stageNumber);
    }

    public Texture2D GetOpponentPortrait(int stageNumber)
    {
      return AtStage(_opponentPortraits, stageNumber);
    }

    private static Texture2D AtStage(Texture2D[] textures, int stageNumber)
    {
      if (textures == null || textures.Length == 0) return null;
      var index = Math.Max(0, Math.Min(textures.Length - 1, stageNumber - 1));
      return textures[index];
    }

    private static Texture2D Require(Func<string, Texture2D> load, string fileName)
    {
      return load(fileName) ?? throw new ArgumentNullException(fileName);
    }
  }
}
