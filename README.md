# codex_game

OpenAI Game 2026 출품작의 코드 저장소입니다.

## 작업공간

- 로컬 Git·코드·테스트·빌드: `C:\sk-encoa\codex_game`
- Drive 문서·규칙·기획·QA·PM: `G:\내 드라이브\codex_game\obsidian`
- 다른 PC 설치·점검 자료: `G:\내 드라이브\codex_game\multi_pc`
- 원격 저장소: <https://github.com/rupria/codex_game>

Git에는 실행 가능한 게임 코드와 빌드·배포 설정을 두고, 기획서, 회의·검토 메모, QA 기록, PM 일정과 제출 자료는 Drive에서 관리하는 것을 기본으로 합니다.

사용자가 명시적으로 Git 공유를 요청했거나 공동 검토·이력 관리·자동화에 필요한 비코드 산출물은 별도 목적의 커밋으로 공유할 수 있습니다. Drive 기준 원본이 있으면 같은 문서를 두 위치에서 독립 수정하지 않습니다.

## 다른 PC 시작

Google Drive 동기화 후 다음 파일을 실행합니다.

```powershell
powershell -ExecutionPolicy Bypass -File "G:\내 드라이브\codex_game\multi_pc\SETUP_OTHER_PC.ps1"
```

Drive 문자가 다르면 스크립트의 `DriveWorkspacePath` 매개변수로 실제 경로를 지정합니다. Drive 최상위의 예전 `.git`은 사용하지 않습니다.

## 브랜치

- `dev`: 일반 개발과 검증 기준
- `pub`: 승인된 웹 빌드·배포 기준

사용자의 명시적 승인 없이 `pub`에 직접 커밋하거나 병합하지 않습니다.

## 빌드 버전

- 제품 버전은 선행 0이 없는 `Major.Minor.Patch` 형식을 사용합니다.
- 공식 배포 후 같은 버전의 내용은 수정하지 않으며 변경분은 새 버전으로 발행합니다.
- 전체 Git commit SHA, 명세 스냅샷 SHA-256과 산출물 SHA-256으로 빌드 근거를 추적합니다.
- 상세 기준과 패키징 명령은 [VERSIONING.md](VERSIONING.md)를 따릅니다.

## 제품 기준

- 설치 없이 브라우저에서 실행되는 웹 빌드
- 승인·초대 없이 접근 가능한 공개 HTTPS 플레이 링크
- 첫 화면에서 확인 가능한 목표·조작법·실행 방법
- 첫 30초 이해와 3분 코어 재미

현재 `programer/CodexGame`에 Unity 6.3 LTS·C#·URP 초기 프로젝트와 코어 규칙 코드가 있습니다. C# 스모크 테스트는 통과했으며 Unity 내부 컴파일과 WebGL 빌드는 아직 검증 전입니다.

## 로그인 없는 온라인 기록

programer/web에는 Cloudflare Pages Functions와 TiDB Cloud Starter를 연결하는 최소 API가 있습니다.
Unity WebGL은 같은 도메인의 /api만 호출하며, 로그인 화면 없이 서명된 익명 쿠키를 발급받습니다.
API 또는 DB가 실패해도 코어 플레이는 계속되고 전송하지 못한 경기 결과는 브라우저 로컬 큐에 보관합니다.

- 공개 파일: Unity WebGL 빌드
- 서버 API: Cloudflare Pages Functions
- 온라인 DB: TiDB Cloud Starter
- 비밀 설정: Cloudflare의 DATABASE_URL, GUEST_TOKEN_SECRET

비밀 값은 Git이나 Unity 프로젝트에 넣지 않습니다. DB 초기 테이블은
programer/web/sql/001_initial.sql을 사용합니다.

검증 상태 업데이트(2026-08-06): C# 스모크 테스트와 저장소 전용 Unity 6.3 LTS 배치 컴파일은 통과했으며 WebGL 빌드는 아직 검증 전입니다.
