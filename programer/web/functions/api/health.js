import { checkDatabase } from "../_shared/database.js";
import { jsonResponse, methodNotAllowed } from "../_shared/response.js";

export async function onRequest(context) {
  if (context.request.method !== "GET") {
    return methodNotAllowed(["GET"]);
  }

  try {
    await checkDatabase(context.env);
    return jsonResponse(200, { ready: true });
  } catch {
    return jsonResponse(503, { ready: false });
  }
}
