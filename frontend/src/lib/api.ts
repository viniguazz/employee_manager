const API_URL = import.meta.env.VITE_API_URL;

function getToken() {
  return localStorage.getItem("accessToken");
}

export async function api<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getToken();

  const headers = new Headers(options.headers || {});
  headers.set("Content-Type", "application/json");
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    const contentType = res.headers.get("content-type") || "";
    if (contentType.includes("application/json")) {
      const data = await res.json().catch(() => null);
      const message = data?.message;
      throw new Error(message || `HTTP ${res.status}`);
    }

    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }

  // 204 No Content
  if (res.status === 204) return null as T;

  return (await res.json()) as T;
}