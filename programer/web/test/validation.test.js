import assert from "node:assert/strict";
import test from "node:test";
import {
  validateMatchResult,
  validateSave,
} from "../functions/_shared/validation.js";

test("valid match result is normalized", () => {
  const result = validateMatchResult({
    matchId: "123E4567-E89B-42D3-A456-426614174000",
    result: "player_win",
    stage: 1,
    durationMs: 180000,
    inputTimeMs: 321,
    contentVersion: "prototype-0.03",
  });

  assert.equal(
    result.matchId,
    "123e4567-e89b-42d3-a456-426614174000",
  );
});

test("invalid input time is rejected", () => {
  assert.equal(
    validateMatchResult({
      matchId: "123e4567-e89b-42d3-a456-426614174000",
      result: "player_win",
      stage: 1,
      durationMs: 180000,
      inputTimeMs: -1,
      contentVersion: "prototype-0.03",
    }),
    null,
  );
});

test("save payload accepts an object within the limit", () => {
  const save = validateSave({
    saveVersion: 1,
    data: { stage: 2, playerHp: 3 },
  });

  assert.equal(save.saveVersion, 1);
  assert.equal(save.data, '{"stage":2,"playerHp":3}');
});
