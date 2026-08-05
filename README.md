# codex_game

OpenAI Game Builders Seoul Track 1 참가작의 코드 저장소입니다.

## 저장소 역할

이 저장소는 실행 가능한 게임 코드, 자동화 테스트, 빌드·배포 설정, GitHub Issue와 Pull Request 기록의 기준입니다.

게임 기획, 총괄 현황, 의사결정과 장기 문서는 Google Drive에 동기화되는 별도 Obsidian 문서 작업공간에서 관리합니다. 로컬 개발과 모든 Git 명령은 C 드라이브 저장소에서 수행합니다.

- 문서 작업공간: `G:\내 드라이브\codex_game\obsidian`
- 로컬 코드 저장소: `C:\codes\codex_game`
- 통합 Obsidian 보기: `C:\codes\codex_game`을 Vault로 열면 Git 문서와 `obsidian` 연결 폴더의 Google Drive 자료를 함께 볼 수 있습니다.
- GitHub: https://github.com/rupria/codex_game
- 협업 규칙: [core/TEAM_WORKFLOW.md](core/TEAM_WORKFLOW.md)
- 로컬 환경 안내: [LOCAL_WORKSPACE.md](LOCAL_WORKSPACE.md)
- 노트북 작업 안내: [LAPTOP_WORKSPACE.md](LAPTOP_WORKSPACE.md)

`G:\내 드라이브\codex_game` 최상위의 예전 Git 사본은 참고·Obsidian 자료 보존용이며 개발 저장소로 사용하지 않습니다. 그 위치에서 `git add`, `commit`, `switch`, `reset` 또는 잠금 파일 삭제를 수행하지 않습니다.

완료된 `dev` 커밋은 GitHub에 푸시하고 `tools\publish-completed-to-drive.ps1`로 Google Drive에 ZIP과 Git bundle을 발행합니다. 노트북 코딩은 Drive 내부가 아니라 노트북의 로컬 Git clone에서 진행합니다.

## 프로젝트 상태

- 단계: 프로젝트 기반 구성
- 게임 아이디어: 미정
- 기술 스택: 미정
- 실행 가능한 빌드: 없음

## 핵심 원칙

1. 코어 플레이 구조 자체가 재미있어야 한다.
2. 브라우저에서 외부 공개와 플레이에 제약이 없어야 한다.
3. 개발 과정에서 Codex를 실질적으로 활용하고 그 과정을 기록한다.

구현 구조와 실행 방법은 기술 스택 확정 후 갱신합니다.
