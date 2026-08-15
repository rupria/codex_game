# 상점 총알 주머니 UI — 느슨한 탄환 승인본

연결 이슈: [#36](https://github.com/rupria/codex_game/issues/36)

## 최종 확정

상점·인벤토리의 총알 주머니 아이콘은 승인 아트 세트 `ammo_pouch_loose_rounds_0_1_0`을 사용합니다.

- 자산 키: `item.ammo_pouch.loose_rounds`
- Unity 런타임: `Assets/Art/Prototype/UI/BarShop_0_3_8/ammo_pouch_loose_rounds_64_0_1_0.png`
- 승인 패키지: `outputs/art/ammo_pouch_loose_rounds_0_1_0/`
- 표시 크기: 64×64 px
- 표시 내용: 열린 부드러운 가죽 주머니와 자유롭게 놓인 총알 5발
- 배경: 투명

기존 `bar_shop_ammo_pouch_static_5_180x150_0_3_8.png`와 `bar_shop_ammo_pouch_pile_5_180x150_0_3_8.png`는 과거안 보관용이며 신규 연결에 사용하지 않습니다.

## 연결 금지

아이콘은 장식용 정적 이미지 한 장입니다.

- `BulletCount`에 따른 총알 추가·제거 금지
- 개별 총알 자식 오브젝트 금지
- 손·던지기·튕기기·붓기 애니메이션 금지
- 빈 주머니 또는 구매 전후 상태 전환 금지
- 사각 프레임·탄창 슬롯·개별 고정 루프 추가 금지

## 승인 잠금

`outputs/art/ammo_pouch_loose_rounds_0_1_0/APPROVED.sha256` 검증을 통과한 파일만 승인본입니다. 수정이 필요하면 기존 파일을 덮어쓰지 않고 `0_1_1` 이상의 새 아트 세트를 만듭니다.

## Unity 설정

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Alpha Is Transparency: 켬
- Filter Mode: `Point`
- Compression: `None`
- Generate Mip Maps: 끔
- Preserve Aspect: 켬

## QA

- [ ] 승인된 64×64 아이콘 한 장만 표시된다.
- [ ] 총알 5발이 모두 자연스럽게 놓여 보인다.
- [ ] 주머니 내부에 고정 슬롯이나 사각 프레임이 없다.
- [ ] 구매 전후 이미지가 바뀌지 않는다.
- [ ] 손·개별 총알·투척·붓기 오브젝트가 나타나지 않는다.
- [ ] 재화 계산과 구매 기능은 아트 표시와 독립적으로 정상 동작한다.
