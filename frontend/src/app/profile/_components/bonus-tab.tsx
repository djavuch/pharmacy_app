"use client";

import { useEffect, useState } from "react";
import { Gift, RefreshCw, Wallet } from "lucide-react";
import { bonusApi } from "@/shared/api";
import type {
  BonusAccountDto,
  BonusSettingsDto,
  BonusTransactionDto,
} from "@/shared/types";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TabsContent } from "@/components/ui/tabs";

const transactionPageSize = 10;

function getErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback;
}

function formatMoney(value: number): string {
  return `$${value.toFixed(2)}`;
}

function formatPoints(value: number): string {
  return value.toFixed(2);
}

function formatEarningRate(value: number): string {
  return `${value.toFixed(2)} pts / $1`;
}

function getTransactionBadgeClass(type: BonusTransactionDto["type"]): string {
  if (type === "Earned") return "bg-emerald-100 text-emerald-800";
  if (type === "Redeemed") return "bg-amber-100 text-amber-800";
  if (type === "Refunded") return "bg-blue-100 text-blue-800";
  return "bg-slate-100 text-slate-800";
}

export function BonusTab() {
  const [account, setAccount] = useState<BonusAccountDto | null>(null);
  const [settings, setSettings] = useState<BonusSettingsDto | null>(null);
  const [summaryLoading, setSummaryLoading] = useState(true);
  const [summaryError, setSummaryError] = useState<string | null>(null);

  const [transactions, setTransactions] = useState<BonusTransactionDto[]>([]);
  const [transactionsLoading, setTransactionsLoading] = useState(true);
  const [transactionsError, setTransactionsError] = useState<string | null>(null);
  const [transactionsPage, setTransactionsPage] = useState(1);
  const [transactionsTotalPages, setTransactionsTotalPages] = useState(1);
  const [transactionsTotalCount, setTransactionsTotalCount] = useState(0);

  const loadSummary = async () => {
    setSummaryLoading(true);
    setSummaryError(null);

    const [accountResult, settingsResult] = await Promise.allSettled([
      bonusApi.getAccount(),
      bonusApi.getSettings(),
    ]);

    const errors: string[] = [];

    if (accountResult.status === "fulfilled") {
      setAccount(accountResult.value);
    } else {
      setAccount(null);
      errors.push(getErrorMessage(accountResult.reason, "Failed to load bonus balance."));
    }

    if (settingsResult.status === "fulfilled") {
      setSettings(settingsResult.value);
    } else {
      setSettings(null);
      errors.push(getErrorMessage(settingsResult.reason, "Failed to load bonus settings."));
    }

    setSummaryError(errors.length > 0 ? errors.join(" ") : null);
    setSummaryLoading(false);
  };

  const loadTransactions = async (page = 1) => {
    setTransactionsLoading(true);
    setTransactionsError(null);

    try {
      const response = await bonusApi.getTransactions({
        pageIndex: page,
        pageSize: transactionPageSize,
      });

      setTransactions(response.items ?? []);
      setTransactionsPage(response.pageIndex ?? page);
      setTransactionsTotalPages(response.totalPages ?? 1);
      setTransactionsTotalCount(response.totalCount ?? 0);
    } catch (error) {
      setTransactions([]);
      setTransactionsPage(page);
      setTransactionsTotalPages(1);
      setTransactionsTotalCount(0);
      setTransactionsError(getErrorMessage(error, "Failed to load bonus history."));
    } finally {
      setTransactionsLoading(false);
    }
  };

  const handleRefresh = async () => {
    await Promise.all([loadSummary(), loadTransactions(1)]);
  };

  useEffect(() => {
    void handleRefresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <TabsContent value="bonuses" className="space-y-5 pt-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-sm text-muted-foreground">
          Track your current balance, reward rules, and recent bonus activity.
        </p>
        <Button
          variant="outline"
          size="sm"
          onClick={() => void handleRefresh()}
          disabled={summaryLoading || transactionsLoading}
        >
          <RefreshCw className="mr-2 h-4 w-4" />
          {summaryLoading || transactionsLoading ? "Refreshing..." : "Refresh"}
        </Button>
      </div>

      {summaryError && (
        <Alert variant="destructive">
          <AlertTitle>Bonus summary unavailable</AlertTitle>
          <AlertDescription>{summaryError}</AlertDescription>
        </Alert>
      )}

      {transactionsError && (
        <Alert variant="destructive">
          <AlertTitle>Bonus history unavailable</AlertTitle>
          <AlertDescription>{transactionsError}</AlertDescription>
        </Alert>
      )}

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Card className="rounded-2xl border-border/70 bg-background/70">
          <CardContent className="flex items-start justify-between p-5">
            <div className="space-y-1">
              <p className="text-sm text-muted-foreground">Current balance</p>
              <p className="text-2xl font-semibold">
                {summaryLoading ? "Loading..." : account ? formatPoints(account.balance) : "-"}
              </p>
              <p className="text-xs text-muted-foreground">Available for checkout redemption</p>
            </div>
            <Wallet className="h-5 w-5 text-primary" />
          </CardContent>
        </Card>

        <Card className="rounded-2xl border-border/70 bg-background/70">
          <CardContent className="flex items-start justify-between p-5">
            <div className="space-y-1">
              <p className="text-sm text-muted-foreground">Earning rate</p>
              <p className="text-2xl font-semibold">
                {summaryLoading ? "Loading..." : settings ? formatEarningRate(settings.earningRate) : "-"}
              </p>
              <p className="text-xs text-muted-foreground">Bonus points earned per paid dollar</p>
            </div>
            <Gift className="h-5 w-5 text-primary" />
          </CardContent>
        </Card>

        <Card className="rounded-2xl border-border/70 bg-background/70">
          <CardContent className="p-5">
            <p className="text-sm text-muted-foreground">Min order to earn</p>
            <p className="mt-1 text-2xl font-semibold">
              {summaryLoading ? "Loading..." : settings ? formatMoney(settings.minOrderAmountToEarn) : "-"}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              Orders below this amount do not add points
            </p>
          </CardContent>
        </Card>

        <Card className="rounded-2xl border-border/70 bg-background/70">
          <CardContent className="p-5">
            <p className="text-sm text-muted-foreground">Max redeem per order</p>
            <p className="mt-1 text-2xl font-semibold">
              {summaryLoading ? "Loading..." : settings ? `${settings.maxRedeemPercent}%` : "-"}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              Upper limit applied during checkout
            </p>
          </CardContent>
        </Card>
      </div>

      <Card className="rounded-[1.6rem] border-border/70 bg-background/70">
        <CardHeader>
          <CardTitle>Bonus program details</CardTitle>
          <CardDescription>
            Current availability and the rules that apply to future orders.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-wrap gap-2">
            <Badge
              className={
                settings?.isEarningEnabled
                  ? "bg-emerald-100 text-emerald-800"
                  : "bg-slate-100 text-slate-800"
              }
            >
              {settings?.isEarningEnabled ? "Earning enabled" : "Earning paused"}
            </Badge>
            <Badge
              className={
                settings?.isRedemptionEnabled
                  ? "bg-emerald-100 text-emerald-800"
                  : "bg-slate-100 text-slate-800"
              }
            >
              {settings?.isRedemptionEnabled ? "Redemption enabled" : "Redemption paused"}
            </Badge>
          </div>

          <div className="rounded-2xl border border-border/70 bg-card/70 p-4 text-sm text-muted-foreground">
            Bonus points are earned after eligible purchases and can be redeemed during
            checkout when redemption is enabled.
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-2xl border border-border/70 bg-card/70 p-4">
              <p className="text-sm text-muted-foreground">Last balance update</p>
              <p className="mt-1 font-medium">
                {account?.updatedAt
                  ? new Date(account.updatedAt).toLocaleString("en-US")
                  : "Not available"}
              </p>
            </div>
            <div className="rounded-2xl border border-border/70 bg-card/70 p-4">
              <p className="text-sm text-muted-foreground">Program settings updated</p>
              <p className="mt-1 font-medium">
                {settings?.updatedAt
                  ? new Date(settings.updatedAt).toLocaleString("en-US")
                  : "Not available"}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="rounded-[1.6rem] border-border/70 bg-background/70">
        <CardHeader>
          <CardTitle>Bonus history</CardTitle>
          <CardDescription>
            {transactionsTotalCount > 0
              ? `${transactionsTotalCount} recorded bonus transaction${transactionsTotalCount === 1 ? "" : "s"}.`
              : "Your bonus activity will appear here after the first earn or redemption."}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {transactionsLoading ? (
            <p className="text-sm text-muted-foreground">Loading bonus history...</p>
          ) : transactions.length === 0 ? (
            <div className="rounded-2xl border border-border/70 bg-card/70 p-4">
              <p className="text-sm text-muted-foreground">
                No bonus transactions yet.
              </p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Date</TableHead>
                      <TableHead>Type</TableHead>
                      <TableHead>Points</TableHead>
                      <TableHead>Order</TableHead>
                      <TableHead>Description</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {transactions.map((transaction) => (
                      <TableRow key={transaction.id}>
                        <TableCell>
                          {new Date(transaction.createdAt).toLocaleString("en-US")}
                        </TableCell>
                        <TableCell>
                          <Badge className={getTransactionBadgeClass(transaction.type)}>
                            {transaction.type}
                          </Badge>
                        </TableCell>
                        <TableCell
                          className={
                            transaction.points >= 0 ? "text-emerald-700" : "text-rose-700"
                          }
                        >
                          {transaction.points >= 0 ? "+" : ""}
                          {formatPoints(transaction.points)}
                        </TableCell>
                        <TableCell>{transaction.orderId ?? "-"}</TableCell>
                        <TableCell>{transaction.description || "-"}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>

              {transactionsTotalPages > 1 && (
                <div className="flex items-center justify-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={transactionsPage <= 1 || transactionsLoading}
                    onClick={() => {
                      if (transactionsPage <= 1) {
                        return;
                      }

                      void loadTransactions(transactionsPage - 1);
                    }}
                  >
                    Previous
                  </Button>
                  <span className="text-sm text-muted-foreground">
                    {transactionsPage} / {transactionsTotalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={transactionsPage >= transactionsTotalPages || transactionsLoading}
                    onClick={() => {
                      if (transactionsPage >= transactionsTotalPages) {
                        return;
                      }

                      void loadTransactions(transactionsPage + 1);
                    }}
                  >
                    Next
                  </Button>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </TabsContent>
  );
}
