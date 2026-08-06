import { insertMatchResult } from "../_shared/database.js";
import { getOrCreateGuest } from "../_shared/guest-session.js";
import {
  jsonResponse,
  methodNotAllowed,
  serviceUnavailable,
} from "../_shared/response.js";
import { validateMatchResult } from "../_shared/validation.js";

export async function onRequest(context) {
  if (context.request.method !== "POST") {
    return methodNotAllowed(["POST"]);
  }

  let body;
  try {
    body = await context.request.json();
  } catch {
    return jsonResponse(400, { error: "invalid_json" });
  }

  const match = validateMatchResult(body);
  if (!match) {
    return jsonResponse(400, { error: "invalid_match_result" });
  }

  try {
    const guest = await getOrCreateGuest(
      context.request,
      context.env.GUEST_TOKEN_SECRET,
    );
    await insertMatchResult(context.env, guest.guestId, match);
    const headers = guest.setCookie ? { "set-cookie": guest.setCookie } : {};
    return jsonResponse(202, { recorded: true }, headers);
  } catch {
    return serviceUnavailable();
  }
}
