import assert from "node:assert/strict";
import test from "node:test";
import {
  createGuestToken,
  verifyGuestToken,
} from "../functions/_shared/guest-session.js";

const SECRET = "0123456789abcdef0123456789abcdef";
const GUEST_ID = "123e4567-e89b-42d3-a456-426614174000";

test("a signed guest token verifies without a login", async () => {
  const token = await createGuestToken(SECRET, GUEST_ID);
  assert.equal(await verifyGuestToken(token, SECRET), GUEST_ID);
});

test("a modified guest token is rejected", async () => {
  const token = await createGuestToken(SECRET, GUEST_ID);
  const modified = token.slice(0, -1) + (token.endsWith("a") ? "b" : "a");
  assert.equal(await verifyGuestToken(modified, SECRET), null);
});

test("a first visit receives an invisible signed guest cookie", async () => {
  const { getOrCreateGuest } = await import(
    "../functions/_shared/guest-session.js"
  );
  const first = await getOrCreateGuest(
    new Request("https://game.example/api/session"),
    SECRET,
  );

  assert.match(first.setCookie, /HttpOnly; Secure; SameSite=Lax/);
  const cookieValue = first.setCookie.split(";")[0];
  const returning = await getOrCreateGuest(
    new Request("https://game.example/api/session", {
      headers: { cookie: cookieValue },
    }),
    SECRET,
  );
  assert.equal(returning.guestId, first.guestId);
  assert.equal(returning.setCookie, null);
});
