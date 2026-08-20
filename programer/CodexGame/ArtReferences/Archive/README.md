# Runtime art archive

이 폴더는 실행 경로에서 제거된 구형 아트의 이력 보관 전용이다. Unity `Assets/`에서 읽거나 새 화면에 다시 연결하지 않는다.

최신 실행 자산의 경로와 교체 대상은 다음 두 파일을 따른다.

- `ArtReferences/RuntimeBindingTools/current_art_runtime_manifest_0_6_1.json`
- `project_docs/programer/26.08.20_아트실행연결_잔여작업_통합표_0.6.1.md`

`LegacyRuntime/`에는 코드·씬 참조 0건을 확인한 뒤 실행 경로에서 제거한 구형 묶음만 둔다. 저장 장면이 아직 GUID를 보유한 구형 묶음은 최신 바인딩과 `PlayableDev.unity` 재생성이 완료된 같은 프로그래머 커밋에서 이곳으로 이동한다.
