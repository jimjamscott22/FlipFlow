import { useQuery } from "@tanstack/react-query";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { apiRequest } from "@/lib/api-client";

type DashboardSummary = {
  displayName: string;
  totalItems: number;
  draftItems: number;
  activeListings: number;
  pendingRecommendations: number;
};

async function getDashboardSummary() {
  return apiRequest<DashboardSummary>("/dashboard/summary", {
    method: "GET",
    authenticated: true,
  });
}

export function DashboardPage() {
  const query = useQuery({
    queryKey: ["dashboard", "summary"],
    queryFn: getDashboardSummary,
  });

  const cards = [
    { label: "Items", value: query.data?.totalItems ?? 0 },
    { label: "Drafts", value: query.data?.draftItems ?? 0 },
    { label: "Live listings", value: query.data?.activeListings ?? 0 },
    { label: "Pending repricing", value: query.data?.pendingRecommendations ?? 0 },
  ];

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] bg-slate-950 px-8 py-10 text-white">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300">Phase 2</p>
        <h2 className="mt-4 text-4xl font-bold">
          {query.data ? `Welcome back, ${query.data.displayName}.` : "Loading your dashboard..."}
        </h2>
        <p className="mt-3 max-w-2xl text-slate-300">
          Inventory CRUD and item photo uploads are now layered on top of the authenticated seller workspace.
        </p>
      </section>

      {query.isError ? (
        <Card>
          <CardContent className="pt-6 text-sm text-red-600">
            {query.error.message}
          </CardContent>
        </Card>
      ) : null}

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => (
          <Card key={card.label}>
            <CardHeader>
              <CardDescription>{card.label}</CardDescription>
              <CardTitle className="text-3xl">{card.value}</CardTitle>
            </CardHeader>
          </Card>
        ))}
      </section>

      <section className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Next build target</CardTitle>
            <CardDescription>Phase 3 will add listing drafts and AI content generation.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2 text-sm text-muted-foreground">
            <p>Items now have full create, edit, detail, and upload flows.</p>
            <p>The next layer will turn those records into reusable listing content drafts.</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Demo access</CardTitle>
            <CardDescription>Seeded account for local development</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            <p>Email: <span className="font-semibold">demo@flipflow.local</span></p>
            <p>Password: <span className="font-semibold">FlipFlow123</span></p>
          </CardContent>
        </Card>
      </section>
    </div>
  );
}
