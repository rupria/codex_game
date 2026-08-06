import { loadSave, upsertSave } from "../_shared/database.js";
import { getOrCreateGuest } from "../_shared/guest-session.js";
import {
  jsonResponse,
  methodNotAllowed,
  serviceUnavailable,
} from "../_shared/response.js";
import { validateSave } from "../_shared/validation.js";

export async function onRequest(context) {
  if (!["GET", "PUT"].includes(context.request.method)) {
    return methodNotAllowed(["GET", "PUT"]);
  }

  try {
    const guest = await getOrCreateGuest(
      context.request,
      context.env.GUEST_TOKEN_SECRET,
    );
    const headers = guest.setCookie ? { "set-cookie": guest.setCookie } : {};

    if (context.request.method === "GET") {
      const save = await loadSave(context.env, guest.guestId);
      if (!save) {
        return jsonResponse(404, { error: "save_not_found" }, headers);
      }

      const data =
        typeof save.save_data === "string"
          ? JSON.parse(save.save_data)
          : save.save_data;
      return jsonResponse(
        200,
        {
          saveVersion: Number(save.save_version),
          data,
          updatedAt: save.updated_at,
        },
        headers,
      );
    }

    let body;
    try {
      body = await context.request.json();
    } catch {
      return jsonResponse(400, { error: "invalid_json" }, headers);
    }

    const save = validateSave(body);
    if (!save) {
      return jsonResponse(400, { error: "invalid_save" }, headers);
    }

    await upsertSave(context.env, guest.guestId, save);
    return jsonResponse(200, { saved: true }, headers);
  } catch {
    return serviceUnavailable();
  }
}
