import React from "react";
import ReactDOM from "react-dom/client";
import { QueryProvider } from "@/app/providers/query-provider";
import { AppRouterProvider } from "@/app/router/app-router";
import "@/index.css";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <QueryProvider>
      <AppRouterProvider />
    </QueryProvider>
  </React.StrictMode>,
);
