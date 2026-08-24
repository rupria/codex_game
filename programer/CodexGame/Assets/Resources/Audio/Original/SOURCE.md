# Original Western Audio Candidates

- DecisionId: `SOUND-RESULT-GUITAR-0.1.2-20260825`
- Supersedes: `SOUND-REFERENCE-BED-0.1.2-20260824`
- Branch: `sound`
- BaseGitCommit: `44ee0b1c5803a2c3e31df23a9a0deef191cc1148`
- TechnicalState: `SOURCE_PRESENT`
- ListeningState: `LISTENING_QA_PENDING`
- RuntimeState: `CODE_BOUND=NO · SCENE_BOUND=NO · BUILD_EXPOSED=NO`

## Direction references

- Lost Saga Western Town — `https://www.youtube.com/watch?v=SyxmlJHdInc`
- We are Desperados — `https://www.youtube.com/watch?v=LKUKUN6j5fY`

The reference recordings were not downloaded, sampled, traced, or bundled. Only the
measured high-level characteristics—Lost Saga's 56/112 BPM dual pulse and dense low-mid
motion, Desperados' restrained low-end weight, reduced high frequencies, and a non-bright
western atmosphere—were used. All
candidate waveforms are original local procedural
renders. The user's later direct-creation request supersedes the older Drive rule entry
that said direct sound creation had not started.

The paired result cues preserve their wooden state signal, then introduce a short second-
guitar answer between the opening and closing impacts. Win rises D-F-A; lose answers with
the matching A-F-D descent. The guitar layer is deliberately brief and softly filtered so
it reads as a western interjection without merging into the gameplay BGM.

The user supplied local MP3 files for analysis only. They are not stored in Git or copied
into the Unity project:

- Lost Saga reference SHA-256: `C0C9985B919404E7C4CF8FB22666A3B25443C9B6E2A0535DF520047777A8C2B0`
- We are Desperados reference SHA-256: `EC66615092316E857A05EA2CB6354EF4F8C04CFD22A37D250E11A1D21D22A60E`

## Candidate files

| File | Intended cue | Duration | SHA-256 |
|---|---|---:|---|
| `Western_Reference_Bed_Loop_12.wav` | Lost-Saga-leaning gameplay BGM loop; v11 rhythm preserved with softer guitar pick/string edge | 25.554 s | `C0A7C24BE283559909CADA13F1B7CAF009D0D9E77E81EEEF55C475C70D52A582` |
| `Prediction_Lock_03.wav` | Player submits and locks a poker prediction | 1.200 s | `A0336D1B1FAB69558C1ED35BB4056B432EBD1E2853A4F246BB8DCAD04C6D6FEB` |
| `Round_Win_06.wav` | `StageWon` / `RunWon` wooden result cue with a rising second-guitar answer | 2.200 s | `E184A66BD5636D91322021192E82FE11085286E612045A039679153C6D6BD01D` |
| `Round_Lose_05.wav` | `BattleFinished` wooden result cue with a descending second-guitar answer | 2.200 s | `5715DEAD1E55FD0CF8FEAE8D641D6D4052C79D98358F335AA6155CE96D32FFCF` |

## Integration boundary

- Sound owns these new WAV candidates and sound-only runtime code after listening approval.
- Sound does not modify existing `.meta`, `PlayableDevSceneBuilder`, `PlayableDev.unity`,
  shared UI classes, or `ProjectSettings`.
- The integration lead creates and approves importer metadata, scene binding, and build exposure.
- Do not promote these candidates beyond `SOURCE_PRESENT` until the user confirms listening approval.
