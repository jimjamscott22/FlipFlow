import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { z } from "zod";
import { login } from "@/features/auth/api/auth-api";
import { useAuth } from "@/features/auth/hooks/use-auth";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const schema = z.object({
  email: z.string().email("Enter a valid email."),
  password: z.string().min(8, "Password must be at least 8 characters."),
});

type FormValues = z.infer<typeof schema>;

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login: storeLogin } = useAuth();
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      email: "demo@flipflow.local",
      password: "FlipFlow123",
    },
  });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (response) => {
      storeLogin(response);
      navigate(location.state?.from?.pathname ?? "/dashboard", { replace: true });
    },
  });

  return (
    <Card className="w-full max-w-xl">
      <CardHeader>
        <CardTitle>Sign in</CardTitle>
        <CardDescription>
          Use the demo account or create your own seller workspace.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form
          className="space-y-5"
          onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
        >
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" {...form.register("email")} />
            <p className="text-sm text-red-600">{form.formState.errors.email?.message}</p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input id="password" type="password" {...form.register("password")} />
            <p className="text-sm text-red-600">{form.formState.errors.password?.message}</p>
          </div>

          {mutation.error ? (
            <p className="text-sm text-red-600">{mutation.error.message}</p>
          ) : null}

          <Button className="w-full" type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Signing in..." : "Sign in"}
          </Button>

          <p className="text-center text-sm text-muted-foreground">
            New to FlipFlow?{" "}
            <Link className="font-semibold text-primary" to="/register">
              Create an account
            </Link>
          </p>
        </form>
      </CardContent>
    </Card>
  );
}
