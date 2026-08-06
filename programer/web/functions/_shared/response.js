export function jsonResponse(status, body, headers = {}) {
  return new Response(JSON.stringify(body), {
    status,
    headers: {
      "content-type": "application/json; charset=utf-8",
      "cache-control": "no-store",
      ...headers,
    },
  });
}

export function methodNotAllowed(allowed) {
  return jsonResponse(
    405,
    { error: "method_not_allowed" },
    { allow: allowed.join(", ") },
  );
}

export function serviceUnavailable() {
  return jsonResponse(503, {
    error: "storage_unavailable",
    retryable: true,
  });
}
