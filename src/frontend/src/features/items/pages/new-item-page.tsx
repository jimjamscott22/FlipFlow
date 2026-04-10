import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { createItem } from "@/features/items/api/items-api";
import { ItemEditorForm } from "@/features/items/components/item-editor-form";
import type { SaveItemPayload } from "@/features/items/types";

export function NewItemPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (payload: SaveItemPayload) => createItem(payload),
    onSuccess: async (item) => {
      await queryClient.invalidateQueries({ queryKey: ["items"] });
      navigate(`/items/${item.id}`);
    },
  });

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] bg-white/80 px-8 py-8 shadow-panel backdrop-blur">
        <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-500">
          Item Intake
        </p>
        <h2 className="mt-3 text-3xl font-bold text-slate-950">Add a new item draft</h2>
        <p className="mt-2 max-w-2xl text-sm text-muted-foreground">
          Capture the inventory facts once so pricing, listing generation, and marketplace tracking all have a reliable starting point later.
        </p>
      </section>

      <Card>
        <CardHeader>
          <CardTitle>Item details</CardTitle>
          <CardDescription>
            Start with the structured resale details you already know today.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {mutation.error ? (
            <p className="text-sm text-red-600">{mutation.error.message}</p>
          ) : null}

          <ItemEditorForm
            isSubmitting={mutation.isPending}
            onSubmit={(values) => mutation.mutate(values)}
            submitLabel="Create item"
          />
        </CardContent>
      </Card>
    </div>
  );
}
