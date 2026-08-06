import { getOrCreateGuest } from "../_shared/guest-session.js";
import { jsonResponse, methodNotAllowed } from "../_shared/response.js";

export async function onRequest(context) {
  if (context.request.method !== "GET") {
    return methodNotAllowed(["GET"]);
  }

  const guest = await getOrCreateGuest(
    context.request,
    context.env.GUEST_TOKEN_SECRET,
  );
  const headers = guest.setCookie ? { "set-cookie": guest.setCookie } : {};

  return jsonResponse(200, { ready: true }, headers);
}
