"""표준 52장 덱으로 100게임을 진행해 카드별 등장·제거 확률을 검증한다."""

from __future__ import annotations

import argparse
import random
from dataclasses import dataclass
from pathlib import Path

import pandas as pd


RANKS = ("A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K")
SUITS = ("S", "C", "H", "D")


@dataclass(frozen=True)
class Card:
    card_id: str
    suit: str
    rank: str
    skull: int


def build_deck(seed: int) -> list[Card]:
    """52장 각각을 고유하게 만들고 해골 1·2·3을 18·17·17장으로 배정한다."""
    identities = [(suit, rank) for suit in SUITS for rank in RANKS]
    skulls = [1] * 18 + [2] * 17 + [3] * 17
    mapping_rng = random.Random(seed + 1)
    mapping_rng.shuffle(skulls)
    return [
        Card(card_id=f"{suit}{rank}", suit=suit, rank=rank, skull=skull)
        for (suit, rank), skull in zip(identities, skulls, strict=True)
    ]


def empty_card_stats(deck: list[Card]) -> dict[str, dict[str, int | str]]:
    return {
        card.card_id: {
            "card_id": card.card_id,
            "suit": card.suit,
            "rank": card.rank,
            "skull": card.skull,
            "appearance_count": 0,
            "player_reveal_count": 0,
            "ai_reveal_count": 0,
            "player_acquired_count": 0,
            "ai_acquired_count": 0,
            "unacquired_field_removal_count": 0,
            "overflow_removal_count": 0,
            "visibility_eviction_count": 0,
            "selected_as_1_card_count": 0,
            "selected_as_2_card_count": 0,
        }
        for card in deck
    }


def player_screen_side(turn: int) -> str:
    return "left" if turn % 2 == 1 else "right"


def ai_screen_side(turn: int) -> str:
    # AI 본인 기준 왼쪽은 플레이어 화면 오른쪽이다.
    return "right" if turn % 2 == 1 else "left"


def remove_to_unacquired(
    cards: list[Card],
    stats: dict[str, dict[str, int | str]],
    reason: str,
) -> int:
    counter_name = (
        "overflow_removal_count"
        if reason == "overflow"
        else "unacquired_field_removal_count"
    )
    for card in cards:
        stats[card.card_id][counter_name] += 1
    return len(cards)


def append_visible_card(
    side: str,
    card: Card,
    piles: dict[str, list[Card]],
    sums: dict[str, int],
    stats: dict[str, dict[str, int | str]],
) -> int:
    """한 더미에 최신 카드 최대 2장만 유지하고 새 카드를 추가한다."""
    evicted = 0
    if len(piles[side]) == 2:
        oldest = piles[side].pop(0)
        sums[side] -= oldest.skull
        stats[oldest.card_id]["visibility_eviction_count"] += 1
        evicted = 1
    piles[side].append(card)
    sums[side] += card.skull
    return evicted


def simulate_game(
    game_id: int,
    seed: int,
    base_deck: list[Card],
    stats: dict[str, dict[str, int | str]],
) -> tuple[dict[str, int | str], list[dict[str, int | str]]]:
    """한쪽이 벨 3승을 달성할 때까지 최대 26회의 펼치기를 수행한다.

    펼치기 1회는 플레이어 1장, AI 1장 공개로 구성한다. 두 장 공개 후
    좌우 합계를 판정한다. 합이 3을 초과한 더미는 미획득 처리 후 비운다.
    벨 결과가 나오면 선택 더미는 승자가 획득하고 반대편은 미획득 처리한다.
    """
    rng = random.Random(seed + game_id * 1009)
    deck = base_deck.copy()
    rng.shuffle(deck)

    piles: dict[str, list[Card]] = {"left": [], "right": []}
    sums = {"left": 0, "right": 0}
    wins = {"player": 0, "ai": 0}
    event_rows: list[dict[str, int | str]] = []
    removed_size_events = {1: 0, 2: 0}
    unacquired_count = 0
    overflow_count = 0
    visibility_eviction_count = 0
    acquired_count = 0
    cards_drawn = 0
    turns_played = 0

    for turn in range(1, 27):
        turns_played = turn
        player_card = deck[(turn - 1) * 2]
        ai_card = deck[(turn - 1) * 2 + 1]
        cards_drawn += 2

        player_side = player_screen_side(turn)
        ai_side = ai_screen_side(turn)
        visibility_eviction_count += append_visible_card(
            player_side, player_card, piles, sums, stats
        )
        visibility_eviction_count += append_visible_card(
            ai_side, ai_card, piles, sums, stats
        )

        stats[player_card.card_id]["appearance_count"] += 1
        stats[player_card.card_id]["player_reveal_count"] += 1
        stats[ai_card.card_id]["appearance_count"] += 1
        stats[ai_card.card_id]["ai_reveal_count"] += 1

        for side in ("left", "right"):
            if sums[side] > 3:
                removed = remove_to_unacquired(piles[side], stats, "overflow")
                overflow_count += removed
                piles[side] = []
                sums[side] = 0

        valid_sides = [side for side in ("left", "right") if sums[side] == 3]
        if not valid_sides:
            continue

        winner = "player" if rng.random() < 0.5 else "ai"
        selected_side = rng.choice(valid_sides)
        acquired_cards = piles[selected_side].copy()
        acquired_size = len(acquired_cards)
        if acquired_size not in removed_size_events:
            raise AssertionError(f"노출 카드 제한 위반: {acquired_size}장")
        removed_size_events[acquired_size] += 1

        for card in acquired_cards:
            stats[card.card_id][f"{winner}_acquired_count"] += 1
            stats[card.card_id][f"selected_as_{acquired_size}_card_count"] += 1
        acquired_count += acquired_size

        opposite_side = "right" if selected_side == "left" else "left"
        discarded_cards = piles[opposite_side].copy()
        unacquired_count += remove_to_unacquired(discarded_cards, stats, "bell_reset")

        wins[winner] += 1
        event_rows.append(
            {
                "game_id": game_id,
                "turn": turn,
                "winner": winner,
                "selected_side": selected_side,
                "valid_side_count": len(valid_sides),
                "acquired_card_count": acquired_size,
                "acquired_cards": "|".join(card.card_id for card in acquired_cards),
                "opposite_removed_count": len(discarded_cards),
                "opposite_removed_cards": "|".join(card.card_id for card in discarded_cards),
                "player_bell_wins": wins["player"],
                "ai_bell_wins": wins["ai"],
            }
        )

        piles = {"left": [], "right": []}
        sums = {"left": 0, "right": 0}

        if wins[winner] == 3:
            break

    completed = max(wins.values()) == 3
    game_winner = max(wins, key=wins.get) if completed else "deck_limit"

    if not completed:
        for side in ("left", "right"):
            unacquired_count += remove_to_unacquired(piles[side], stats, "bell_reset")

    game_row = {
        "game_id": game_id,
        "winner": game_winner,
        "completed_three_wins": int(completed),
        "turns_played": turns_played,
        "cards_drawn": cards_drawn,
        "remaining_deck_cards": 52 - cards_drawn,
        "player_bell_wins": wins["player"],
        "ai_bell_wins": wins["ai"],
        "bell_event_count": len(event_rows),
        "one_card_acquisition_events": removed_size_events[1],
        "two_card_acquisition_events": removed_size_events[2],
        "acquired_card_count": acquired_count,
        "unacquired_field_removal_count": unacquired_count,
        "overflow_removal_count": overflow_count,
        "visibility_eviction_count": visibility_eviction_count,
    }
    return game_row, event_rows


def run_simulation(games: int, seed: int, output_dir: Path) -> dict[str, object]:
    if games <= 0:
        raise ValueError("games는 1 이상이어야 합니다.")

    output_dir.mkdir(parents=True, exist_ok=True)
    deck = build_deck(seed)
    stats = empty_card_stats(deck)
    game_rows: list[dict[str, int | str]] = []
    event_rows: list[dict[str, int | str]] = []

    for game_id in range(1, games + 1):
        game_row, game_events = simulate_game(game_id, seed, deck, stats)
        game_rows.append(game_row)
        event_rows.extend(game_events)

    card_df = pd.DataFrame(stats.values())
    total_appearances = int(card_df["appearance_count"].sum())
    mean_appearance = total_appearances / 52
    card_df["appearance_rate_per_game"] = card_df["appearance_count"] / games
    card_df["player_reveal_rate_per_game"] = card_df["player_reveal_count"] / games
    card_df["ai_reveal_rate_per_game"] = card_df["ai_reveal_count"] / games
    card_df["total_acquired_count"] = (
        card_df["player_acquired_count"] + card_df["ai_acquired_count"]
    )
    card_df["acquired_given_appearance_rate"] = (
        card_df["total_acquired_count"]
        / card_df["appearance_count"].where(card_df["appearance_count"] > 0, 1)
    )
    card_df["appearance_minus_mean"] = card_df["appearance_count"] - mean_appearance
    card_df = card_df.sort_values(
        ["appearance_count", "card_id"], ascending=[False, True]
    ).reset_index(drop=True)

    game_df = pd.DataFrame(game_rows)
    event_df = pd.DataFrame(event_rows)
    skull_df = (
        card_df.groupby("skull", as_index=False)
        .agg(
            card_count=("card_id", "count"),
            mean_appearance_count=("appearance_count", "mean"),
            total_appearance_count=("appearance_count", "sum"),
            total_acquired_count=("total_acquired_count", "sum"),
            mean_acquired_given_appearance_rate=("acquired_given_appearance_rate", "mean"),
        )
        .sort_values("skull")
    )

    paths = {
        "card": output_dir / "card_appearance_100_games.csv",
        "game": output_dir / "game_summary_100_games.csv",
        "event": output_dir / "removal_events_100_games.csv",
        "skull": output_dir / "appearance_by_skull_100_games.csv",
    }
    card_df.to_csv(paths["card"], index=False, encoding="utf-8-sig")
    game_df.to_csv(paths["game"], index=False, encoding="utf-8-sig")
    event_df.to_csv(paths["event"], index=False, encoding="utf-8-sig")
    skull_df.to_csv(paths["skull"], index=False, encoding="utf-8-sig")

    return {
        "card_df": card_df,
        "game_df": game_df,
        "event_df": event_df,
        "skull_df": skull_df,
        "paths": paths,
        "mean_appearance_count": mean_appearance,
    }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--games", type=int, default=100)
    parser.add_argument("--seed", type=int, default=20260806)
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=Path(__file__).resolve().parent / "output",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    artifacts = run_simulation(args.games, args.seed, args.output_dir)
    print("100게임 카드 등장 확률 검증 완료")
    for key, path in artifacts["paths"].items():
        print(f"{key}={path}")
    print(f"mean_appearance_count={artifacts['mean_appearance_count']:.4f}")
    print(artifacts["card_df"].head(10).to_string(index=False))


if __name__ == "__main__":
    main()
