# 포커 아이템 상자 0.3.4 프로그래머 인계

기존 단순 선형 상자를 서부 살룬의 낡은 목재 운반 상자로 교체한다. 세 상태는 같은 상자의 연속 상태다.

- `poker_item_crate_closed_160x160_0_3_4.png`
- `poker_item_crate_open_empty_160x160_0_3_4.png`
- `poker_item_crate_open_filled_160x160_0_3_4.png`

닫힘 → 열림(비어 있음) → 열림(아이템 표시) 순으로 사용한다. Pivot은 `(0.5, 0.0)`, Filter Mode는 Point, Compression은 None을 권장한다. 플레이어·AI 상자는 같은 에셋을 사용하고 조명/위치만 구분한다.

기준 시안은 `preview/poker_item_crate_application_preview_960x540_0_3_4.png`이다.

