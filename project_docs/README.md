# 프로젝트 공유 문서 스냅샷

이 폴더는 Google Drive·Obsidian의 기준 문서를 Git에서 역할 간 공유하고 버전을 고정하기 위한 읽기 전용 스냅샷입니다.

- 공통 역할 리비전: `2026-08-06-03`
- 제품 버전: `UNRELEASED`
- 기준 명세: `게임진행플로우_0.03`
- 기준 원본: `G:\내 드라이브\codex_game\obsidian`
- Git 대상 브랜치: `dev`
- 운영 원칙: Git 공유본을 독립적으로 편집하지 않고 Drive 원본을 수정한 뒤 새 리비전으로 다시 동기화합니다.

## 역할별 최신 공유본

- 전체·총괄: [CURRENT_SYNC.md](CURRENT_SYNC.md), [SYNC_MANIFEST.json](SYNC_MANIFEST.json)
- 기획: [현재 기준](designer/00_기획_현재기준.md), [0.01](designer/게임진행플로우_0.01.md), [0.02](designer/게임진행플로우_0.02.md), [0.03](designer/게임진행플로우_0.03.md)
- 프로그래머: [Unity C# 작업 세팅 결과](programer/26.08.06_PM공유_게임진행플로우_0.03_Unity_CSharp_작업세팅_결과.md), [실제 코드](../programer/)
- QA: [QA_MASTER.md](QA/QA_MASTER.md)
- PM: [PM_MASTER.md](PM/PM_MASTER.md)
- 공통 규칙: [rules/README.md](rules/README.md)

## 동기화 검증

`SYNC_MANIFEST.json`에는 각 Drive 원본과 Git 스냅샷의 SHA-256이 기록됩니다. `matchesSource: true`는 생성 당시 두 파일의 내용이 같았음을 의미합니다.

0.01 PDF·XMind와 기획 분석 CSV도 역사·근거 보존을 위해 포함합니다. Google Sheets 바로가기는 Drive 보존본에만 유지합니다.

`pub`는 빌드 통합 및 사용자 승인 전에는 변경하지 않습니다.
