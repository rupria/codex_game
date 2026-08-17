# 아이템 4종 아트 인계 0.5.2

- 적용 명세: `게임진행플로우 0.1.2.5`
- 기획 리비전: `2026-08-17-02`
- 상태: 사용자 승인 완료, `dev` 공유 대상, 프로그래머 런타임 연결 대기
- 제작 도구: Aseprite
- 외부 이미지/AI 생성 래스터 사용: 없음

## 포함 범위

1. IT-04 와일드 잉크: 80px 팝업, 64px 4상태, 0.65초 잉크 확산, 문양 인장 4종, 적용/교환 잠금 마커
2. IT-05 술통: 80px 팝업, 64px 4상태, 방어 준비, 충돌·파손 8프레임, HP 보존 마커
3. IT-06 예측 보험: 80px 팝업, 64px 4상태, 인장 적용 6프레임, 충전 2/1/0, 실제 성공/보험 성공 분리 배지
4. IT-07 용병단: 80px 팝업, 64px 4상태, 양측 동시 교환 10프레임, 플레이어 대상/AI 비공개 마커

## 연결 원칙

- 아이콘 자체에는 사각 배경과 번역 문자열이 없다.
- 상태는 색만으로 구분하지 않는다. 선택은 체크·프레임, 비활성은 철제 사선·자물쇠가 함께 보인다.
- 와일드 잉크 인장은 카드 좌상단 랭크와 우상단 문양을 피한 카드 중앙 하단 안전 영역에 둔다.
- 용병단 AI 카드는 교환 연출 중 끝까지 뒷면이다.
- 보험 결과 화면에서 실제 성공과 보험 성공 아이콘을 합치지 않는다.
- 연출 종료와 효과 적용은 프로그래머 이벤트 한 번에 연결하되, 2분 공용 타이머는 연출 중에도 계속 흐른다.

## Unity 권장 Import

- Texture Type: Sprite (2D and UI)
- Filter Mode: Point
- Compression: None
- Mip Maps: Off
- Alpha Is Transparency: On
- Pixels Per Unit: 1

## 검증본

- `preview/item_expansion_full_contact_sheet_1280x720_0_5_2.png`
- `preview/item_expansion_application_preview_960x540_0_5_2.png`
- `preview/item_expansion_safearea_preview_1280x720_0_5_2.png`
- `preview/item_expansion_safearea_preview_1920x1080_0_5_2.png`
- `preview/item_expansion_card_safezone_preview_960x540_0_5_2.png`

## 작업 경계

이 묶음은 아트 산출물이다. `GameItemId`, 상점 데이터, 효과 처리, 타이머, 조커·패널티 상호작용, 런타임 바인딩은 프로그래머 작업으로 남긴다.
