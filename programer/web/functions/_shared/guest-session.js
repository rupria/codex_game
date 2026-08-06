const COOKIE_NAME = "codex_guest";
const UUID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function parseCookies(header) {
  if (!header) {
    return {};
  }

  return Object.fromEntries(
    header.split(";").map((part) => {
      const separator = part.indexOf("=");
      if (separator < 0) {
        return [part.trim(), ""];
      }

      const key = part.slice(0, separator).trim();
      const value = part.slice(separator + 1).trim();
      return [key, value];
    }),
  );
}

function toBase64Url(bytes) {
  let binary = "";
  for (const byte of bytes) {
    binary += String.fromCharCode(byte);
  }

  return btoa(binary)
    .replaceAll("+", "-")
    .replaceAll("/", "_")
    .replaceAll("=", "");
}

function fromBase64Url(value) {
  const base64 = value.replaceAll("-", "+").replaceAll("_", "/");
  const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
  const binary = atob(padded);
  return Uint8Array.from(binary, (character) => character.charCodeAt(0));
}

async function importSigningKey(secret) {
  if (!secret || secret.length < 32) {
    throw new Error("GUEST_TOKEN_SECRET must contain at least 32 characters.");
  }

  return crypto.subtle.importKey(
    "raw",
    new TextEncoder().encode(secret),
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign", "verify"],
  );
}

async function signGuestId(guestId, secret) {
  const key = await importSigningKey(secret);
  const signature = await crypto.subtle.sign(
    "HMAC",
    key,
    new TextEncoder().encode(guestId),
  );
  return toBase64Url(new Uint8Array(signature));
}

export async function createGuestToken(secret, guestId = crypto.randomUUID()) {
  if (!UUID_PATTERN.test(guestId)) {
    throw new Error("Guest ID must be a UUID v4.");
  }

  return guestId + "." + (await signGuestId(guestId, secret));
}

export async function verifyGuestToken(token, secret) {
  if (!token) {
    return null;
  }

  const separator = token.indexOf(".");
  if (separator < 0) {
    return null;
  }

  const guestId = token.slice(0, separator);
  const signature = token.slice(separator + 1);
  if (!UUID_PATTERN.test(guestId) || !signature) {
    return null;
  }

  try {
    const key = await importSigningKey(secret);
    const valid = await crypto.subtle.verify(
      "HMAC",
      key,
      fromBase64Url(signature),
      new TextEncoder().encode(guestId),
    );
    return valid ? guestId : null;
  } catch {
    return null;
  }
}

export async function getOrCreateGuest(request, secret) {
  const cookies = parseCookies(request.headers.get("cookie"));
  const existingGuestId = await verifyGuestToken(cookies[COOKIE_NAME], secret);
  if (existingGuestId) {
    return { guestId: existingGuestId, setCookie: null };
  }

  const token = await createGuestToken(secret);
  return {
    guestId: token.slice(0, token.indexOf(".")),
    setCookie:
      COOKIE_NAME + "=" + token + "; Path=/; Max-Age=31536000; " +
      "HttpOnly; Secure; SameSite=Lax",
  };
}
