import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { PlusSquare, Search } from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { getItems } from "@/features/items/api/items-api";
import { ItemCard } from "@/features/items/components/item-card";
import { itemStatusLabels, itemStatuses } from "@/features/items/types";

export function ItemsPage() {
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<"all" | (typeof itemStatuses)[number]>("all");

  const query = useQuery({
    queryKey: ["items"],
    queryFn: getItems,
  });

  const items = query.data ?? [];
  const normalizedSearch = search.trim().toLowerCase();
  const filteredItems = items.filter((item) => {
    const matchesStatus = statusFilter === "all" || item.status === statusFilter;
    const haystack = [item.title, item.brand, item.model, item.category].join(" ").toLowerCase();
    const matchesSearch = normalizedSearch.length === 0 || haystack.includes(normalizedSearch);

    return matchesStatus && matchesSearch;
  });

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] bg-[linear-gradient(135deg,_rgba(15,23,42,0.98),_rgba(30,41,59,0.94)),radial-gradient(circle_at_top_right,_rgba(245,158,11,0.18),_transparent_30%)] px-8 py-10 text-white">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
          <div className="max-w-2xl">
            <p className="text-xs font-semibold uppercase tracking-[0.3em] text-amber-300">
              Phase 2
            </p>
            <h2 className="mt-4 text-4xl font-bold">Inventory takes shape here.</h2>
            <p className="mt-3 text-slate-300">
              Capture your electronics, track condition and pricing, and start building the item library that future listing workflows will depend on.
            </p>
          </div>

          <Link
            className={cn(
              buttonVariants({ size: "lg" }),
              "h-12 bg-amber-400 text-slate-950 hover:bg-amber-300",
            )}
            to="/items/new"
          >
            <PlusSquare className="mr-2 h-4 w-4" />
            Add new item
          </Link>
        </div>
      </section>

      <Card>
        <CardHeader>
          <CardTitle>My items</CardTitle>
          <CardDescription>
            Review everything in your inventory pipeline before it becomes a listing.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-5">
          <div className="grid gap-4 lg:grid-cols-[1fr_220px]">
            <div className="relative">
              <Search className="pointer-events-none absolute left-3 top-3.5 h-4 w-4 text-slate-400" />
              <Input
                className="pl-10"
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Search by title, brand, model, or category"
                value={search}
              />
            </div>

            <Select
              onChange={(event) => setStatusFilter(event.target.value as typeof statusFilter)}
              value={statusFilter}
            >
              <option value="all">All statuses</option>
              {itemStatuses.map((status) => (
                <option key={status} value={status}>
                  {itemStatusLabels[status]}
                </option>
              ))}
            </Select>
          </div>

          {query.isLoading ? (
            <p className="text-sm text-muted-foreground">Loading your inventory...</p>
          ) : null}

          {query.isError ? (
            <p className="text-sm text-red-600">{query.error.message}</p>
          ) : null}

          {!query.isLoading && !query.isError && filteredItems.length === 0 ? (
            <div className="rounded-3xl border border-dashed border-slate-300 bg-slate-50 px-6 py-12 text-center">
              <p className="text-lg font-semibold text-slate-900">
                {items.length === 0 ? "No inventory yet." : "No items match that filter."}
              </p>
              <p className="mt-2 text-sm text-muted-foreground">
                {items.length === 0
                  ? "Create your first item to start building reusable listing data."
                  : "Try a different search term or status filter."}
              </p>
            </div>
          ) : null}

          <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
            {filteredItems.map((item) => (
              <ItemCard key={item.id} item={item} />
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
