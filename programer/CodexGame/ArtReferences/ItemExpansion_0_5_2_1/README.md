# ItemExpansion 0.5.2.1 보완 검수 패키지

- 기준 명세: 게임진행플로우 0.1.2.5 / `2026-08-17-02`
- 요청서 검토 기준: `dev` `441b082`
- 실제 작업 기준: `dev` `964cdd2`
- 기존 런타임 아트: `ItemExpansion_0_5_2` 재사용
- 제작·조립·검수 도구: Aseprite 1.3.18.2
- 신규 런타임 PNG: 없음

기존 0.5.2 자산을 덮어쓰지 않는다. 보험의 아이템 사용과 실제 보정 발동을 서로 다른 배치·크기·타이밍으로 연결하기 위한 시각 기준과 신규 아이템 4종 경계 상태 검수본만 추가한다.

## 포함 파일

- `source/insurance_activation_review_4f_0_5_2_1.aseprite`
- `source/generate_item_expansion_review_0_5_2_1.lua`
- `preview/insurance_activation_review_{960x540,1280x720,1920x1080}_0_5_2_1.png`
- `preview/insurance_activation_timing_4step_960x180_0_5_2_1.png`
- `preview/item_boundary_review_{960x540,1280x720,1920x1080}_0_5_2_1.png`
- `QA_REPORT.md`

프리뷰는 이벤트 연결 전 아트 안전 영역 검수본이다. 실제 Unity 빌드 캡처와 이벤트 조건 검증은 프로그래머 연결 뒤 진행한다.
