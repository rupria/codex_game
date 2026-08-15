# 탄약 주머니 아이템 아트 인계 0.1.0

## 승인 상태

- 상태: **사용자 승인 완료**
- 승인일: 2026-08-15
- 자산 키: `item.ammo_pouch.loose_rounds`
- 핵심 표현: 고정 슬롯이나 칸막이가 없는 부드러운 가죽 주머니 안에 탄환 5발이 자유롭게 흩어져 있음

## 프로그래머 적용 파일

| 용도 | 파일 |
|---|---|
| 인벤토리·상점 64px 아이콘 | `final/ammo_pouch_loose_rounds_64_0_1_0.png` |
| 큰 UI 표시 | `final/ammo_pouch_loose_rounds_256_0_1_0.png` |
| 투명 마스터 | `final/ammo_pouch_loose_rounds_1254_0_1_0.png` |
| 승인 원본 보관 | `source/ammo_pouch_loose_rounds_1254_source_magenta_0_1_0.png` |

## Unity 권장 임포트

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Alpha Is Transparency: 켬
- Filter Mode: `Point`
- Compression: `None`
- Generate Mip Maps: 끔
- 비율을 유지하고 임의 크롭·회전·색상 보정은 하지 않음

## 프로그래머 연결 참고

읽기 전용 Unity 참조 스냅샷에서는 기존 탄약 주머니가 다음 위치와 연결되어 있었다. 현재 작업에서는 이 파일들을 수정하지 않았다.

- 기존 런타임 아트 폴더: `programer/CodexGame/Assets/Art/Prototype/UI/BarShop_0_3_8/`
- 기존 교체 대상명: `bar_shop_ammo_pouch_static_5_180x150_0_3_8.png`
- 연결 후보: `BarShopUiArtSet.cs`, `PlayableDevSceneBuilder.cs`, `BarShopDevPanel.cs`, `PlayableDev.unity`
- 구현 확인점: `DrawAmmoPouch`의 수량별 개별 탄환 배치가 새 정적 스프라이트 위에 중복 렌더링되지 않도록 한다.
- 기존 PNG와 `.meta`는 덮어쓰거나 GUID를 재사용하지 않는다. 새 파일명으로 Unity가 고유 `.meta`를 생성하게 한다.

## 변경 방지 규칙

1. 이 폴더의 `0_1_0` 승인 파일은 제자리에서 수정하거나 같은 이름으로 덮어쓰지 않는다.
2. 코드에서는 파일명을 직접 하드코딩하지 말고 자산 키 `item.ammo_pouch.loose_rounds`를 카탈로그 또는 ArtSet에 연결한다.
3. 수정이 필요하면 `ammo_pouch_loose_rounds_0_1_1`처럼 새 버전 폴더와 새 파일을 만든다.
4. 반드시 다음 시각 조건을 유지한다: 탄환 정확히 5발, 모든 탄환은 느슨하게 놓임, 고정 슬롯·개별 루프·칸막이 없음.
5. `APPROVED.sha256` 검증이 실패한 파일은 승인본으로 취급하지 않는다.

## 작업 범위 보호

이번 인계는 `outputs/art/ammo_pouch_loose_rounds_0_1_0/` 새 폴더만 추가한다. 기존 프로그래머 코드, 기존 아트 세트, 빌드 파일 및 카탈로그는 수정하지 않았다.
