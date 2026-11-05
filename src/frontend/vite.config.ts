import path from "node:path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173,
    // In development the frontend runs on :5173 and the API on :5000. Proxying keeps the
    // browser on one origin, so the passcode cookie works and no CORS setup is needed.
    proxy: {
      "/api": "http://localhost:5000",
      "/storage": "http://localhost:5000",
    },
  },
});
