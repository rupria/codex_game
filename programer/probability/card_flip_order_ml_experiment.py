"""좌우 카드 펼치기 순서가 게임 진행에 미치는 영향을 ML 방식으로 검증한다.

data-collection-workspace의 분류 분석 노트북에서 다음 실험 패턴을 가져왔다.

1. 고정 random_state로 재현 가능한 데이터를 만든다.
2. train_test_split으로 학습/평가 데이터를 분리한다.
3. LogisticRegression으로 두 펼치기 방식을 분류한다.
4. Accuracy, Precision, Recall, F1, Confusion Matrix를 CSV로 저장한다.

이 모델은 실제 플레이 승자를 예측하지 않는다. 두 펼치기 방식이 만들어 내는
진행 지표가 충분히 다르면 분류 정확도가 50% 기준선보다 높아지는 점을 이용해
순서 변경의 영향 유무를 확인한다.
"""

from __future__ import annotations

import argparse
import random
from dataclasses import dataclass
from pathlib import Path

import pandas as pd
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import (
    accuracy_score,
    confusion_matrix,
    f1_score,
    precision_score,
    recall_score,
)
from sklearn.model_selection import train_test_split
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler


RANKS = ("A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K")
SUITS = ("S", "C", "H", "D")
MODE_ALIGNED = "aligned_lr"
MODE_MIRRORED = "mirrored_rl"
FEATURE_COLUMNS = (
    "bell_opportunities",
    "first_bell_turn",
    "double_choice_count",
    "left_exact_three_count",
    "right_exact_three_count",
    "overflow_reset_count",
    "turns_played",
    "cards_revealed",
    "winner_reached_three",
)


@dataclass(frozen=True)
class Card:
    suit: str
    rank: str
    skull: int

    @property
    def name(self) -> str:
        return f"{self.suit}{self.rank}"


def build_standard_deck(seed: int) -> list[Card]:
    """표준 52장에 해골 1·2·3을 18·17·17장으로 고정 배정한다."""
    identities = [(suit, rank) for suit in SUITS for rank in RANKS]
    skull_values = [1] * 18 + [2] * 17 + [3] * 17
    skull_rng = random.Random(seed + 1)
    skull_rng.shuffle(skull_values)
    return [
        Card(suit=suit, rank=rank, skull=skull)
        for (suit, rank), skull in zip(identities, skull_values, strict=True)
    ]


def side_for_turn(turn: int) -> str:
    return "left" if turn % 2 == 1 else "right"


def simulate_trial(mode: str, trial_id: int, seed: int, base_deck: list[Card]) -> dict[str, int | str]:
    """26회 이내에서 한 게임의 할리갈리 단계를 시뮬레이션한다.

    가정:
    - 펼치기 1회마다 플레이어 카드와 AI 카드가 차례로 공개된다.
    - 두 카드 공개가 끝난 뒤 좌우 합계를 한 번 판정한다.
    - 합이 3을 초과한 더미만 즉시 초기화한다.
    - 합이 정확히 3인 더미가 있으면 벨 기회 1회로 처리하고 양쪽을 초기화한다.
    - 벨 승자는 순서 효과와 분리하기 위해 50:50으로 정한다.
    - 한쪽이 벨 3승을 달성하면 조기 종료하고, 아니면 최대 26회 진행한다.
    """
    if mode not in {MODE_ALIGNED, MODE_MIRRORED}:
        raise ValueError(f"지원하지 않는 mode: {mode}")

    rng = random.Random(seed + trial_id * 17 + (0 if mode == MODE_ALIGNED else 1_000_003))
    deck = base_deck.copy()
    rng.shuffle(deck)

    sums = {"left": 0, "right": 0}
    bell_opportunities = 0
    first_bell_turn = 27
    double_choice_count = 0
    exact_counts = {"left": 0, "right": 0}
    overflow_reset_count = 0
    wins = [0, 0]
    turns_played = 26

    for turn in range(1, 27):
        player_card = deck[(turn - 1) * 2]
        ai_card = deck[(turn - 1) * 2 + 1]
        player_side = side_for_turn(turn)

        if mode == MODE_ALIGNED:
            ai_side = "right" if player_side == "left" else "left"
        else:
            ai_side = player_side

        sums[player_side] += player_card.skull
        sums[ai_side] += ai_card.skull

        for side in ("left", "right"):
            if sums[side] > 3:
                sums[side] = 0
                overflow_reset_count += 1

        valid_sides = [side for side in ("left", "right") if sums[side] == 3]
        if not valid_sides:
            continue

        bell_opportunities += 1
        first_bell_turn = min(first_bell_turn, turn)
        for side in valid_sides:
            exact_counts[side] += 1
        if len(valid_sides) == 2:
            double_choice_count += 1

        winner = rng.randrange(2)
        wins[winner] += 1
        sums = {"left": 0, "right": 0}

        if wins[winner] == 3:
            turns_played = turn
            break

    return {
        "mode": mode,
        "label": int(mode == MODE_MIRRORED),
        "trial_id": trial_id,
        "bell_opportunities": bell_opportunities,
        "first_bell_turn": first_bell_turn,
        "double_choice_count": double_choice_count,
        "left_exact_three_count": exact_counts["left"],
        "right_exact_three_count": exact_counts["right"],
        "overflow_reset_count": overflow_reset_count,
        "turns_played": turns_played,
        "cards_revealed": turns_played * 2,
        "winner_reached_three": int(max(wins) >= 3),
    }


def build_sample_sequence(base_deck: list[Card], seed: int) -> pd.DataFrame:
    rng = random.Random(seed)
    deck = base_deck.copy()
    rng.shuffle(deck)
    rows: list[dict[str, int | str]] = []

    for turn in range(1, 27):
        player_card = deck[(turn - 1) * 2]
        ai_card = deck[(turn - 1) * 2 + 1]
        player_side = side_for_turn(turn)
        aligned_ai_side = "right" if player_side == "left" else "left"
        mirrored_ai_side = player_side
        rows.append(
            {
                "turn": turn,
                "player_card": player_card.name,
                "player_skull": player_card.skull,
                "ai_card": ai_card.name,
                "ai_skull": ai_card.skull,
                "aligned_player_side": player_side,
                "aligned_ai_side_screen": aligned_ai_side,
                "mirrored_player_side": player_side,
                "mirrored_ai_side_screen": mirrored_ai_side,
            }
        )
    return pd.DataFrame(rows)


def build_result_rows(data: pd.DataFrame, metrics: dict[str, float], matrix: list[list[int]], model: Pipeline) -> pd.DataFrame:
    rows: list[dict[str, str | float | int]] = []

    for mode, group in data.groupby("mode", sort=True):
        for feature in FEATURE_COLUMNS:
            rows.append(
                {
                    "section": "mode_summary",
                    "mode": mode,
                    "metric": f"mean_{feature}",
                    "value": float(group[feature].mean()),
                    "notes": "simulation mean",
                }
            )

    for metric, value in metrics.items():
        rows.append(
            {
                "section": "model_metric",
                "mode": "comparison",
                "metric": metric,
                "value": value,
                "notes": "mirrored_rl is positive class",
            }
        )

    tn, fp = matrix[0]
    fn, tp = matrix[1]
    for metric, value in {"tn": tn, "fp": fp, "fn": fn, "tp": tp}.items():
        rows.append(
            {
                "section": "confusion_matrix",
                "mode": "comparison",
                "metric": metric,
                "value": int(value),
                "notes": "test split count",
            }
        )

    classifier = model.named_steps["classifier"]
    for feature, coefficient in zip(FEATURE_COLUMNS, classifier.coef_[0], strict=True):
        rows.append(
            {
                "section": "model_coefficient",
                "mode": "comparison",
                "metric": feature,
                "value": float(coefficient),
                "notes": "standardized logistic coefficient",
            }
        )

    rows.extend(
        [
            {
                "section": "assumption",
                "mode": "both",
                "metric": "skull_distribution",
                "value": "18/17/17",
                "notes": "skull 1, 2, 3 card counts",
            },
            {
                "section": "assumption",
                "mode": "both",
                "metric": "judge_timing",
                "value": "after_two_cards",
                "notes": "judge after player and AI reveal",
            },
            {
                "section": "assumption",
                "mode": "both",
                "metric": "overflow_policy",
                "value": "reset_pile",
                "notes": "only the pile over 3 is reset",
            },
        ]
    )
    return pd.DataFrame(rows, columns=["section", "mode", "metric", "value", "notes"])


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--trials-per-mode", type=int, default=10_000)
    parser.add_argument("--seed", type=int, default=20260806)
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=Path(__file__).resolve().parent / "output",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    if args.trials_per_mode < 100:
        raise ValueError("trials-per-mode는 안정적인 비교를 위해 100 이상이어야 합니다.")

    args.output_dir.mkdir(parents=True, exist_ok=True)
    base_deck = build_standard_deck(args.seed)
    records = [
        simulate_trial(mode, trial_id, args.seed, base_deck)
        for mode in (MODE_ALIGNED, MODE_MIRRORED)
        for trial_id in range(args.trials_per_mode)
    ]
    data = pd.DataFrame.from_records(records)

    x_train, x_test, y_train, y_test = train_test_split(
        data.loc[:, list(FEATURE_COLUMNS)],
        data["label"],
        test_size=0.2,
        random_state=args.seed,
        stratify=data["label"],
    )
    model = Pipeline(
        steps=[
            ("scaler", StandardScaler()),
            (
                "classifier",
                LogisticRegression(max_iter=10_000, random_state=args.seed),
            ),
        ]
    )
    model.fit(x_train, y_train)
    predictions = model.predict(x_test)
    metrics = {
        "baseline_accuracy": 0.5,
        "accuracy": float(accuracy_score(y_test, predictions)),
        "precision": float(precision_score(y_test, predictions, zero_division=0)),
        "recall": float(recall_score(y_test, predictions, zero_division=0)),
        "f1": float(f1_score(y_test, predictions, zero_division=0)),
    }
    matrix = confusion_matrix(y_test, predictions).tolist()

    results = build_result_rows(data, metrics, matrix, model)
    results_path = args.output_dir / "card_flip_order_ml_results.csv"
    sequence_path = args.output_dir / "card_flip_order_sample_sequence.csv"
    results.to_csv(results_path, index=False, encoding="utf-8-sig")
    build_sample_sequence(base_deck, args.seed).to_csv(
        sequence_path,
        index=False,
        encoding="utf-8-sig",
    )

    print(f"results={results_path}")
    print(f"sequence={sequence_path}")
    print(f"accuracy={metrics['accuracy']:.4f}")
    print(f"f1={metrics['f1']:.4f}")


if __name__ == "__main__":
    main()
