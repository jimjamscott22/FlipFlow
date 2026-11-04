import { LayoutDashboard, ListChecks, Package2, PlusSquare } from "lucide-react";
import { NavLink, Outlet } from "react-router-dom";
import { cn } from "@/lib/utils";

const navigation = [
  { to: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { to: "/items", label: "My Items", icon: Package2 },
  { to: "/items/new", label: "New Item", icon: PlusSquare },
  { to: "/listings", label: "Listings", icon: ListChecks },
];

export function AppShell() {
  return (
    <div className="min-h-screen">
      <header className="border-b border-white/70 bg-white/70 backdrop-blur">
        <div className="container flex items-center py-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-500">
              FlipFlow
            </p>
            <h1 className="text-xl font-bold text-slate-900">Seller workspace</h1>
          </div>
        </div>
      </header>

      <div className="container grid gap-8 py-8 lg:grid-cols-[240px_1fr]">
        <aside className="rounded-3xl border border-white/70 bg-white/80 p-3 shadow-panel">
          <nav className="space-y-1">
            {navigation.map(({ to, label, icon: Icon }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium text-slate-600 transition hover:bg-slate-100 hover:text-slate-900",
                    isActive && "bg-slate-900 text-white hover:bg-slate-900 hover:text-white",
                  )
                }
              >
                <Icon className="h-4 w-4" />
                <span>{label}</span>
              </NavLink>
            ))}
          </nav>
        </aside>

        <main>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
