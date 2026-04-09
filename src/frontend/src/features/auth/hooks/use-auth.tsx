import {
  createContext,
  startTransition,
  useContext,
  useEffect,
  useState,
} from "react";
import type { ReactNode } from "react";
import { getAccessToken, setAccessToken } from "@/lib/api-client";
import { getCurrentUser, type AuthResponse, type AuthUser } from "@/features/auth/api/auth-api";

type AuthContextValue = {
  user: AuthUser | null;
  isBootstrapping: boolean;
  login: (response: AuthResponse) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isBootstrapping, setIsBootstrapping] = useState(true);

  useEffect(() => {
    const token = getAccessToken();

    if (!token) {
      setIsBootstrapping(false);
      return;
    }

    getCurrentUser()
      .then((currentUser) => {
        startTransition(() => {
          setUser(currentUser);
        });
      })
      .catch(() => {
        setAccessToken(null);
      })
      .finally(() => {
        setIsBootstrapping(false);
      });
  }, []);

  const value: AuthContextValue = {
    user,
    isBootstrapping,
    login(response) {
      setAccessToken(response.token.accessToken);
      setUser(response.user);
    },
    logout() {
      setAccessToken(null);
      setUser(null);
    },
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}
