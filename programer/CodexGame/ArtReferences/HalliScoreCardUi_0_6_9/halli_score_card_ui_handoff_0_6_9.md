# Halli score and card UI 0.6.9 handoff

- IntegrationId: `HALLI-PREDICTION-UI-0.6.9`
- DecisionId: `D-ART-HALLI-SCORE-CARDS-2026-08-24-01`
- ArtPackageVersion: `HalliScoreCardUi_0_6_9`
- Art source base (`origin/dev`): `3005bc8121530657ffd0e5627ae5bd1017244c21`
- Latest integrated visual/code reference: `12aa900094388f96c11329d3d05adbbb1b5e9bf3`
- Approval: user approved the 960x540 preview on 2026-08-24.
- Current state: `ART_PRODUCED=YES`, `ART_HANDOFF_READY=YES`, `ART_APPROVED=YES`, `RUNTIME_BOUND=NO`, `SCENE_SAVED=NO`, `BUILD_VERIFIED=NO`.

## Final visual contract

1. Round wins are no longer split between the lower-left and upper-right corners.
2. Player and AI each show three 32x32 medallions directly under their HP block.
3. Player uses cyan filled/empty assets; AI uses red filled/empty assets. Do not draw duplicate fallback pips elsewhere.
4. The Halli pile contract remains unchanged: maximum two cards, horizontal overlap no more than 10%, newest revealed card on top.
5. The lower-left tray keeps a separate token compartment and exposes exactly three acquired-card positions. Empty positions remain visually quiet.
6. Bells, public card, draw deck, timer, card IDs, ranks, suits and game rules do not change.

## Runtime mapping for programmer/integrator

All coordinates below are Unity IMGUI coordinates for a 960x540 reference canvas.

### `HalliDevPanel.DrawRoundWins`

- Player origin: change `(18, 365)` to `(99, 84)`.
- AI origin: change `(790, 92)` to `(737, 84)`.
- Pip rect remains `32x32`; horizontal step remains `46`.
- Continue using these already approved textures:
  - `Assets/Art/Prototype/UI/IconOverhaul_0_5_0/round_win_badge_player_empty_32_0_5_0.png`
  - `Assets/Art/Prototype/UI/IconOverhaul_0_5_0/round_win_badge_player_filled_32_0_5_0.png`
  - `Assets/Art/Prototype/UI/IconOverhaul_0_5_0/round_win_badge_ai_empty_32_0_5_0.png`
  - `Assets/Art/Prototype/UI/IconOverhaul_0_5_0/round_win_badge_ai_filled_32_0_5_0.png`

### `HalliBoardLayout` and `HalliLowerHudRenderer`

- Keep `PlayerTray = (34, 390, 378, 130)` and the existing `Halli_0_3_4` open tray texture.
- Change player-only maximum visible cards from `5` to `3`.
- Set player-only acquired card slots to:
  - slot 0: `(152, 419, 56, 78)`
  - slot 1: `(230, 419, 56, 78)`
  - slot 2: `(308, 419, 56, 78)`
- Equivalent constants: `PlayerOnlyCardStartX=118`, `PlayerOnlyCardStartY=29`, `PlayerOnlyCardStepX=78`.
- When more than three cards are owned, show the newest three; do not shrink cards and do not create a second row.
- Acquisition motion must terminate at the same slot returned by `PlayerOnlyAcquiredCard`.

### Halli pile regression lock

Do not change `HalliPileOverlapLayout` values from the latest contract:

- `MaximumPileCards=2`
- `CardWidth=64`, `CardHeight=90`
- `CardStepX=59` (5px overlap, 7.8125%)
- draw order keeps the newest card on top

## Files not owned by this art package

- No modification to `PlayableDev.unity`, `PlayableDevSceneBuilder`, UI ArtSet, audio, rules, localization, `ProjectSettings` or existing `.meta`.
- The preview is a layout approval reference, not a full-screen runtime texture.
- The integrator must implement the mapping in code, regenerate/save the scene if required, and return a 960x540 runtime capture.

## Acceptance checks

- Player 1/3 and AI 0/3 visibly match the approved preview.
- Both pip groups are centered under their corresponding HP blocks.
- No score pips remain at the lower-left table edge.
- The lower tray shows token + three card slots; cards do not overlap the tray border.
- One, two and three acquired-card cases are readable; four or more display only the newest three.
- Halli piles still pass the two-card/10%/newest-on-top regression test.
- Verify ko/en at 960x540; no new text was added by this package.

## Evidence

- Approved preview: `preview/halli_score_card_ui_approved_960x540_0_6_9.png`
- Before capture: `source/halli_score_card_ui_before_960x540.png`
- Preview SHA-256: `cb34b121a8b02eba3f90930dcda335be71a41b9c3b2b0e2730f4e14f814e08f5`
