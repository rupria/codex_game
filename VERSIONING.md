# 빌드 버전 및 리비전 정책

이 문서는 `codex_game`의 공식 빌드 식별·보존·추적 기준이다.

## 제품 버전

- 형식: `Major.Minor.Patch`
- 허용 정규식: `^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$`
- 각 값은 0 이상의 정수이며 `01`, `002`처럼 앞에 0을 붙이지 않는다.
- Major 증가 시 Minor와 Patch를 `0`으로, Minor 증가 시 Patch를 `0`으로 초기화한다.
- Git 태그는 `vMajor.Minor.Patch`로 만든다.

## 공식 배포 불변성

- 특정 버전으로 공식 배포한 폴더와 산출물은 절대 덮어쓰거나 수정하지 않는다.
- 변경이 하나라도 있으면 영향에 맞춰 새 제품 버전을 발행한다.
- 리비전이나 해시만 바꿔 동일한 제품 버전을 다시 배포하지 않는다.
- 공식 배포 폴더는 `G:\내 드라이브\codex_game\multi_pc\versioned_builds\vMajor.Minor.Patch`에 보존한다.

## 추적 식별자

| 항목 | 기준 | 용도 |
|---|---|---|
| `ProductVersion` | `Major.Minor.Patch` | 제품 버전 |
| `GitCommit` | 전체 Git commit SHA | 정확한 코드 상태 |
| `GitShortCommit` | SHA 앞 12자 | 간단한 화면 표시 |
| `SpecRevision` | 명세 목록의 SHA-256 | 적용한 Drive 명세 묶음 |
| `ArtifactSHA256` | 빌드 파일의 SHA-256 | 산출물 동일성 검증 |

전체 해시가 원본 식별자이며 짧은 값은 표시용이다. 리비전은 추적 정보이고 제품 버전을 대신하지 않는다.

## 배포 절차

1. PM이 변경 영향에 따라 다음 제품 버전을 결정한다.
2. 프로그래머가 깨끗한 Git 작업 트리의 대상 커밋에서 웹 빌드를 만든다.
3. `tools/New-VersionedBuild.ps1`로 산출물과 명세 스냅샷을 불변 배포 폴더에 패키징한다.
4. 생성된 `RELEASE_MANIFEST.json` 식별자를 QA에 전달한다.
5. QA가 동일 빌드와 공개 URL을 검증한다.
6. 승인 후 같은 번호의 Git 태그와 `pub` 통합을 진행한다. 사용자 승인 없이 `pub`을 변경하지 않는다.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\New-VersionedBuild.ps1 `
  -Version 0.1.0 `
  -ArtifactPath .\build\web
```

도구는 Git 태그, `pub` 병합 또는 외부 배포를 자동 실행하지 않는다.

## 현재 상태

- 공식 제품 버전: `UNRELEASED`
- 첫 공개 후보 권장값: `0.1.0`
- 실제 발행은 PM·사용자 결정과 QA 검증 후 수행한다.

## 참고 기준

- Semantic Versioning 2.0.0: <https://semver.org/>
- Git revision 확인: <https://git-scm.com/docs/git-rev-parse>
- 사용자 제공 참고글: <https://itprogramming119.tistory.com/entry/IT-%EC%83%81%EC%8B%9D-%EB%B2%84%EC%A0%84-%ED%91%9C%EA%B8%B0%EB%B2%95-100>
