"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/entities/user";
import { accountApi, getAccessToken } from "@/shared/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Mail } from "lucide-react";

export default function ResendConfirmationPage() {
  const router = useRouter();
  const { loadProfile } = useAuthStore();
  const [isGuardChecking, setIsGuardChecking] = useState(true);
  const [email, setEmail] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let disposed = false;

    const initPage = async () => {
      if (useAuthStore.getState().isAuthenticated) {
        router.replace("/profile");
        return;
      }

      const token = await getAccessToken();
      if (token) {
        await loadProfile();
        if (!disposed && useAuthStore.getState().isAuthenticated) {
          router.replace("/profile");
          return;
        }
      }

      const params = new URLSearchParams(window.location.search);
      const emailFromQuery = params.get("email");
      if (emailFromQuery) {
        setEmail(emailFromQuery);
      }

      if (!disposed) {
        setIsGuardChecking(false);
      }
    };

    void initPage();

    return () => {
      disposed = true;
    };
  }, [loadProfile, router]);

  if (isGuardChecking) {
    return (
      <div className="container mx-auto flex min-h-[calc(100vh-8rem)] items-center justify-center px-4 py-8">
        <p className="text-sm text-muted-foreground">Loading...</p>
      </div>
    );
  }

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setSuccessMessage(null);

    const normalizedEmail = email.trim();
    if (!normalizedEmail) {
      setError("Email is required.");
      return;
    }

    setIsSubmitting(true);
    try {
      const result = await accountApi.resendConfirmation(normalizedEmail);
      setSuccessMessage(
        result.message ||
          "If an account with that email exists, a confirmation link has been sent."
      );
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to resend confirmation email."
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container mx-auto flex min-h-[calc(100vh-8rem)] items-center justify-center px-4 py-8">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="mb-2 flex justify-center">
            <div className="rounded-full bg-primary p-3">
              <Mail className="h-6 w-6 text-primary-foreground" />
            </div>
          </div>
          <CardTitle className="text-2xl">Resend Confirmation Email</CardTitle>
          <CardDescription>
            Enter your account email and we will send a new activation link.
          </CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit}>
          <CardContent className="space-y-4">
            {error && (
              <Alert variant="destructive">
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}
            {successMessage && (
              <Alert>
                <AlertDescription>{successMessage}</AlertDescription>
              </Alert>
            )}
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="name@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
          </CardContent>
          <CardFooter className="flex flex-col space-y-4">
            <Button type="submit" className="w-full" disabled={isSubmitting}>
              {isSubmitting ? "Sending..." : "Resend Email"}
            </Button>
            <p className="text-center text-sm text-muted-foreground">
              Back to{" "}
              <Link href="/login" className="font-medium text-primary hover:underline">
                Sign In
              </Link>
            </p>
          </CardFooter>
        </form>
      </Card>
    </div>
  );
}
