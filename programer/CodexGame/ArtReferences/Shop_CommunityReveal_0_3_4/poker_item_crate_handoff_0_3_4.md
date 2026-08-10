# 포커 아이템 상자 0.3.4 프로그래머 인계

기존 단순 선형 상자를 서부 살룬의 낡은 목재 운반 상자로 교체한다. 세 상태는 같은 상자의 연속 상태다.

- `poker_item_crate_closed_160x160_0_3_4.png`
- `poker_item_crate_open_empty_160x160_0_3_4.png`
- `poker_item_crate_open_filled_160x160_0_3_4.png`

닫힘 → 열림(비어 있음) → 열림(아이템 표시) 순으로 사용한다. Pivot은 `(0.5, 0.0)`, Filter Mode는 Point, Compression은 None을 권장한다.

0.1.2 기준에서 AI는 아이템을 사용하지 않으므로 AI 상자는 표시하지 않는다. 플레이어 상자는 접힌 인벤토리 진입점으로 사용하고, 열림 상태에서는 `Gameplay_0_1_2/inventory_tray_4slot_388x92_0_1_2.png`로 전환한다.

기준 시안은 `preview/poker_item_crate_application_preview_960x540_0_3_4.png`이다.
