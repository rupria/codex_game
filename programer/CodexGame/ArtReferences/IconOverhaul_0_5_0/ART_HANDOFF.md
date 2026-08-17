# 아이콘 전면 개선 0.5.0 — 아트 인계

- 기준: `origin/dev` 커밋 `00b389c36610b1669b1f2415cbb81229b6df787c`
- 상태: `dev` 공유 아트, 게임 연결 전
- 제작 도구: Aseprite
- 런타임 리소스: `Assets/Art/Prototype/UI/IconOverhaul_0_5_0/`
- 편집 원본: `ArtReferences/IconOverhaul_0_5_0/source/*.aseprite`
- 파일별 연결 키와 규격: `icon_overhaul_art_catalog_0_5_0.json`

## 포함 범위

- 정식 아이템 4종
- 기본·임시 재화와 상점 경고
- 플레이어·AI HP 3상태
- 플레이어·AI 라운드 승수
- 포커 예측 결과와 초상 선택 상태
- 커뮤니티 자물쇠 3상태
- AI 판단 중 리볼버 실린더 8프레임
- 3회 호출·쇼다운 단계 표식
- 아이템 횟수·소진·제한 표식
- 가이드 이전·다음·닫기 단일 아이콘

## 연결 주의사항

상점의 `dummy_01~06`을 새 아이템 여섯 개로 취급하지 않습니다. 정식 키는
`bar_shop.item.reload`, `bar_shop.item.bottom_deal`,
`bar_shop.item.hype_man`, `bar_shop.item.health_recovery` 네 개입니다.

기존 승인 리소스인 황동 벨, 총알 주머니, 밧줄·폭발, 포커 상자,
카드·조커, 상대 초상화와 스테이지 로딩은 교체하지 않습니다.
