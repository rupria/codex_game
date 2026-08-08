# #26 스테이지 이동 연출 아트 인계 0.3.1

## 결과

기존 `NextStageTransition`의 1.2초 입력 잠금·안내 상태 뒤에 실제로 보여 줄 `가게 퇴장 → 걷기 → 문 통과 → 암전 → 로딩 → 다음 스테이지` 아트 팩을 제작했다. 상점과 전투의 규칙 코드는 수정하지 않았으며, Unity 카메라·Spot Light·씬 로딩에 연결할 수 있도록 모든 요소를 배경·문·FX·로딩 레이어로 분리했다.

## 확정 연출 흐름

| 순서 | 상태 | 권장 시간 | 화면 |
|---:|---|---:|---|
| 1 | `shop_ui_clear` | 0.22초 | 상점 상품·버튼·수치 UI가 먼저 정리된다. |
| 2 | `camera_turn_to_exit` | 0.32초 | 카메라가 바에서 물러나 출구 정면 구도로 전환한다. |
| 3 | `walk_to_door` | 0.65초 | 출구로 전진하며 약한 카메라 보브, 비네트, 바닥 먼지를 사용한다. |
| 4 | `push_swing_doors` | 0.18초 | 좌·우 스윙도어가 중앙에서 바깥쪽으로 열린다. |
| 5 | `cross_threshold` | 0.28초 | 문틀이 화면 바깥으로 빠지도록 카메라가 한 번 더 전진한다. |
| 6 | `fade_out_and_begin_load` | 0.25초 | 검정 페이드와 동시에 다음 스테이지 로딩을 시작한다. |
| 7 | `loading_loop` | 실제 로딩 시간 | 로딩이 남아 있을 때만 중앙 해골 링 8프레임을 반복한다. |
| 8 | `next_stage_fade_in` | 0.35초 | 다음 스테이지 카메라가 준비된 뒤 검정에서 복귀한다. |

고정 프리로드 구간은 1.90초다. 기존 1.2초 타이머가 끝났다고 입력을 풀지 말고, 다음 스테이지의 페이드인이 완료될 때까지 입력 잠금을 유지한다.

## 전달 파일

### 배경

- `stage_exit_background_closed_unlit_960x540_0_3_1.png`
- `stage_exit_background_open_unlit_960x540_0_3_1.png`

배경에는 천장 조명 기구, 강한 고정 광원, 외부 일광을 굽지 않았다. Unity Spot Light와 카메라 리그가 장면 강조를 담당한다.

### 좌·우 스윙도어

- `stage_exit_door_left_01~04_128x210_0_3_1.png`
- `stage_exit_door_right_01~04_128x210_0_3_1.png`
- 상태 확인: `stage_exit_door_states_contact_sheet_1024x230_0_3_1.png`

권장 Pivot은 왼쪽 문 `(0, 0.5)`, 오른쪽 문 `(1, 0.5)`다. 2D 프로토타입에서는 4상태 Sprite를 순차 교체하고, 최종 3D 구현에서는 동일한 실루엣·시간을 기준으로 문 오브젝트를 Y축 회전해도 된다.

### 보행·페이드 FX

- `stage_exit_walk_dust_01~04_96x64_0_3_1.png`
- `stage_exit_walk_vignette_960x540_0_3_1.png`
- `stage_transition_fade_black_16_0_3_1.png`

먼지는 화면 하단 중앙의 짧은 보조 효과다. 매 걸음마다 강하게 반복하지 않고, 0.65초 접근 구간에 한 번만 약하게 사용한다. 비네트는 카메라 보브가 있을 때 0 → 0.35 → 0.15 정도로 알파를 조정한다.

### 로딩

- `stage_transition_loading_01~08_64_0_3_1.png`
- `stage_transition_loading_64_0_3_1.gif`
- `stage_transition_loading_64_0_3_1.aseprite`
- `stage_transition_loading_contact_sheet_576x80_0_3_1.png`

8프레임, 프레임당 0.09초, 무한 반복이다. 이미지 안에 `LOADING` 같은 문자열을 굽지 않았다. 로딩이 `minimumBlackHoldSeconds=0.15` 안에 끝나면 링을 표시하지 않고 바로 다음 스테이지 페이드인으로 넘어가도 된다.

### 전체 참고

- `stage_transition_storyboard_1920x720_0_3_1.png`
- `stage_transition_storyboard_960x540_0_3_1.gif`
- `stage_transition_storyboard_960x540_0_3_1.aseprite`
- `stage_transition_preview_exit_closed_960x540_0_3_1.png`
- `stage_transition_preview_door_open_960x540_0_3_1.png`
- `stage_transition_preview_loading_960x540_0_3_1.png`
- `stage_transition_art_catalog_0_3_1.json`

스토리보드 PNG·GIF는 방향 확인용이다. 런타임에서 평면 영상 한 장을 재생하지 말고, 분리된 배경·문·FX·로딩을 `StageTransitionUiArtSet` 같은 주입 경계로 연결한다.

## Aseprite 원본 레이어

`stage_transition_storyboard_960x540_0_3_1.aseprite`

1. `01_Shop_Unlit_Reference`
2. `02_Exit_Backgrounds_Unlit`
3. `03_Swing_Door_States`
4. `04_Walk_Dust_And_Vignette`
5. `05_Fade_To_Black`
6. `06_Loading_Indicator_No_Text`

## Unity 연결 요청

1. `StageWon → BarShop → NextStageTransition`에서 다음 진행 버튼이 승인되면 상점 UI 입력을 즉시 막는다.
2. 기존의 고정 1.2초 종료 타이머 대신 전환 시퀀스 완료와 로드 완료를 함께 기다린다.
3. `camera_turn_to_exit`, `walk_to_door`, `cross_threshold`는 기존 Presentation Rig/Cinemachine 계층에서 구현하고 게임 규칙 상태와 분리한다.
4. 문은 2D Sprite 교체 또는 3D 문 오브젝트 회전 중 하나만 사용한다. 두 연출을 겹치지 않는다.
5. 검정 페이드가 완전히 덮인 뒤 다음 스테이지 씬/상태를 활성화한다.
6. 로딩이 길어질 때만 8프레임 링을 표시한다. 로딩 링이 떠 있는 동안 모든 전투·상점 입력과 전투 타이머는 비활성이다.
7. 로드 완료, 페이드인 완료, 입력 해제, 스테이지 증가를 각각 중복 호출하지 않도록 동일 전환 토큰을 사용한다.
8. 다음 스테이지 카메라와 Spot Light의 초기 위치가 준비된 뒤 페이드인을 시작한다. 준비 전 한 프레임 노출을 허용하지 않는다.
9. 화면 문자열이 필요하면 Unity 현지화 텍스트로 출력한다. 제공 이미지에는 문자열이 없다.

## 수용 기준

- 다음 진행 입력 후 상점 UI가 먼저 사라지고 출구가 화면 중심에 들어온다.
- 카메라 전진, 문 열림, 문턱 통과가 순서대로 보여 카드 게임 화면에서 순간 이동한 느낌이 나지 않는다.
- 문이 닫힌 배경 위에 분리 문 Sprite를 중복 표시하지 않는다.
- 암전 전에 다음 스테이지가 노출되지 않는다.
- 빠른 로딩에서는 검정 화면이 최소 0.15초 유지되고, 느린 로딩에서는 해골 링이 끊김 없이 반복된다.
- 페이드인이 끝나기 전에 입력 또는 전투 타이머가 활성화되지 않는다.
- 다음 스테이지 증가와 보상 적용은 한 번만 발생한다.
- 1280×720, 1920×1080에서 문·비네트·로딩의 기준 위치가 16:9 Canvas Scaler로 유지된다.

## 생성 방식

- 출구 배경: 기존 `BarShop_0_3_0` 상점 배경을 스타일 기준으로 한 ImageGen 기본 도구 생성·정밀 편집
- 문·먼지·비네트·페이드·로딩·스토리보드·Aseprite 원본: Aseprite 1.3.18.1
- 생성 프롬프트 핵심: 동일한 어두운 서부 살룬, 중앙 스윙도어, 1인칭 바닥 동선, 무조명, 무문자, Unity 조명용 배경
