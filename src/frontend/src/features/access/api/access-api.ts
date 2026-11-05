import { apiRequest } from "@/lib/api-client";

export type AccessStatus = {
  required: boolean;
  authorized: boolean;
};

export function getAccessStatus() {
  return apiRequest<AccessStatus>("/access", { method: "GET" });
}

export function submitPasscode(passcode: string) {
  return apiRequest<void>("/access", {
    method: "POST",
    body: JSON.stringify({ passcode }),
  });
}
