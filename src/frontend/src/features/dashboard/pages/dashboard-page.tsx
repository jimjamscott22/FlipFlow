import { useQuery } from "@tanstack/react-query";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { apiRequest } from "@/lib/api-client";

type DashboardSummary = {
  totalItems: number;
  draftItems: number;
  activeListings: number;
};

async function getDashboardSummary() {
  return apiRequest<DashboardSummary>("/dashboard/summary", {
    method: "GET",
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
  ];

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] bg-slate-950 px-8 py-10 text-white">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300">FlipFlow</p>
        <h2 className="mt-4 text-4xl font-bold">
          Your inventory at a glance.
        </h2>
        <p className="mt-3 max-w-2xl text-slate-300">
          A personal tool for photographing, listing and tracking the things you resell.
        </p>
      </section>

      {query.isError ? (
        <Card>
          <CardContent className="pt-6 text-sm text-red-600">
            {query.error.message}
          </CardContent>
        </Card>
      ) : null}

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {cards.map((card) => (
          <Card key={card.label}>
            <CardHeader>
              <CardDescription>{card.label}</CardDescription>
              <CardTitle className="text-3xl">{card.value}</CardTitle>
            </CardHeader>
          </Card>
        ))}
      </section>

      <section className="grid gap-4">
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
      </section>
    </div>
  );
}
