import { RouterProvider, createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import { AppShell } from "@/components/layout/app-shell";
import { AuthLayout } from "@/components/layout/auth-layout";
import { ProtectedRoute } from "@/features/auth/components/protected-route";
import { AuthProvider } from "@/features/auth/hooks/use-auth";
import { DashboardPage } from "@/features/dashboard/pages/dashboard-page";
import { LoginPage } from "@/features/auth/pages/login-page";
import { RegisterPage } from "@/features/auth/pages/register-page";
import { ItemsPage } from "@/features/items/pages/items-page";
import { ListingsPage } from "@/features/items/pages/listings-page";
import { NewItemPage } from "@/features/items/pages/new-item-page";

const router = createBrowserRouter([
  {
    element: (
      <AuthProvider>
        <Outlet />
      </AuthProvider>
    ),
    children: [
      {
        element: <ProtectedRoute />,
        children: [
          {
            element: <AppShell />,
            children: [
              { path: "/", element: <Navigate to="/dashboard" replace /> },
              { path: "/dashboard", element: <DashboardPage /> },
              { path: "/items", element: <ItemsPage /> },
              { path: "/items/new", element: <NewItemPage /> },
              { path: "/listings", element: <ListingsPage /> },
            ],
          },
        ],
      },
      {
        element: <AuthLayout />,
        children: [
          { path: "/login", element: <LoginPage /> },
          { path: "/register", element: <RegisterPage /> },
        ],
      },
    ],
  },
]);

export function AppRouterProvider() {
  return <RouterProvider router={router} />;
}
