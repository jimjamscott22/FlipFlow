import { Outlet } from "react-router-dom";

export function AuthLayout() {
  return (
    <main className="relative min-h-screen overflow-hidden">
      <div className="absolute inset-0 bg-hero-grid bg-[size:22px_22px] opacity-50" />
      <div className="container relative flex min-h-screen items-center justify-center py-10">
        <div className="grid w-full max-w-6xl gap-8 lg:grid-cols-[1.1fr_0.9fr]">
          <section className="hidden rounded-[2rem] bg-slate-950 p-10 text-slate-50 lg:flex lg:flex-col lg:justify-between">
            <div className="space-y-6">
              <p className="text-sm font-semibold uppercase tracking-[0.3em] text-amber-300">
                FlipFlow
              </p>
              <div className="space-y-4">
                <h1 className="max-w-lg text-5xl font-extrabold leading-tight">
                  Turn neglected electronics into polished listings faster.
                </h1>
                <p className="max-w-lg text-base text-slate-300">
                  Portfolio-grade full-stack app for reseller workflows, pricing discipline,
                  and marketplace execution.
                </p>
              </div>
            </div>

            <div className="grid gap-4 text-sm text-slate-300 sm:grid-cols-3">
              <div>
                <p className="text-2xl font-bold text-white">3</p>
                <p>Marketplace targets</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-white">1</p>
                <p>Seller dashboard</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-white">∞</p>
                <p>Future category growth</p>
              </div>
            </div>
          </section>

          <section className="flex items-center">
            <Outlet />
          </section>
        </div>
      </div>
    </main>
  );
}
