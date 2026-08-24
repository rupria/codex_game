# Unity Open Project #1 audio sample

- Source: https://github.com/UnityTechnologies/open-project-1
- Source revision: `608eac98df29cd97821a6115cd52dfb9027345b1`
- Publisher: Unity Technologies
- License: Apache License 2.0
- Modification: none; original WAV files are copied byte-for-byte.
- Purpose: temporary playable SFX sample. Music is intentionally excluded.

## Included files

| File | Original path | SHA-256 |
|---|---|---|
| `Interface_05.wav` | `UOP1_Project/Assets/Audio/SFX/Interface/Interface_05.wav` | `2E9DC2F0E9E2DF877AA1057012DD362A6109D7096F0A8468EAD3BA56E75D7BD2` |
| `Interface_06.wav` | `UOP1_Project/Assets/Audio/SFX/Interface/Interface_06.wav` | `4BDEF5C79524B93F96922F62103C3606BD56B73116087CBEDC4EA45E108829FF` |
| `Interface_07.wav` | `UOP1_Project/Assets/Audio/SFX/Interface/Interface_07.wav` | `58F161325D34B4CBC4094852CFEBB8A299979CC950C89E26745910BC094DD47F` |
| `Interface_08.wav` | `UOP1_Project/Assets/Audio/SFX/Interface/Interface_08.wav` | `5B5EA77D2E71AE5BB448B32E7DCCA6D1872BB776A78E6F0017E5BB789B486934` |
| `PhoenixChick_Flap.wav` | `UOP1_Project/Assets/Audio/SFX/Characters/Actions/PhoenixChick_Flap.wav` | `D3D1F8BCD3A3B3B4E33E4A74AFCF4D0484C81BC4FCD1DD4FB282099844344149` |
| `Env_Pots_09.wav` | `UOP1_Project/Assets/Audio/SFX/Environment/Env_Pots_09.wav` | `50AAE985FC910033A1864E6B5C43004B8C903DA40B26C9BBB9A9780CA968A158` |
| `Grabbing_01.wav` | `UOP1_Project/Assets/Audio/SFX/Characters/Actions/PigChef/Grabbing_01.wav` | `2D3BC5B8B944AC78C6BA0732D36C57F1A1EB8E1E75861A83A73BBD4B85D601E0` |
| `AttackLanding_Rock.wav` | `UOP1_Project/Assets/Audio/SFX/Characters/Actions/Critters/AttackLanding_Rock.wav` | `983E84819E729F7AB60524879E663D8625F5768CFB30359E01019B27BF2FCE9D` |
| `Swing_Cane_01.wav` | `UOP1_Project/Assets/Audio/SFX/Characters/Actions/PigChef/Swing_Cane_01.wav` | `783B9E7270486AB86A6DAA69BE9647FDB36F46FF40F8E86172F493BA4AFDCC40` |
| `Swing_Cane_03.wav` | `UOP1_Project/Assets/Audio/SFX/Characters/Actions/PigChef/Swing_Cane_03.wav` | `53EBB0BBAA38C7FBC96BA5F21ED0D4225048ACA5C0792F58374E2EFF7DBE8D01` |

## Runtime cue decision

- DecisionId: `SOUND-CARD-FLIP-0.1.2-20260825`
- Supersedes: the `SOUND-DYNAMICS-0.1.1` card-flip pool only. Other cue pools remain unchanged.
- Primary implementation: `PlayableAudioDirector.cs`
- The original WAV files remain unmodified. Variation is applied only at playback time.
- High-frequency UI and item cues use 2-3 clip pools with immediate-repeat prevention.
- Card reveal uses only `Swing_Cane_03.wav`; it no longer rotates into tonal UI clips.
- Runtime loading prefers `Swing_Cane_03.wav` over an older scene-serialized card clip,
  while retaining the older clip only as an import fallback.
- Card reveal playback uses a restrained `0.97-1.03` pitch range and the slightly louder
  `0.86-1.00` cue-volume range requested during listening review.
- Each cue owns a constrained pitch, volume, stereo-pan and cooldown range.
- Bell, error and terminal cues use narrower ranges so their gameplay meaning stays recognizable.
- Four rotating SFX voices limit stacking while allowing short cues to overlap.
- Music remains intentionally unassigned.

Current formal state on `sound` is `SOURCE_PRESENT=YES · SOUND_BRANCH_CODE_READY=YES ·
CODE_BOUND=NO · SCENE_BOUND=NO · BUILD_EXPOSED=NO · LISTENING_QA_PENDING`.
The lead integration pass owns the new AudioImporter `.meta`, `dev` integration, saved-scene
verification and build exposure.
