const UUID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const MATCH_RESULTS = new Set([
  "player_win",
  "ai_win",
  "draw",
  "abandoned",
]);
const MAX_SAVE_BYTES = 32 * 1024;

function isIntegerInRange(value, minimum, maximum) {
  return Number.isInteger(value) && value >= minimum && value <= maximum;
}

export function validateMatchResult(value) {
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    return null;
  }

  if (!UUID_PATTERN.test(value.matchId ?? "")) {
    return null;
  }

  if (!MATCH_RESULTS.has(value.result)) {
    return null;
  }

  if (!isIntegerInRange(value.stage, 1, 1000)) {
    return null;
  }

  if (!isIntegerInRange(value.durationMs, 0, 7_200_000)) {
    return null;
  }

  if (
    value.inputTimeMs !== null &&
    value.inputTimeMs !== undefined &&
    !isIntegerInRange(value.inputTimeMs, 0, 120_000)
  ) {
    return null;
  }

  if (
    typeof value.contentVersion !== "string" ||
    value.contentVersion.length < 1 ||
    value.contentVersion.length > 64
  ) {
    return null;
  }

  return {
    matchId: value.matchId.toLowerCase(),
    result: value.result,
    stage: value.stage,
    durationMs: value.durationMs,
    inputTimeMs: value.inputTimeMs ?? null,
    contentVersion: value.contentVersion,
  };
}

export function validateSave(value) {
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    return null;
  }

  if (!isIntegerInRange(value.saveVersion, 1, 1_000_000)) {
    return null;
  }

  if (
    !value.data ||
    typeof value.data !== "object" ||
    Array.isArray(value.data)
  ) {
    return null;
  }

  const serialized = JSON.stringify(value.data);
  if (new TextEncoder().encode(serialized).byteLength > MAX_SAVE_BYTES) {
    return null;
  }

  return {
    saveVersion: value.saveVersion,
    data: serialized,
  };
}
