# codex_game

OpenAI Game 2026 출품작의 코드 저장소입니다.

## 작업공간

- 로컬 Git·코드·테스트·빌드: `C:\sk-encoa\codex_game`
- Drive 문서·규칙·기획·QA·PM: `G:\내 드라이브\codex_game\obsidian`
- 다른 PC 설치·점검 자료: `G:\내 드라이브\codex_game\multi_pc`
- 원격 저장소: <https://github.com/rupria/codex_game>

Git에는 실행 가능한 게임 코드와 빌드·배포 설정만 둡니다. 기획서, 회의·검토 메모, QA 기록, PM 일정과 제출 자료는 Drive에서 관리합니다.

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

현재 저장소에는 게임 엔진 프로젝트가 아직 없습니다. 구현 시작 시 게임 코드는 `programer/` 아래에 둡니다.
