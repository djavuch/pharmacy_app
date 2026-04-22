"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/entities/user";
import { accountApi, getAccessToken } from "@/shared/api";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

type ConfirmStatus = "loading" | "success" | "error";

export default function ConfirmEmailPage() {
  const router = useRouter();
  const { loadProfile } = useAuthStore();
  const [status, setStatus] = useState<ConfirmStatus>("loading");
  const [message, setMessage] = useState("Confirming your email...");

  useEffect(() => {
    let disposed = false;

    const confirm = async () => {
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
      const userId = params.get("userId");
      const token = params.get("token");

      if (!userId || !token) {
        if (disposed) {
          return;
        }
        setStatus("error");
        setMessage("Invalid confirmation link.");
        return;
      }

      try {
        const result = await accountApi.confirmEmail(userId, token);
        if (disposed) {
          return;
        }
        setStatus("success");
        setMessage(result.message || "Email confirmed successfully.");
      } catch (err) {
        if (disposed) {
          return;
        }
        setStatus("error");
        setMessage(err instanceof Error ? err.message : "Email confirmation failed.");
      }
    };

    void confirm();

    return () => {
      disposed = true;
    };
  }, [loadProfile, router]);

  return (
    <div className="container mx-auto flex min-h-[calc(100vh-8rem)] items-center justify-center px-4 py-8">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Email Confirmation</CardTitle>
          <CardDescription>Verification status for your account.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert variant={status === "error" ? "destructive" : "default"}>
            <AlertDescription>{message}</AlertDescription>
          </Alert>

          <div className="flex gap-3">
            <Link href="/login" className="flex-1">
              <Button className="w-full">Go to Login</Button>
            </Link>
            <Link href="/" className="flex-1">
              <Button variant="outline" className="w-full">
                Home
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
