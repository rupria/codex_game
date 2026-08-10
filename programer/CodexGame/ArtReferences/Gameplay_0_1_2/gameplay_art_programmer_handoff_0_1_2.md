# 게임진행플로우 0.1.2 아트 연결 인계

## 적용 범위

- 아트 에셋, 배치, 상태, 연출 연결 규격
- 제외: 아이템 효과, 카드 판정, 승패 계산, 구매 로직
- 제작 도구: Aseprite 1.3.18.1
- 연결 이슈: [#37](https://github.com/rupria/codex_game/issues/37)

## 1. 상점

기준 시안: `../BarShop_0_3_4/preview/bar_shop_four_slot_pouch_layout_preview_960x540_0_3_4.png`

| 요소 | 960×540 기준 |
|---|---|
| 상품 1~4 | X `20, 250, 480, 710`, Y `146`, `190×174` |
| REROLL | `(38,462)`, `190×48` |
| CONTINUE | `(382,462)`, `210×48` |
| 총알 주머니 | `(776,384)`, `180×150` |

- 상품 행은 기존보다 아래로 내려 상단 상태 UI와의 간격을 확보한다.
- 주머니 뒤에 사각 패널, 마스크, 배경을 만들지 않는다.
- 주머니는 상품 카드와 겹치지 않는다.
- 구매 모션은 주머니 → 상점 주인 방향으로 0.50초 이동, 540° 회전, 충돌 시 비용 차감 표시로 연결한다.

## 2. 할리갈리 최근 카드

- 런타임: `UI/Halli_0_3_5/halli_reveal_history_rail_player_72x122_0_3_5.png`
- 런타임: `UI/Halli_0_3_5/halli_reveal_history_rail_ai_72x122_0_3_5.png`
- 각 필드 최근 3장을 세로 12px 간격으로 겹친다.
- 그리기 순서: 가장 오래된 카드 → 최신 카드.
- 판정 데이터는 최신 카드 1장만 사용한다. 이전 카드는 가독성 보조다.

## 3. 포커 아이템 상자

본 화면:

- `preview/poker_item_box_closed_preview_960x540_0_3_6.png`
- 총알 개수/총알 미터 없음
- AI 아이템 상자 없음
- 플레이어 닫힌 상자만 오른쪽에 표시

클릭 상태 전이:

1. 닫힌 상자 클릭
2. 뒤 화면 디밍 + 입력 차단
3. `inventoryCount == 0` → `poker_item_crate_open_empty_160x160_0_3_4.png`
4. `inventoryCount > 0` → `poker_item_crate_open_filled_160x160_0_3_4.png`와 4칸 인벤토리
5. X 버튼 또는 허용된 닫기 입력으로 본 화면 복귀

상자 원본:

- `UI/Poker_0_3_4/poker_item_crate_closed_160x160_0_3_4.png`
- `UI/Poker_0_3_4/poker_item_crate_open_empty_160x160_0_3_4.png`
- `UI/Poker_0_3_4/poker_item_crate_open_filled_160x160_0_3_4.png`

팝업 프레임:

- `UI/Poker_0_3_6/poker_item_popup_frame_560x300_0_3_6.png`
- 기준 위치 `(200,120)`, 크기 `560×300`
- 열림 중 본 화면 알파 디밍 기준 `168/255`

## 4. 공용 아이템

| ItemId | 에셋 |
|---|---|
| `IT-01` | `UI/Gameplay_0_1_2/item_reload_64_0_1_2.png` |
| `IT-02` | `UI/Gameplay_0_1_2/item_bottom_deal_64_0_1_2.png` |
| `IT-03` | `UI/Gameplay_0_1_2/item_hype_man_64_0_1_2.png` |
| `HP-01` | `UI/Gameplay_0_1_2/item_heal_tonic_64_0_1_2.png` |

인벤토리 용량은 4칸이며 `idle / hover / selected / disabled` 상태를 사용한다.

## 5. 살룬 조명

- 테이블 Spot Light 강도: 기존 값 `×1.20`
- 살룬 광역 보조광과 좌/우 실내광을 추가한다.
- 배경은 실루엣이 아니라 병·선반·액자·계단·의자·바닥 결을 알아볼 수 있어야 한다.
- UI Canvas와 카드 스프라이트에는 월드 조명 보정을 중복 적용하지 않는다.
- 상세 값: `lighting_profile_0_3_6.json`, `lighting_art_handoff_0_3_6.md`

## 6. Unity 임포트

- Texture Type: `Sprite (2D and UI)`
- Mesh Type: `Full Rect`
- Filter Mode: `Point`
- Compression: `None`
- Alpha Is Transparency: `On`
- Pivot: 기본 `(0.5,0.5)`, 상자 `(0.5,0.0)` 권장
