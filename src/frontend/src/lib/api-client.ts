import { env } from "@/lib/env";

/** Fired when the API answers 401 "passcode_required", so the passcode screen can appear. */
export const PASSCODE_REQUIRED_EVENT = "flipflow:passcode-required";

type ErrorPayload = {
  message?: string;
  code?: string;
  errors?: { error?: string }[];
};

export function resolveApiAssetUrl(path: string | null | undefined) {
  if (!path) {
    return null;
  }

  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const normalized = path.startsWith("/") ? path : `/${path}`;

  // Uploaded photos are served by the same origin. Only an absolute API base
  // URL (a custom VITE_API_BASE_URL) needs its origin prepended.
  return /^https?:\/\//i.test(env.apiBaseUrl)
    ? `${new URL(env.apiBaseUrl).origin}${normalized}`
    : normalized;
}

export async function apiRequest<T>(path: string, options: RequestInit = {}) {
  const headers = new Headers(options.headers);
  const isFormData = options.body instanceof FormData;

  if (!isFormData && options.body !== undefined && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${env.apiBaseUrl}${path}`, {
    credentials: "same-origin",
    ...options,
    headers,
  });

  if (!response.ok) {
    const payload: ErrorPayload | null = await response.json().catch(() => null);

    if (response.status === 401 && payload?.code === "passcode_required") {
      window.dispatchEvent(new Event(PASSCODE_REQUIRED_EVENT));
    }

    const message =
      payload?.message ??
      payload?.errors?.[0]?.error ??
      "The request could not be completed.";

    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get("Content-Type");

  if (!contentType?.includes("application/json")) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
