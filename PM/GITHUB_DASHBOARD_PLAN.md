# GitHub 대시보드 설계

- 프로젝트 이름: `OpenAI Game Builders Seoul — codex_game`
- 저장소: `rupria/codex_game`
- 관리 책임: PM·QA
- 목적: 개발 산출물을 직접 소유하지 않고 일정, 의존성, 검증과 위험을 한곳에서 추적

## 필드

| 필드 | 값 |
|---|---|
| Status | Backlog / Ready / In Progress / In Review / Validation / Blocked / Done |
| Workstream | Planning / Development / Art / Audio / Deployment / QA / Release / Documentation |
| Priority | P0 / P1 / P2 / P3 |
| Target | D-11 / D-8 / D-5 / D-4 / D-3 / Buffer |
| Owner | 실제 작업 담당자 |
| QA Result | Not Tested / Pass / Conditional / Fail |

## 권장 보기

1. `Execution Board` — Status 기준 전체 작업 보드
2. `PM Timeline` — Target 기준 일정과 의존성 관리
3. `QA Queue` — Status가 Validation인 항목
4. `Risks & Blockers` — Status가 Blocked이거나 Priority가 P0인 미완료 항목
5. `Release Gate` — Target이 D-5, D-4 또는 D-3인 항목

## 초기 대시보드 항목

| 제목 | Workstream | Priority | Target | 초기 상태 |
|---|---|---|---|---|
| 공식 제출 마감 일시 확인 | Planning | P0 | D-11 | Blocked |
| 게임 한 줄 설명과 핵심 루프 확정 | Planning | P0 | D-11 | In Progress |
| 기술 스택과 배포 방식 확정 | Development | P0 | D-11 | Backlog |
| 코어 루프 내부 빌드 전달 | Development | P0 | D-8 | Backlog |
| 공개 웹 배포 경로 구성 | Deployment | P0 | D-8 | Backlog |
| 코어 루프 완주 검증 | QA | P0 | D-8 | Backlog |
| 플레이 가능 후보 빌드 전달 | Development | P0 | D-5 | Backlog |
| 첫 30초 무설명 플레이 테스트 | QA | P0 | D-5 | Backlog |
| 3분 핵심 재미 플레이 테스트 | QA | P0 | D-5 | Backlog |
| 시크릿 창 및 지원 브라우저 검증 | QA | P0 | D-4 | Backlog |
| 에셋 라이선스 완전성 검증 | QA | P0 | D-4 | Backlog |
| Codex 협업 기록 완전성 검증 | QA | P1 | D-4 | Backlog |
| 데모 영상·발표 동선 전달 | Release | P0 | D-3 | Backlog |
| 최종 빌드 검증 및 동결 승인 | QA | P0 | D-3 | Backlog |
| 실제 제출 및 접수 확인 | Release | P0 | D-3 | Backlog |

## 운영 규칙

- 기획·개발·아트 작업에는 실제 담당자를 Owner로 지정한다.
- PM·QA는 담당 작업을 대신 수행하지 않고 완료 조건과 목표 시점을 관리한다.
- 개발 작업은 산출물 링크나 커밋이 있어야 `Validation`으로 이동한다.
- QA Result가 `Pass` 또는 승인된 `Conditional`이어야 `Done`으로 이동한다.
- P0 차단 요소는 발견 즉시 `Risks & Blockers` 보기에 표시한다.
- D-5 이후 신규 기능은 등록하지 않고 P0/P1 결함과 제출 작업만 다룬다.

## 생성 차단 요소

현재 GitHub 앱에서 저장소가 조회되지 않고 로컬 GitHub CLI가 로그아웃 상태다. 다음 중 하나가 완료되면 이 설계대로 프로젝트와 이슈를 생성한다.

1. Codex GitHub 앱에 `rupria/codex_game` 저장소 접근 권한 부여
2. 로컬에서 `gh auth login` 완료
