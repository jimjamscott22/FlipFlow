import { apiRequest } from "@/lib/api-client";

export type AuthUser = {
  id: string;
  displayName: string;
  email: string;
};

export type AuthResponse = {
  user: AuthUser;
  token: {
    accessToken: string;
    expiresAtUtc: string;
  };
};

export type LoginPayload = {
  email: string;
  password: string;
};

export type RegisterPayload = {
  displayName: string;
  email: string;
  password: string;
};

export function login(payload: LoginPayload) {
  return apiRequest<AuthResponse>("/auth/login", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function register(payload: RegisterPayload) {
  return apiRequest<AuthResponse>("/auth/register", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function getCurrentUser() {
  return apiRequest<AuthUser>("/auth/me", {
    method: "GET",
    authenticated: true,
  });
}
