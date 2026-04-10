export const itemConditions = [
  "New",
  "LikeNew",
  "Good",
  "Fair",
  "Poor",
  "ForParts",
] as const;

export const itemStatuses = [
  "Draft",
  "ReadyToList",
  "Listed",
  "Sold",
  "Archived",
] as const;

export type ItemCondition = (typeof itemConditions)[number];
export type ItemStatus = (typeof itemStatuses)[number];

export type ItemPhoto = {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  sortOrder: number;
  isPrimary: boolean;
  relativePath: string;
  createdAtUtc: string;
};

export type ItemSummary = {
  id: string;
  title: string;
  brand: string;
  model: string;
  category: string;
  condition: ItemCondition;
  askingPrice: number;
  status: ItemStatus;
  photoCount: number;
  primaryPhotoRelativePath: string | null;
  updatedAtUtc: string;
};

export type ItemDetail = {
  id: string;
  title: string;
  brand: string;
  model: string;
  category: string;
  condition: ItemCondition;
  description: string;
  askingPrice: number;
  status: ItemStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
  photos: ItemPhoto[];
};

export type SaveItemPayload = {
  title: string;
  brand: string;
  model: string;
  category: string;
  condition: ItemCondition;
  description: string;
  askingPrice: number;
  status: ItemStatus;
};

export const itemConditionLabels: Record<ItemCondition, string> = {
  New: "New",
  LikeNew: "Like New",
  Good: "Good",
  Fair: "Fair",
  Poor: "Poor",
  ForParts: "For Parts",
};

export const itemStatusLabels: Record<ItemStatus, string> = {
  Draft: "Draft",
  ReadyToList: "Ready To List",
  Listed: "Listed",
  Sold: "Sold",
  Archived: "Archived",
};
