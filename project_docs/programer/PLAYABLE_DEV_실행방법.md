# Playable Dev 실행 방법

## Unity Editor

1. GitHub의 `dev` 브랜치를 최신화합니다.
2. Unity `6000.3.18f1`로 이 폴더를 프로젝트로 엽니다.
3. `Assets/Scenes/PlayableDev.unity`를 열고 Play를 누릅니다.

조작은 `Enter`/`Space`로 시작, `↑`/`Space`로 플립 또는 계속, `←`/`→`로 해당 더미의 벨 판정, `R`로 재시작입니다.

## WebGL 개발 빌드

Unity 메뉴에서 `Codex Game > Playable Dev > Build WebGL Development`를 실행합니다. 기본 출력은 `Builds/WebGLDev`이며 Git에는 포함되지 않습니다.

로컬 브라우저 테스트 예시:

```powershell
py -m http.server 8765 --bind 127.0.0.1 --directory Builds/WebGLDev
```

그다음 <http://127.0.0.1:8765/>로 접속합니다. 공개 HTTPS 배포는 별도 승인 후 `pub` 기준으로 진행합니다.

## 현재 아트 적용 범위

- 적용: `Assets/Art/Prototype/Board/board_layout_wip.png`
- 미적용: 카드·버튼·결과 연출 리소스. 현재 플레이 화면에서는 코드 플레이스홀더를 사용합니다.
