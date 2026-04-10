import { apiRequest, resolveApiAssetUrl } from "@/lib/api-client";
import type { ItemDetail, ItemPhoto, ItemSummary, SaveItemPayload } from "@/features/items/types";

export function getItems() {
  return apiRequest<ItemSummary[]>("/items", {
    method: "GET",
    authenticated: true,
  });
}

export function getItem(itemId: string) {
  return apiRequest<ItemDetail>(`/items/${itemId}`, {
    method: "GET",
    authenticated: true,
  });
}

export function createItem(payload: SaveItemPayload) {
  return apiRequest<ItemDetail>("/items", {
    method: "POST",
    body: JSON.stringify(payload),
    authenticated: true,
  });
}

export function updateItem(itemId: string, payload: SaveItemPayload) {
  return apiRequest<ItemDetail>(`/items/${itemId}`, {
    method: "PUT",
    body: JSON.stringify(payload),
    authenticated: true,
  });
}

export function deleteItem(itemId: string) {
  return apiRequest<void>(`/items/${itemId}`, {
    method: "DELETE",
    authenticated: true,
  });
}

export function uploadItemPhoto(itemId: string, file: File) {
  const formData = new FormData();
  formData.set("file", file);

  return apiRequest<ItemPhoto>(`/items/${itemId}/photos`, {
    method: "POST",
    body: formData,
    authenticated: true,
  });
}

export function getItemPhotoUrl(relativePath: string | null | undefined) {
  return resolveApiAssetUrl(relativePath);
}
