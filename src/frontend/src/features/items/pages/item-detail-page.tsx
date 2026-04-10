import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Camera, ImagePlus, Trash2 } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { deleteItem, getItem, getItemPhotoUrl, uploadItemPhoto, updateItem } from "@/features/items/api/items-api";
import { ItemEditorForm } from "@/features/items/components/item-editor-form";
import { itemConditionLabels, itemStatusLabels, type SaveItemPayload } from "@/features/items/types";

export function ItemDetailPage() {
  const { itemId } = useParams<{ itemId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  const itemQuery = useQuery({
    queryKey: ["items", itemId],
    queryFn: () => getItem(itemId!),
    enabled: Boolean(itemId),
  });

  const updateMutation = useMutation({
    mutationFn: (payload: SaveItemPayload) => updateItem(itemId!, payload),
    onSuccess: async (updatedItem) => {
      queryClient.setQueryData(["items", itemId], updatedItem);
      await queryClient.invalidateQueries({ queryKey: ["items"] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteItem(itemId!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["items"] });
      navigate("/items");
    },
  });

  async function handleFileSelection(file: File | undefined) {
    if (!file || !itemId) {
      return;
    }

    setUploadError(null);
    setIsUploading(true);

    try {
      await uploadItemPhoto(itemId, file);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["items"] }),
        queryClient.invalidateQueries({ queryKey: ["items", itemId] }),
      ]);
    } catch (error) {
      setUploadError(error instanceof Error ? error.message : "The image could not be uploaded.");
    } finally {
      setIsUploading(false);
    }
  }

  if (!itemId) {
    return <p className="text-sm text-red-600">A valid item id is required.</p>;
  }

  if (itemQuery.isLoading) {
    return <p className="text-sm text-muted-foreground">Loading item details...</p>;
  }

  if (itemQuery.isError || !itemQuery.data) {
    return <p className="text-sm text-red-600">{itemQuery.error?.message ?? "Item not found."}</p>;
  }

  const item = itemQuery.data;

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] bg-white/80 px-8 py-8 shadow-panel backdrop-blur">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <div className="flex flex-wrap gap-2">
              <Chip label={itemStatusLabels[item.status]} tone="slate" />
              <Chip label={itemConditionLabels[item.condition]} tone="amber" />
            </div>
            <h2 className="mt-4 text-3xl font-bold text-slate-950">{item.title}</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              {item.brand} {item.model} · {item.category}
            </p>
          </div>

          <div className="grid gap-3 sm:grid-cols-3">
            <MetricCard label="Asking price" value={formatCurrency(item.askingPrice)} />
            <MetricCard label="Photos" value={`${item.photos.length}`} />
            <MetricCard label="Last updated" value={formatDateTime(item.updatedAtUtc)} />
          </div>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-[1.25fr_0.85fr]">
        <Card>
          <CardHeader>
            <CardTitle>Edit item</CardTitle>
            <CardDescription>
              Keep the inventory record accurate before you turn it into marketplace content.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {updateMutation.error ? (
              <p className="text-sm text-red-600">{updateMutation.error.message}</p>
            ) : null}

            <ItemEditorForm
              initialValues={{
                title: item.title,
                brand: item.brand,
                model: item.model,
                category: item.category,
                condition: item.condition,
                description: item.description,
                askingPrice: item.askingPrice,
                status: item.status,
              }}
              isSubmitting={updateMutation.isPending}
              onSubmit={(values) => updateMutation.mutate(values)}
              submitLabel="Save changes"
            />
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Photos</CardTitle>
              <CardDescription>
                Upload up to 8 JPG, PNG, or WEBP images. The first image becomes the primary photo automatically.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <label className="flex cursor-pointer flex-col items-center justify-center rounded-3xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center transition hover:border-slate-400 hover:bg-slate-100">
                <ImagePlus className="h-8 w-8 text-slate-500" />
                <span className="mt-3 text-sm font-semibold text-slate-900">
                  {isUploading ? "Uploading..." : "Choose an image to upload"}
                </span>
                <span className="mt-1 text-xs text-muted-foreground">
                  Max 5 MB · JPG, PNG, WEBP
                </span>
                <input
                  className="sr-only"
                  disabled={isUploading}
                  onChange={(event) => {
                    const file = event.target.files?.[0];
                    void handleFileSelection(file);
                    event.currentTarget.value = "";
                  }}
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                />
              </label>

              {uploadError ? <p className="text-sm text-red-600">{uploadError}</p> : null}

              {item.photos.length === 0 ? (
                <div className="rounded-3xl bg-slate-50 px-5 py-10 text-center">
                  <Camera className="mx-auto h-8 w-8 text-slate-400" />
                  <p className="mt-3 text-sm text-muted-foreground">
                    No photos uploaded yet.
                  </p>
                </div>
              ) : (
                <div className="grid grid-cols-2 gap-4">
                  {item.photos.map((photo) => {
                    const imageUrl = getItemPhotoUrl(photo.relativePath);

                    return (
                      <div key={photo.id} className="overflow-hidden rounded-3xl border border-slate-200 bg-white">
                        <div className="aspect-[4/3] bg-slate-100">
                          {imageUrl ? (
                            <img alt={photo.fileName} className="h-full w-full object-cover" src={imageUrl} />
                          ) : null}
                        </div>
                        <div className="space-y-1 px-4 py-3 text-sm">
                          <div className="flex items-center justify-between gap-2">
                            <p className="truncate font-medium text-slate-900">{photo.fileName}</p>
                            {photo.isPrimary ? (
                              <span className="rounded-full bg-amber-100 px-2 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] text-amber-800">
                                Primary
                              </span>
                            ) : null}
                          </div>
                          <p className="text-xs text-muted-foreground">
                            {formatBytes(photo.fileSizeBytes)}
                          </p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </CardContent>
          </Card>

          <Card className="border-red-100">
            <CardHeader>
              <CardTitle>Delete item</CardTitle>
              <CardDescription>
                Remove this item and its stored photo references from your workspace.
              </CardDescription>
            </CardHeader>
            <CardContent>
              {deleteMutation.error ? (
                <p className="mb-3 text-sm text-red-600">{deleteMutation.error.message}</p>
              ) : null}

              <Button
                disabled={deleteMutation.isPending}
                onClick={() => {
                  if (window.confirm("Delete this item? This cannot be undone.")) {
                    deleteMutation.mutate();
                  }
                }}
                variant="outline"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                {deleteMutation.isPending ? "Deleting..." : "Delete item"}
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-3xl bg-slate-950 px-5 py-4 text-white">
      <p className="text-xs uppercase tracking-[0.18em] text-slate-400">{label}</p>
      <p className="mt-2 text-lg font-semibold">{value}</p>
    </div>
  );
}

function Chip({ label, tone }: { label: string; tone: "slate" | "amber" }) {
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

function formatBytes(value: number) {
  if (value < 1024 * 1024) {
    return `${Math.max(1, Math.round(value / 1024))} KB`;
  }

  return `${(value / (1024 * 1024)).toFixed(1)} MB`;
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}
