import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import type { FormEvent, ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getAccessStatus, submitPasscode } from "@/features/access/api/access-api";
import { PASSCODE_REQUIRED_EVENT } from "@/lib/api-client";

const accessKey = ["access"] as const;

/**
 * Shows the passcode screen only when the API says a passcode is configured and this
 * browser has not entered it yet. With no passcode set it renders the app straight away.
 */
export function PasscodeGate({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const [passcode, setPasscode] = useState("");

  const status = useQuery({ queryKey: accessKey, queryFn: getAccessStatus, staleTime: 0 });

  // If the cookie expires or is cleared while the app is open, any API call that gets a
  // "passcode_required" answer re-checks the status and brings the passcode screen back.
  useEffect(() => {
    const recheck = () => void queryClient.invalidateQueries({ queryKey: accessKey });
    window.addEventListener(PASSCODE_REQUIRED_EVENT, recheck);
    return () => window.removeEventListener(PASSCODE_REQUIRED_EVENT, recheck);
  }, [queryClient]);

  const unlock = useMutation({
    mutationFn: submitPasscode,
    onSuccess: async () => {
      setPasscode("");
      await queryClient.invalidateQueries();
    },
  });

  if (status.isPending) {
    return null;
  }

  if (status.isError) {
    return (
      <main className="flex min-h-screen items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle>Cannot reach FlipFlow</CardTitle>
            <CardDescription>{status.error.message}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button className="w-full" onClick={() => void status.refetch()}>
              Try again
            </Button>
          </CardContent>
        </Card>
      </main>
    );
  }

  if (!status.data.required || status.data.authorized) {
    return <>{children}</>;
  }

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    unlock.mutate(passcode);
  };

  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-500">FlipFlow</p>
          <CardTitle>Enter passcode</CardTitle>
          <CardDescription>This FlipFlow is protected. You only need to do this once per device.</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="space-y-4" onSubmit={onSubmit}>
            <div className="space-y-2">
              <Label htmlFor="passcode">Passcode</Label>
              <Input
                autoFocus
                autoComplete="current-password"
                id="passcode"
                type="password"
                value={passcode}
                onChange={(event) => setPasscode(event.target.value)}
              />
            </div>

            {unlock.error ? <p className="text-sm text-red-600">{unlock.error.message}</p> : null}

            <Button className="w-full" disabled={unlock.isPending || passcode.length === 0} type="submit">
              {unlock.isPending ? "Checking..." : "Unlock"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </main>
  );
}
