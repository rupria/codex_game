import { connect } from "@tidbcloud/serverless";

function connection(env) {
  if (!env.DATABASE_URL) {
    throw new Error("DATABASE_URL is not configured.");
  }

  return connect({ url: env.DATABASE_URL });
}

export async function checkDatabase(env) {
  await connection(env).execute("SELECT 1 AS ready");
}

export async function insertMatchResult(env, guestId, match) {
  await connection(env).execute(
    "INSERT IGNORE INTO match_results " +
      "(match_id, guest_id, result, stage, duration_ms, input_time_ms, content_version) " +
      "VALUES (?, ?, ?, ?, ?, ?, ?)",
    [
      match.matchId,
      guestId,
      match.result,
      match.stage,
      match.durationMs,
      match.inputTimeMs,
      match.contentVersion,
    ],
  );
}

export async function loadSave(env, guestId) {
  const rows = await connection(env).execute(
    "SELECT save_version, save_data, updated_at " +
      "FROM guest_saves WHERE guest_id = ? LIMIT 1",
    [guestId],
  );

  return rows.length > 0 ? rows[0] : null;
}

export async function upsertSave(env, guestId, save) {
  await connection(env).execute(
    "INSERT INTO guest_saves (guest_id, save_version, save_data) " +
      "VALUES (?, ?, CAST(? AS JSON)) " +
      "ON DUPLICATE KEY UPDATE " +
      "save_version = VALUES(save_version), " +
      "save_data = VALUES(save_data), " +
      "updated_at = CURRENT_TIMESTAMP(3)",
    [guestId, save.saveVersion, save.data],
  );
}
