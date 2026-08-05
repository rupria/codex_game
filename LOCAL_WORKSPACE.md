# 로컬 작업환경

## 기준 위치

| 목적 | 기준 위치 |
|---|---|
| Git 저장소, 코드, 빌드와 공유 산출물 | `C:\codes\codex_game` |
| Obsidian 규칙, 내부 메모와 증거 | `G:\내 드라이브\codex_game\obsidian` |
| C 저장소에서 보이는 Obsidian 연결 | `C:\codes\codex_game\obsidian` |
| GitHub 원격 | `https://github.com/rupria/codex_game.git` |
| 완료본 Drive 공유 | `G:\내 드라이브\codex_game\shared` |

`C:\codes\codex_game\obsidian`은 Google Drive의 Obsidian Vault를 가리키는 로컬 디렉터리 연결이다. Git에서 `/obsidian/`과 `/.obsidian/`을 제외하므로 내부 메모와 Obsidian 설정은 커밋되지 않는다.

## 시작 절차

1. `C:\codes\codex_game`을 Codex 프로젝트와 Obsidian Vault로 연다.
2. `obsidian\rules`에서 공통 규칙과 자신의 역할 규칙을 읽는다.
3. Git 명령은 반드시 `C:\codes\codex_game`에서 실행한다.
4. `git status --short --branch`로 기존 변경을 확인한다.
5. 내부 메모는 `obsidian\core\<역할>`에, Git 공유 산출물은 저장소의 역할 폴더에 작성한다.

Obsidian 자료만 볼 때는 `G:\내 드라이브\codex_game\obsidian`을 직접 Vault로 열어도 된다. 통합 뷰가 필요하면 C 저장소 루트를 연다.

## 금지 사항

- `G:\내 드라이브\codex_game` 최상위의 예전 `.git`을 개발 저장소로 사용하지 않는다.
- G의 `.git\index.lock`이나 인덱스를 임의로 삭제·복구하지 않는다.
- C와 G의 동일한 Git 공유 문서를 각각 독립적으로 수정하지 않는다.
- 사용자 승인 없이 `pub`에 커밋, 푸시 또는 병합하지 않는다.

## 연결 복구

다른 PC이거나 연결 폴더가 없으면 저장소 루트에서 다음을 실행한다.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\setup-local-workspace.ps1
```

스크립트는 기존 경로를 덮어쓰지 않으며, 대상이 이미 올바른 디렉터리 연결이면 그대로 종료한다.

## 완료본 공유

완료된 작업은 `dev`에 커밋하고 GitHub에 푸시한 뒤 다음 명령으로 Drive에 발행한다.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\publish-completed-to-drive.ps1
```

발행 스크립트는 작업 트리가 깨끗할 때만 실행되며 다음을 만든다.

- 커밋 기준 소스 ZIP
- 전체 Git 이력이 포함된 bundle
- 브랜치, 커밋 SHA, 체크섬과 파일 수를 기록한 매니페스트
- 최신 릴리스 이름을 가리키는 `LATEST.txt`

노트북에서는 GitHub 또는 bundle을 노트북 로컬 디스크에 clone한다. Google Drive 안에 활성 `.git` 작업트리를 만들지 않는다.
