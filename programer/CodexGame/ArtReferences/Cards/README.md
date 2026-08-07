# 카드 비주얼 방향 — 승인 WIP

- 승인일: 2026-08-07
- 승인 레퍼런스: `card_visual_direction_approved_wip.png`
- 런타임 미리보기: `card_runtime_preview_wip.png`
- 클럽 보완 원본: `card_club_direction_imagegen_wip.png`
- 적용 범위: 4문양 × 13랭크 × 해골 수 3단계 = 카드 전면 156종 전체
- 카드 뒷면: 레퍼런스의 네이비·시안·골드 해골 벨 문양을 공통 뒷면으로 사용
- 실사용 전면: `Assets/Art/Prototype/Cards/deck_variants/`
- 실사용 뒷면: `Assets/Art/Prototype/Cards/components/card_back.png`
- 개발 조립용 컴포넌트: `Assets/Art/Prototype/Cards/components/`
- 런타임 카탈로그: `Assets/Art/Prototype/Cards/card_art_catalog.json`

첨부 레퍼런스에서 스페이드·하트·다이아몬드·뒷면을 추출하고, 레퍼런스에 없던 클럽 전면은 OpenAI 내장 ImageGen으로 같은 방향에 맞춰 보완했다. 외부 카드 팩은 사용하지 않았다. 레퍼런스 파일은 Unity `Assets` 밖에 두어 빌드에는 포함하지 않는다.

현재 64×90 실사용 카드는 레퍼런스의 크림색 전면, 검정 픽셀 테두리, 문양별 해골 엠블럼, 네이비·시안·골드 뒷면을 직접 반영한다. 전면 156종과 조립용 컴포넌트 22종을 개발에서 바로 사용할 수 있다.

현재 `PlayableDevView` 화면은 카드 아트를 읽지 않는 코드 플레이스홀더 상태다. 게임 화면에 표시하려면 프로그래밍 작업에서 위 카탈로그 또는 카드 PNG를 연결해야 한다.
