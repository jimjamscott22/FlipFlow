import { Camera, PencilLine } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getItemPhotoUrl } from "@/features/items/api/items-api";
import { itemConditionLabels, itemStatusLabels, type ItemSummary } from "@/features/items/types";

type ItemCardProps = {
  item: ItemSummary;
};

export function ItemCard({ item }: ItemCardProps) {
  const imageUrl = getItemPhotoUrl(item.primaryPhotoRelativePath);

  return (
    <Card className="overflow-hidden border-slate-200/90">
      <div className="aspect-[4/3] bg-slate-100">
        {imageUrl ? (
          <img alt={item.title} className="h-full w-full object-cover" src={imageUrl} />
        ) : (
          <div className="flex h-full items-center justify-center bg-[radial-gradient(circle_at_top,_rgba(251,191,36,0.25),_transparent_45%),linear-gradient(135deg,_rgba(15,23,42,0.96),_rgba(30,41,59,0.92))] text-white">
            <div className="text-center">
              <Camera className="mx-auto h-8 w-8 text-amber-300" />
              <p className="mt-3 text-sm text-slate-300">No photos yet</p>
            </div>
          </div>
        )}
      </div>

      <CardContent className="space-y-4 p-5">
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <StatusChip label={itemStatusLabels[item.status]} tone="slate" />
            <StatusChip label={itemConditionLabels[item.condition]} tone="amber" />
          </div>

          <div>
            <h3 className="text-lg font-semibold text-slate-950">{item.title}</h3>
            <p className="text-sm text-slate-500">
              {item.brand} {item.model}
            </p>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3 rounded-2xl bg-slate-50 p-4 text-sm">
          <Metric label="Price" value={formatCurrency(item.askingPrice)} />
          <Metric label="Photos" value={`${item.photoCount}`} />
          <Metric label="Category" value={item.category} />
          <Metric label="Updated" value={formatDate(item.updatedAtUtc)} />
        </div>

        <Link
          className={cn(buttonVariants({ size: "default" }), "w-full")}
          to={`/items/${item.id}`}
        >
          <PencilLine className="mr-2 h-4 w-4" />
          Open item
        </Link>
      </CardContent>
    </Card>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs uppercase tracking-[0.18em] text-slate-400">{label}</p>
      <p className="mt-1 font-medium text-slate-700">{value}</p>
    </div>
  );
}

function StatusChip({ label, tone }: { label: string; tone: "slate" | "amber" }) {
  return (
    <span
      className={
        tone === "amber"
          ? "rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-amber-800"
          : "rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-slate-700"
      }
    >
      {label}
    </span>
  );
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value);
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
  }).format(new Date(value));
}
