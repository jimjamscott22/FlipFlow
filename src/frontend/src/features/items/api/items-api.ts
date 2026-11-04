import { apiRequest, resolveApiAssetUrl } from "@/lib/api-client";
import type { ItemDetail, ItemPhoto, ItemSummary, SaveItemPayload } from "@/features/items/types";

export function getItems() {
  return apiRequest<ItemSummary[]>("/items", {
    method: "GET",
  });
}

export function getItem(itemId: string) {
  return apiRequest<ItemDetail>(`/items/${itemId}`, {
    method: "GET",
  });
}

export function createItem(payload: SaveItemPayload) {
  return apiRequest<ItemDetail>("/items", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function updateItem(itemId: string, payload: SaveItemPayload) {
  return apiRequest<ItemDetail>(`/items/${itemId}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function deleteItem(itemId: string) {
  return apiRequest<void>(`/items/${itemId}`, {
    method: "DELETE",
  });
}

export function uploadItemPhoto(itemId: string, file: File) {
  const formData = new FormData();
  formData.set("file", file);

  return apiRequest<ItemPhoto>(`/items/${itemId}/photos`, {
    method: "POST",
    body: formData,
  });
}

export function getItemPhotoUrl(relativePath: string | null | undefined) {
  return resolveApiAssetUrl(relativePath);
}
