# 할리갈리 UI 아트 소스 인계 0.1.0

## 기준

- 기획 기준: `project_docs/designer/게임진행플로우_0.1.0.md`
- 프로그래머 인계 기준: `project_docs/programer/26.08.07_기획인계_할리갈리_UI_연출_개선.md`
- 기준 커밋: `77c9917c8e2bc2aeeab168c5c0d7e2d3c607e84c`
- 화면 기준 해상도: 960×540, 안전 여백 5%
- 기존 승인 배경/배치: `../Halli_0_06/halli_screen_layout_central_bells_mock_960x540_0_06.png`

이 폴더는 화면 레퍼런스와 생성 원본을 보관한다. 실제 Unity 적용용 투명 PNG는
`Assets/Art/Prototype/UI/Halli_0_1_0/`에 분리되어 있다. 프로그래밍 코드와 프리팹은 수정하지 않았다.

## Unity 적용용 아트

| 파일 | 규격 | 용도 |
|---|---:|---|
| `bell_correct.png` | 128×128 RGBA | 정답 판정 직후의 성공 강조 |
| `bell_disabled.png` | 128×128 RGBA | 입력 잠금·판정 대기 상태 |
| `flip_deck_idle.png` | 128×160 RGBA | 중앙 하단 카드 펼치기 더미 기본 상태 |
| `flip_deck_hover.png` | 128×160 RGBA | W/마우스 포커스 상태 |
| `flip_deck_pressed.png` | 128×160 RGBA | 카드 4장 순차 배분 시작 순간 |
| `flip_deck_disabled.png` | 128×160 RGBA | 연출 재생 중 또는 입력 불가 상태 |
| `player_acquired_tray.png` | 384×128 RGBA | 좌측 하단 플레이어 획득 카드 최대 3장 보관 틀 |
| `ai_acquired_status_panel.png` | 384×128 RGBA | 우측 하단 AI 판단·카드 뒷면·보유 장수 표시 |

모든 Unity PNG에는 Point 필터, 무압축, 밉맵 없음, 투명 알파 설정의 `.meta`를 동봉했다.

## 화면 레퍼런스

| 파일 | 용도 |
|---|---|
| `halli_start_screen_mock_960x540_0_1_0.png` | 확정 방향의 시작 화면. 버튼 문구는 `START`, `GUIDE`만 사용 |
| `halli_guide_overlay_mock_960x540_0_1_0.png` | 텍스트를 줄인 3단계 그림형 가이드 |
| `halli_ui_asset_preview_960x540_0_1_0.png` | 신규 UI 소스를 한 화면에서 확인하는 검수용 보드 |

## 적용 규칙

1. `bell_correct`는 플레이어가 누르기 전 정답 힌트로 표시하지 않는다. 판정 완료 뒤에만 사용한다.
2. 기존 `bell_idle`, `bell_hover`, `bell_pressed`, `bell_wrong`과 이번 `bell_correct`, `bell_disabled`를 한 상태 세트로 사용한다.
3. 카드 펼치기 입력 한 번에 `플레이어 왼쪽 → AI 왼쪽 → 플레이어 오른쪽 → AI 오른쪽` 순서로 한 장씩 출력한다.
4. 4장 출력 연출 중에는 `flip_deck_disabled`와 `bell_disabled`를 사용하고 네 번째 카드가 놓인 뒤 벨 입력을 연다.
5. `player_acquired_tray`의 카드 슬롯은 빈 틀이다. 실제 획득한 포커 카드 앞면을 그 위에 별도 오브젝트로 배치한다.
6. AI 획득 카드는 판정 직후 앞면을 잠깐 보여준 다음 `ai_acquired_status_panel`의 뒷면과 장수로 전환한다.
7. AI 패널은 상태 표시용이며 플레이어 입력 버튼으로 사용하지 않는다.
8. 기존 승인 배경은 교체하지 않고, 이번 소스를 그 위의 UI 레이어로 배치한다.

## 생성 원본 및 권리 기록

- 제작 방식: OpenAI 내장 ImageGen으로 신규 생성 후 크로마 제거·최근접 보간 분리
- 외부 이미지 또는 유료 에셋을 직접 포함하지 않음
- 생성 프롬프트 요약:
  - 어두운 청남색 픽셀 아트, 아이보리·금색 벨, 청록 성공 링, 회색 비활성 상태
  - 승인 카드 뒷면 문양의 3장 더미, 기본·청록 포커스·금색 누름·회색 비활성 4상태
  - 청록 플레이어 획득 카드 3슬롯 패널과 붉은 AI 판단·보유 장수 패널
  - 타원형 테이블과 해골·벨 심볼, `START`·`GUIDE`만 노출하는 시작 화면
  - W로 4장 펼치기, Q/E 벨 판정, 획득 카드 포커 활용을 그림으로 설명하는 가이드
- 원본과 투명 시트는 `sources/`에 보관했다.
- 프로젝트 배포 전 최종 라이선스·상표 검토는 저장소 운영 정책에 따라 진행한다.

## 0.1.1 누락 리소스 보완

2026-08-08에 `게임진행플로우_0.1.0`, 프로그래머 인계서와 아트 요청 산출물을 다시 대조했다.
기존 카드 2종 덱, 벨 상태, HP·승수, 타이머, 공용 카드 잠금 슬롯은 재사용 가능함을 확인했다.
개발 적용 경로에 없던 다음 소스를 추가했다.

### Unity 적용용 추가 파일

| 파일 | 규격 | 용도 |
|---|---:|---|
| `card_select_idle.png` | 80×106 RGBA | 선택 해제·기본 상태 |
| `card_select_hover.png` | 80×106 RGBA | 마우스 탐색 상태 |
| `card_select_selected.png` | 80×106 RGBA | 현재 선택 카드 |
| `card_select_confirmable.png` | 80×106 RGBA | 필요한 카드 수가 충족된 확정 가능 상태 |
| `card_select_disabled.png` | 80×106 RGBA | 입력 잠금·선택 불가 상태 |
| `start_screen_background.png` | 960×540 RGB | `START`, `GUIDE` 시작 화면 런타임 배경 |
| `guide_overlay_background.png` | 960×540 RGB | 이미지 중심 조작 가이드 런타임 배경 |

선택 프레임은 카드 앞면 위에 별도 UI 레이어로 겹친다. 프레임 자체에 카드 랭크·문양 정보를 넣지 않는다.
확정 가능 상태는 금색, 탐색·선택은 청록, 비활성은 저채도 회색과 잠금 표시를 사용한다.

### 화면 전환 추가 시안

| 파일 | 용도 |
|---|---|
| `halli_first_community_reveal_mock_960x540_0_1_1.png` | START 직후 딜러의 첫 공용 카드 확대·고정 구도 |
| `halli_poker_wide_mock_960x540_0_1_1.png` | 할리갈리 종료 후 벨·필드 제거, 포커 손패와 공용 카드 중심의 와이드 구도 |
| `halli_gapfill_preview_960x540_0_1_1.png` | 두 카메라 구도와 카드 선택 5상태 통합 검수 보드 |

포커 와이드 시안의 K·Q는 기존 결정에 맞춰 해골 로열 일러스트를 사용한다. 숫자 카드는 일반 포커 배열을 사용하며
할리갈리 판정용 중앙 해골 수 표시는 넣지 않는다.

### 0.1.1 생성 기록

- 제작 방식: OpenAI 내장 ImageGen으로 첫 공용 카드 공개·포커 와이드 시안을 생성하고, 960×540 최근접 보간으로 저장
- 입력 레퍼런스: 저장소의 승인 할리갈리 배치, 카드 뒷면, 포커 카드, AI 실루엣
- 프롬프트 요약:
  - 첫 공개: 딜러 실루엣, 확대 중인 공용 카드 1장, 첫 슬롯 청록 강조, 두 번째 슬롯 잠금, 벨 입력 비활성
  - 포커 와이드: 두 공용 카드, 플레이어 앞면 3장, AI 뒷면 3장, 할리갈리 벨·더미 제거, 해골 K·Q 로열
- 카드 선택 프레임은 프로젝트 색상표에 따라 결정적으로 제작한 투명 픽셀 UI이며 외부 소스를 포함하지 않는다.
- 생성 원본은 `sources/halli_first_community_reveal_source_0_1_1.png`와
  `sources/halli_poker_wide_source_0_1_1.png`에 보관했다.
