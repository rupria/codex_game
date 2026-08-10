# 인게임 텍스트 최소화·재화 UI 아트 0.4.0

기준 문서: `07_인게임_텍스트_최소화_및_재화_UI_기준.md` (2026-08-10)

이 패키지는 기존에 승인된 0.3.9 서부식 탄환과 캐릭터 실루엣을 확장한다. 같은 요청을 새 자산으로 중복 제작하지 않고, 기존 자산이 없는 재화 구분·무문자 경고·초상 예측 상태만 추가했다.

모든 항목은 동일 빌드에 적용하는 필수 계약이다. 우선순위 또는 선택 적용 항목이 아니다.

## 런타임 경로

`Assets/Art/Prototype/UI/Textless_Currency_0_4_0/`

## 재화 아이콘

- `currency_basic_bullet_40x40_0_4_0.png`: 온전한 탄환. 기본 재화.
- `currency_temporary_cracked_hourglass_40x40_0_4_0.png`: 탄피 균열과 모래시계 배지를 함께 넣은 일회성 재화. 색을 제거해도 기본 재화와 실루엣이 다르다.
- `shop_price_bullet_24x24_0_4_0.png`: 상품 가격 앞에 배치하는 온전한 탄환 축약 아이콘.
- `battle_currency_basic_panel_112x52_0_4_0.png`: 전투 화면 고정 기본 재화 패널.
- `battle_currency_temporary_panel_112x52_0_4_0.png`: 획득 직후에만 노출 가능한 일회성 재화 보조 패널.
- `shop_currency_dual_panel_240x64_0_4_0.png`: 상점 고정 패널. 왼쪽 일회성 → 오른쪽 기본 순서.

숫자는 이미지에 굽지 않는다. 각 검은 숫자 영역에 현재 수량을 TMP로 올리고, 재화 이름은 표시하지 않는다.

## 상점 이탈 경고·소멸

- `shop_exit_warning_badge_24x24_0_4_0.png`
- `shop_exit_warning_pulse_6f_144x24_0_4_0.png`: 24×24, 6프레임, 프레임당 0.07초.
- `currency_temporary_expire_8f_320x40_0_4_0.png`: 40×40, 8프레임, 총 0.58초 권장.

일회성 재화가 남은 첫 이탈 입력은 일회성 아이콘 흔들림과 출구 배지 점멸만 실행한다. 문장은 띄우지 않는다. 두 번째 이탈 입력은 소멸 시트를 재생한 뒤 전환한다.

## 5장 승부 예측

기존 체크·X 및 `승리/패배` 버튼은 사용하지 않는다. 플레이어 초상과 AI 초상 중 예상 승자를 직접 선택한다.

- `poker_predict_player_portrait_idle_88_0_4_0.png`
- `poker_predict_player_portrait_hover_88_0_4_0.png`
- `poker_predict_player_portrait_selected_88_0_4_0.png`
- `poker_predict_ai_portrait_idle_88_0_4_0.png`
- `poker_predict_ai_portrait_hover_88_0_4_0.png`
- `poker_predict_ai_portrait_selected_88_0_4_0.png`

시각 88×88, 권장 클릭 영역 104×104. 두 선택지는 테이블 중심축을 기준으로 좌우 대칭 배치하며 텍스트 라벨을 붙이지 않는다.

## Aseprite 원본

- `source/currency_basic_temporary_states_0_4_0.aseprite`
- `source/shop_currency_dual_panel_0_4_0.aseprite`
- `source/shop_exit_warning_pulse_0_4_0.aseprite`
- `source/currency_temporary_expire_0_4_0.aseprite`
- `source/poker_prediction_portrait_states_0_4_0.aseprite`

재생성:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\source\generate_textless_currency_0_4_0.ps1
```

## 중복 없이 재사용하는 기존 자산

- 기본 탄환 상세·광택·결제 연출: `BarShop_0_3_9`
- 주머니·손 오버레이·정확한 수량 표시: `BarShop_0_3_8`
- 할리갈리 벨·점등 승수 마커·하트: 기존 Halli UI 패키지
- 좌측 하단 플레이어 획득 카드 트레이: `Halli_0_3_4/player_acquired_tray_open_378x130_0_3_4.png`
- 포커 아이템 상자와 불투명 팝업: `Poker_0_3_4`, `Poker_0_3_6`, `Poker_0_3_7`

## 화면 문자열 정책

- `총알`, `일회성 총알`, `할리갈리`, `포커`, `승리`, `패배`, `카드 펼치기`, `왼쪽`, `오른쪽`: 일반 플레이 화면 `hidden`.
- Q/W/E: 텍스트 문장이 아닌 키캡 그래픽으로만 `icon_only`.
- 아이템 이름·효과: 포커스·마우스 오버·상세 보기에서만 `focus_only`.
- 수량·타이머 숫자: `always`.
- 접근성 이름과 개발·QA 로그의 내부 명칭은 유지한다.
