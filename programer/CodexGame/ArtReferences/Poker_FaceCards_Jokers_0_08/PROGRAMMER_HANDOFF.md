# 프로그래머 인계 — 포커 J/Q/K + Joker 0.08

## 적용 대상

`programer/CodexGame/Assets/Art/Prototype/Cards_0_08/`

### J/Q/K 파일 규칙

`card_poker_{suit}_{rank}.png`

- suit: `clubs`, `diamonds`, `hearts`, `spades`
- rank: `j`, `q`, `k`

총 12장입니다.

### Joker 파일

- `card_poker_joker_brass_sheriff_revolver.png`
- `card_poker_joker_crimson_cardsharp.png`

## 구현 체크

- 기존 J/Q/K Sprite 매핑을 0.08 파일명에 연결합니다.
- 검정 문양은 Clubs/Spades, 붉은 문양은 Diamonds/Hearts로 유지합니다.
- Joker Sheriff는 반드시 `revolver` 파일을 사용합니다. 종 버전은 사용하지 않습니다.
- 포커 카드 확대·커뮤니티 공개 연출에는 `poker_face_cards_hires`를 사용합니다.
- 작은 획득 카드 UI에는 우선 `112x156`, 기존 고정 슬롯 때문에 불가할 때만 `56x78`을 사용합니다.
- Joker의 덱 포함 여부·수량·효과는 이 아트 패키지에서 정하지 않습니다.

## 아트 QA

- 4문양 × J/Q/K = 12장 확인
- Joker 2장 확인
- 카드 모서리·테두리 크롭 확인
- 리볼버 조커에 종 잔존 없음
- 112×156, 56×78 호환본 생성 확인
