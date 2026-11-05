export const env = {
  // Same-origin by default: in production the API serves the frontend, and in
  // development the Vite dev server proxies /api and /storage to the API.
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "/api",
};
