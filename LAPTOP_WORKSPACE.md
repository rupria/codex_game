# 노트북 서브 작업환경

## 권장 방식: GitHub에서 로컬 clone

노트북의 Google Drive 동기화가 끝난 뒤, 코드는 노트북 로컬 디스크에 복제한다.

```powershell
New-Item -ItemType Directory -Force -Path 'C:\codes' | Out-Null
git clone --branch dev https://github.com/rupria/codex_game.git C:\codes\codex_game
powershell -ExecutionPolicy Bypass -File C:\codes\codex_game\tools\setup-local-workspace.ps1
```

Google Drive의 드라이브 문자나 Vault 위치가 다르면 명시한다.

```powershell
powershell -ExecutionPolicy Bypass -File C:\codes\codex_game\tools\setup-local-workspace.ps1 `
  -RepositoryPath C:\codes\codex_game `
  -ObsidianVaultPath 'H:\내 드라이브\codex_game\obsidian'
```

## 오프라인 방식: Drive의 Git bundle 사용

`G:\내 드라이브\codex_game\shared\LATEST.txt`에서 최신 릴리스 폴더명을 확인한다.

```powershell
git clone 'G:\내 드라이브\codex_game\shared\releases\<릴리스>\codex_game.bundle' C:\codes\codex_game
git -C C:\codes\codex_game remote set-url origin https://github.com/rupria/codex_game.git
git -C C:\codes\codex_game switch dev
powershell -ExecutionPolicy Bypass -File C:\codes\codex_game\tools\setup-local-workspace.ps1
```

온라인이 되면 작업 전에 다음을 실행한다.

```powershell
git -C C:\codes\codex_game fetch origin
git -C C:\codes\codex_game pull --ff-only origin dev
```

## 서브 작업 규칙

- 작업 시작 전에 `AGENTS.md`, `obsidian\rules\README.md`와 역할 규칙을 읽는다.
- 노트북에서도 GitHub fetch, pull, commit과 push를 수행할 수 있다.
- 코딩은 기능별 브랜치에서 진행하고 완료한 변경만 커밋·푸시한다.
- 다른 PC에서 작업 중인 변경이 있는지 먼저 확인한다.
- 같은 브랜치의 미푸시 변경을 데스크톱과 노트북에 동시에 남기지 않는다.
- `pub`은 사용자 승인 없이 변경하지 않는다.
- Google Drive의 `shared\releases`는 배포된 완료본이므로 그 안에서 직접 수정하지 않는다.
- 내부 메모는 Google Drive의 `obsidian\core`에 작성할 수 있지만 동기화 완료를 확인한 뒤 편집한다.

## 노트북 푸시 예시

```powershell
git -C C:\codes\codex_game fetch origin
git -C C:\codes\codex_game switch dev
git -C C:\codes\codex_game pull --ff-only origin dev
git -C C:\codes\codex_game switch -c codex/laptop-작업명
# 작업·검증·커밋 후
git -C C:\codes\codex_game push -u origin codex/laptop-작업명
```

데스크톱에서 이어갈 때는 먼저 `git fetch origin`을 실행하고 동일한 원격 브랜치를 추적한다. `dev` 직접 푸시가 필요한 경우에도 원격 최신 상태를 먼저 가져오고 fast-forward 가능한 상태인지 확인한다.
