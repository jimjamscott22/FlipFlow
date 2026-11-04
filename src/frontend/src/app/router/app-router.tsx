import { RouterProvider, createBrowserRouter, Navigate } from "react-router-dom";
import { AppShell } from "@/components/layout/app-shell";
import { PasscodeGate } from "@/features/access/components/passcode-gate";
import { DashboardPage } from "@/features/dashboard/pages/dashboard-page";
import { ItemsPage } from "@/features/items/pages/items-page";
import { ListingsPage } from "@/features/items/pages/listings-page";
import { NewItemPage } from "@/features/items/pages/new-item-page";
import { ItemDetailPage } from "@/features/items/pages/item-detail-page";

const router = createBrowserRouter([
  {
    element: (
      <PasscodeGate>
        <AppShell />
      </PasscodeGate>
    ),
    children: [
      { path: "/", element: <Navigate to="/dashboard" replace /> },
      { path: "/dashboard", element: <DashboardPage /> },
      { path: "/items", element: <ItemsPage /> },
      { path: "/items/new", element: <NewItemPage /> },
      { path: "/items/:itemId", element: <ItemDetailPage /> },
      { path: "/listings", element: <ListingsPage /> },
    ],
  },
]);

export function AppRouterProvider() {
  return <RouterProvider router={router} />;
}
