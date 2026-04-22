"use client";

import { FormEvent, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/entities/user";
import { accountApi, getAccessToken } from "@/shared/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";

export default function ResetPasswordPage() {
  const router = useRouter();
  const { loadProfile } = useAuthStore();
  const [isGuardChecking, setIsGuardChecking] = useState(true);
  const [email, setEmail] = useState("");
  const [token, setToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
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

      const accessToken = await getAccessToken();
      if (accessToken) {
        await loadProfile();
        if (!disposed && useAuthStore.getState().isAuthenticated) {
          router.replace("/profile");
          return;
        }
      }

      const params = new URLSearchParams(window.location.search);
      setEmail(params.get("email") ?? "");
      setToken(params.get("token") ?? "");

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

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setSuccessMessage(null);

    if (!email || !token) {
      setError("Invalid reset link.");
      return;
    }

    if (!newPassword || !confirmPassword) {
      setError("Fill in both password fields.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setIsSubmitting(true);
    try {
      const result = await accountApi.resetPassword({
        email,
        token,
        newPassword,
        confirmPassword,
      });
      setSuccessMessage(result.message || "Password has been reset successfully.");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Password reset failed.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container mx-auto flex min-h-[calc(100vh-8rem)] items-center justify-center px-4 py-8">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Reset Password</CardTitle>
          <CardDescription>Set a new password for your account.</CardDescription>
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
              <Label htmlFor="new-password">New password</Label>
              <Input
                id="new-password"
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirm-password">Confirm new password</Label>
              <Input
                id="confirm-password"
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>
          </CardContent>
          <CardFooter className="flex flex-col gap-3">
            <Button type="submit" className="w-full" disabled={isSubmitting}>
              {isSubmitting ? "Saving..." : "Reset Password"}
            </Button>
            <Link href="/login" className="w-full">
              <Button type="button" variant="outline" className="w-full">
                Back to Login
              </Button>
            </Link>
          </CardFooter>
        </form>
      </Card>
    </div>
  );
}
