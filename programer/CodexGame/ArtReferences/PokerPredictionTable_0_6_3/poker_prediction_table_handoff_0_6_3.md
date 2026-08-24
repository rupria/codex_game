# Poker prediction table UI 0.6.3 handoff

- IntegrationId: `HALLI-PREDICTION-UI-0.6.9`
- DecisionId: `D-ART-POKER-PREDICTION-TABLE-2026-08-24-01`
- ArtPackageVersion: `PokerPredictionTable_0_6_3`
- Art source base (`origin/dev`): `3005bc8121530657ffd0e5627ae5bd1017244c21`
- Latest integrated visual/code reference: `12aa900094388f96c11329d3d05adbbb1b5e9bf3`
- Approval: user approved the 960x540 preview on 2026-08-24.
- Current state: `ART_PRODUCED=YES`, `ART_HANDOFF_READY=YES`, `ART_APPROVED=YES`, `RUNTIME_BOUND=NO`, `SCENE_SAVED=NO`, `BUILD_VERIFIED=NO`.

## Final visual contract

1. The prediction phase uses the circular table as the layout boundary; do not add a full-screen rectangular modal.
2. Card counts are fixed to AI private 3, community maximum 2, player private 3.
3. Card identities are runtime data. The cards in the preview are examples only and must never be hard-coded or baked into a full-screen image.
4. Each group receives compact spacing and a subtle local card grounding treatment. Do not crop cards.
5. The lower choice buttons remain `승리 예측` and `패배 예측`, with teal/player and red/AI color ownership.
6. The item chest and `예측 성공 {count}/5` remain one compact lower-right group.
7. Insurance UI remains governed by the existing rule: show only `남은 보험` when insurance is active. Do not restore a right-side charge panel.

## Runtime mapping for programmer/integrator

All coordinates are Unity IMGUI coordinates for a 960x540 reference canvas.

### `PokerTableLayout`

Recommended approved positions:

- `PredictionTitlePlate = (350, 20, 260, 52)`
- `PredictionStageEmblem = (360, 26, 40, 40)`
- `PredictionTitleText = (410, 32, 180, 28)`
- AI cards, `64x90`: `(374, 78)`, `(448, 78)`, `(522, 78)`
- Community cards, `64x90`: `(411, 196)`, `(485, 196)`; maximum remains two
- Player cards, `64x90`: `(374, 326)`, `(448, 326)`, `(522, 326)`
- Player health: keep left of the player cards without covering the lower buttons.
- AI health: keep left of the AI cards without covering the timer.
- `PlayerItem` and the existing item chest remain near `(650, 350, 88, 76)`.
- `PredictionSuccessPlate = (716, 366, 224, 44)`
- `PredictionSuccessIcon = (730, 374, 28, 28)`
- `PredictionSuccessText = (766, 374, 160, 28)`
- Win visual/button: `(236, 456, 212, 64)`; hit rect expands by 8px on all sides.
- Loss visual/button: `(512, 456, 212, 64)`; hit rect expands by 8px on all sides.
- Center button labels vertically and keep at least 24px safe area from the bottom edge.

The existing 244x66 button textures may be scaled with `ScaleToFit`; do not create duplicate button art.

### Existing assets to reuse

- `Assets/Art/Prototype/UI/PokerPredictionClean_0_6_2/poker_prediction_title_plate_308x52_0_6_2.png`
- `Assets/Art/Prototype/UI/PokerPredictionClean_0_6_2/poker_prediction_player_*_244x66_0_6_2.png`
- `Assets/Art/Prototype/UI/PokerPredictionClean_0_6_2/poker_prediction_ai_*_244x66_0_6_2.png`
- `Assets/Art/Prototype/UI/PokerPredictionClean_0_6_2/poker_prediction_success_icon_28_0_6_2.png`
- Existing card renderer, health renderer and item chest art.

### Elements to remove from this phase

- Long full-width decorative bars above AI cards and above the lower buttons.
- Full-screen or large gray/black rectangular background panels.
- Any card-selection highlight. Prediction is a button choice, not private-card selection.
- Any third community slot or card.

## Files not owned by this art package

- No modification to poker rules, prediction outcomes, item rules, localization, audio, scenes, ArtSet, existing PNG or `.meta`.
- The preview is a layout approval reference, not a full-screen runtime texture.
- The integrator owns `PokerTableLayout.cs`, `PokerDevPanel.cs`, tests and scene verification.

## Acceptance checks

- AI private cards: exactly three back-facing cards, none clipped.
- Community cards: zero to two; two-card state remains centered and no third slot appears.
- Player private cards: exactly three in the normal state, none clipped.
- `승리 예측` and `패배 예측` are readable at 960x540 in ko/en layouts.
- Item chest and `예측 성공 0/5` do not overlap cards or buttons.
- No full-screen rectangular modal and no long decorative rails remain.
- Existing prediction, insurance, result and audio logic remain unchanged.

## Evidence

- Approved preview: `preview/poker_prediction_table_approved_960x540_0_6_3.png`
- Before capture: `source/poker_prediction_table_before_960x540.png`
- Preview SHA-256: `42654a68547a1c6cc2d31d41bfa7a85d886de9117edf787128f46adf817a2ef5`
