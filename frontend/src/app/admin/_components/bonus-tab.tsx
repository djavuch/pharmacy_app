"use client";

import { useEffect, useState } from "react";
import { Search } from "lucide-react";
import { adminBonusApi, adminUserApi } from "@/shared/api";
import type { AdminUserDto, BonusSettingsDto, BonusTransactionDto } from "@/shared/types";
import { TabsContent } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { userSearchByOptions, type UserSearchBy } from "./admin-constants";

const transactionPageSize = 20;

type BonusSettingsFormState = {
  earningRate: string;
  minOrderAmountToEarn: string;
  maxRedeemPercent: string;
  isEarningEnabled: boolean;
  isRedemptionEnabled: boolean;
};

function settingsToFormState(settings: BonusSettingsDto): BonusSettingsFormState {
  return {
    earningRate: String(settings.earningRate),
    minOrderAmountToEarn: String(settings.minOrderAmountToEarn),
    maxRedeemPercent: String(settings.maxRedeemPercent),
    isEarningEnabled: settings.isEarningEnabled,
    isRedemptionEnabled: settings.isRedemptionEnabled,
  };
}

function getTransactionBadgeClass(type: BonusTransactionDto["type"]): string {
  if (type === "Earned") return "bg-emerald-100 text-emerald-800";
  if (type === "Redeemed") return "bg-amber-100 text-amber-800";
  if (type === "Refunded") return "bg-blue-100 text-blue-800";
  return "bg-slate-100 text-slate-800";
}

const userSearchByLabels: Record<UserSearchBy, string> = {
  Email: "Email",
  UserName: "Username",
  FirstName: "First name",
  LastName: "Last name",
};

export function BonusTab() {
  const [settings, setSettings] = useState<BonusSettingsDto | null>(null);
  const [settingsForm, setSettingsForm] = useState<BonusSettingsFormState | null>(null);
  const [settingsLoading, setSettingsLoading] = useState(true);
  const [settingsError, setSettingsError] = useState<string | null>(null);
  const [settingsSaving, setSettingsSaving] = useState(false);

  const [userSearch, setUserSearch] = useState("");
  const [userSearchBy, setUserSearchBy] = useState<UserSearchBy>("Email");
  const [users, setUsers] = useState<AdminUserDto[]>([]);
  const [usersLoading, setUsersLoading] = useState(false);
  const [usersError, setUsersError] = useState<string | null>(null);
  const [selectedUser, setSelectedUser] = useState<AdminUserDto | null>(null);

  const [accountBalance, setAccountBalance] = useState<number | null>(null);
  const [accountUpdatedAt, setAccountUpdatedAt] = useState<string | null>(null);
  const [accountLoading, setAccountLoading] = useState(false);
  const [accountError, setAccountError] = useState<string | null>(null);

  const [transactions, setTransactions] = useState<BonusTransactionDto[]>([]);
  const [transactionsLoading, setTransactionsLoading] = useState(false);
  const [transactionsError, setTransactionsError] = useState<string | null>(null);
  const [transactionsPage, setTransactionsPage] = useState(1);
  const [transactionsHasNext, setTransactionsHasNext] = useState(false);

  const [adjustPoints, setAdjustPoints] = useState("");
  const [adjustReason, setAdjustReason] = useState("");
  const [adjusting, setAdjusting] = useState(false);

  useEffect(() => {
    const fetchSettings = async () => {
      setSettingsLoading(true);
      setSettingsError(null);
      try {
        const response = await adminBonusApi.getSettings();
        setSettings(response);
        setSettingsForm(settingsToFormState(response));
      } catch (err) {
        setSettings(null);
        setSettingsForm(null);
        setSettingsError(err instanceof Error ? err.message : "Failed to load bonus settings.");
      } finally {
        setSettingsLoading(false);
      }
    };

    void fetchSettings();
  }, []);

  const fetchUsers = async () => {
    setUsersLoading(true);
    setUsersError(null);
    try {
      const query = userSearch.trim();
      const response = await adminUserApi.getAll({
        pageIndex: 1,
        pageSize: 20,
        filterOn: query ? userSearchBy : undefined,
        filterQuery: query || undefined,
        sortBy: "CreatedAt",
        isAscending: false,
      });

      setUsers(response.data?.items ?? []);
    } catch (err) {
      setUsers([]);
      setUsersError(err instanceof Error ? err.message : "Failed to load users.");
    } finally {
      setUsersLoading(false);
    }
  };

  useEffect(() => {
    void fetchUsers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchAccountAndTransactions = async (userId: string, page = 1) => {
    setAccountLoading(true);
    setTransactionsLoading(true);
    setAccountError(null);
    setTransactionsError(null);
    try {
      const [account, transactionItems] = await Promise.all([
        adminBonusApi.getAccount(userId),
        adminBonusApi.getTransactions(userId, page, transactionPageSize),
      ]);

      setAccountBalance(account.balance);
      setAccountUpdatedAt(account.updatedAt);
      setTransactions(transactionItems);
      setTransactionsPage(page);
      setTransactionsHasNext(transactionItems.length === transactionPageSize);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Failed to load bonus account data.";
      setAccountBalance(null);
      setAccountUpdatedAt(null);
      setTransactions([]);
      setTransactionsPage(page);
      setTransactionsHasNext(false);
      setAccountError(message);
      setTransactionsError(message);
    } finally {
      setAccountLoading(false);
      setTransactionsLoading(false);
    }
  };

  useEffect(() => {
    if (!selectedUser) {
      setAccountBalance(null);
      setAccountUpdatedAt(null);
      setTransactions([]);
      setTransactionsPage(1);
      setTransactionsHasNext(false);
      setAccountError(null);
      setTransactionsError(null);
      return;
    }

    void fetchAccountAndTransactions(selectedUser.id, 1);
  }, [selectedUser]);

  const handleSaveSettings = async () => {
    if (!settingsForm) {
      return;
    }

    const earningRate = Number.parseFloat(settingsForm.earningRate);
    const minOrderAmountToEarn = Number.parseFloat(settingsForm.minOrderAmountToEarn);
    const maxRedeemPercent = Number.parseFloat(settingsForm.maxRedeemPercent);

    if (!Number.isFinite(earningRate) || earningRate <= 0) {
      setSettingsError("Earning rate must be greater than 0.");
      return;
    }
    if (!Number.isFinite(minOrderAmountToEarn) || minOrderAmountToEarn < 0) {
      setSettingsError("Minimum order amount to earn cannot be negative.");
      return;
    }
    if (!Number.isFinite(maxRedeemPercent) || maxRedeemPercent < 1 || maxRedeemPercent > 100) {
      setSettingsError("Max redeem percent must be between 1 and 100.");
      return;
    }

    setSettingsSaving(true);
    setSettingsError(null);
    try {
      const updated = await adminBonusApi.updateSettings({
        earningRate,
        minOrderAmountToEarn,
        maxRedeemPercent,
        isEarningEnabled: settingsForm.isEarningEnabled,
        isRedemptionEnabled: settingsForm.isRedemptionEnabled,
      });

      setSettings(updated);
      setSettingsForm(settingsToFormState(updated));
    } catch (err) {
      setSettingsError(err instanceof Error ? err.message : "Failed to update bonus settings.");
    } finally {
      setSettingsSaving(false);
    }
  };

  const handleAdjustBonus = async () => {
    if (!selectedUser) {
      setAccountError("Select a user first.");
      return;
    }

    const points = Number.parseFloat(adjustPoints);
    if (!Number.isFinite(points) || points === 0) {
      setAccountError("Points must be a non-zero number.");
      return;
    }

    const reason = adjustReason.trim();
    if (!reason) {
      setAccountError("Reason is required.");
      return;
    }

    setAdjusting(true);
    setAccountError(null);
    try {
      const updatedAccount = await adminBonusApi.adjust(selectedUser.id, { points, reason });
      setAccountBalance(updatedAccount.balance);
      setAccountUpdatedAt(updatedAccount.updatedAt);
      setAdjustPoints("");
      setAdjustReason("");
      await fetchAccountAndTransactions(selectedUser.id, 1);
    } catch (err) {
      setAccountError(err instanceof Error ? err.message : "Failed to adjust bonus points.");
    } finally {
      setAdjusting(false);
    }
  };

  return (
    <TabsContent value="bonuses">
      <div className="grid gap-4 xl:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Bonus Settings</CardTitle>
            <CardDescription>Configure earning and redemption behavior.</CardDescription>
          </CardHeader>
          <CardContent>
            {settingsLoading ? (
              <p className="py-6 text-center text-muted-foreground">Loading settings...</p>
            ) : settingsForm ? (
              <div className="space-y-4">
                {settingsError && (
                  <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
                    {settingsError}
                  </div>
                )}

                <div className="grid gap-4 md:grid-cols-3">
                  <div className="space-y-2">
                    <Label htmlFor="bonusEarningRate">Earning rate *</Label>
                    <Input
                      id="bonusEarningRate"
                      type="number"
                      min={0}
                      step="0.01"
                      value={settingsForm.earningRate}
                      onChange={(event) =>
                        setSettingsForm((current) =>
                          current ? { ...current, earningRate: event.target.value } : current,
                        )
                      }
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="bonusMinOrderAmount">Min order amount</Label>
                    <Input
                      id="bonusMinOrderAmount"
                      type="number"
                      min={0}
                      step="0.01"
                      value={settingsForm.minOrderAmountToEarn}
                      onChange={(event) =>
                        setSettingsForm((current) =>
                          current ? { ...current, minOrderAmountToEarn: event.target.value } : current,
                        )
                      }
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="bonusMaxRedeemPercent">Max redeem percent *</Label>
                    <Input
                      id="bonusMaxRedeemPercent"
                      type="number"
                      min={1}
                      max={100}
                      step="0.01"
                      value={settingsForm.maxRedeemPercent}
                      onChange={(event) =>
                        setSettingsForm((current) =>
                          current ? { ...current, maxRedeemPercent: event.target.value } : current,
                        )
                      }
                    />
                  </div>
                </div>

                <div className="flex flex-col gap-2">
                  <label className="flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={settingsForm.isEarningEnabled}
                      onChange={(event) =>
                        setSettingsForm((current) =>
                          current ? { ...current, isEarningEnabled: event.target.checked } : current,
                        )
                      }
                      className="h-4 w-4 rounded border-input"
                    />
                    Enable earning points
                  </label>

                  <label className="flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={settingsForm.isRedemptionEnabled}
                      onChange={(event) =>
                        setSettingsForm((current) =>
                          current ? { ...current, isRedemptionEnabled: event.target.checked } : current,
                        )
                      }
                      className="h-4 w-4 rounded border-input"
                    />
                    Enable redeeming points
                  </label>
                </div>

                <div className="flex items-center justify-between">
                  <p className="text-xs text-muted-foreground">
                    Last update: {settings ? new Date(settings.updatedAt).toLocaleString("en-US") : "Unknown"}
                  </p>
                  <Button onClick={() => void handleSaveSettings()} disabled={settingsSaving}>
                    {settingsSaving ? "Saving..." : "Save settings"}
                  </Button>
                </div>
              </div>
            ) : (
              <p className="py-6 text-center text-muted-foreground">
                Could not load settings.
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>User Bonus Accounts</CardTitle>
            <CardDescription>Select a user and manage their bonus points.</CardDescription>
          </CardHeader>
          <CardContent>
            <form
              className="grid gap-3 md:grid-cols-3"
              onSubmit={(event) => {
                event.preventDefault();
                void fetchUsers();
              }}
            >
              <Input
                value={userSearch}
                placeholder="Search users..."
                onChange={(event) => setUserSearch(event.target.value)}
              />

              <Select
                value={userSearchBy}
                onValueChange={(value) => {
                  if (!value) return;
                  setUserSearchBy(value as UserSearchBy);
                }}
              >
                <SelectTrigger>
                  <SelectValue>
                    {(value) => (value ? userSearchByLabels[value as UserSearchBy] ?? value : "Search by")}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {userSearchByOptions.map((option) => (
                    <SelectItem key={option} value={option}>
                      {userSearchByLabels[option]}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Button type="submit" disabled={usersLoading}>
                <Search className="mr-2 h-4 w-4" />
                {usersLoading ? "Searching..." : "Find users"}
              </Button>
            </form>

            {usersError && (
              <div className="mt-4 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
                {usersError}
              </div>
            )}

            <div className="mt-4 overflow-x-auto">
              {usersLoading ? (
                <p className="py-6 text-center text-muted-foreground">Loading users...</p>
              ) : users.length > 0 ? (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>User</TableHead>
                      <TableHead>Role</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead className="text-right">Action</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {users.map((user) => (
                      <TableRow
                        key={user.id}
                        className={selectedUser?.id === user.id ? "bg-muted/50" : undefined}
                      >
                        <TableCell>
                          <div className="font-medium">{user.email}</div>
                          <div className="text-xs text-muted-foreground">
                            @{user.userName}
                          </div>
                        </TableCell>
                        <TableCell>{user.role || "Customer"}</TableCell>
                        <TableCell>
                          <Badge className={user.isLockedOut ? "bg-rose-100 text-rose-800" : "bg-emerald-100 text-emerald-800"}>
                            {user.isLockedOut ? "Locked" : "Active"}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setSelectedUser(user)}
                          >
                            {selectedUser?.id === user.id ? "Selected" : "Select"}
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <p className="py-6 text-center text-muted-foreground">
                  No users found.
                </p>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      <Card className="mt-4">
        <CardHeader>
          <CardTitle>Selected User Bonus Details</CardTitle>
          <CardDescription>
            {selectedUser
              ? `${selectedUser.email} (${selectedUser.id})`
              : "Select a user above to inspect and manage bonus points."}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {!selectedUser ? (
            <p className="py-6 text-center text-muted-foreground">
              Choose a user to view bonus account and transaction history.
            </p>
          ) : (
            <div className="space-y-4">
              {(accountError || transactionsError) && (
                <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
                  {accountError || transactionsError}
                </div>
              )}

              <div className="grid gap-4 md:grid-cols-3">
                <div className="rounded-md border p-3">
                  <p className="text-xs text-muted-foreground">Current balance</p>
                  <p className="text-xl font-semibold">
                    {accountLoading ? "..." : accountBalance != null ? accountBalance.toFixed(2) : "-"}
                  </p>
                </div>
                <div className="rounded-md border p-3">
                  <p className="text-xs text-muted-foreground">Last account update</p>
                  <p className="text-sm font-medium">
                    {accountUpdatedAt ? new Date(accountUpdatedAt).toLocaleString("en-US") : "-"}
                  </p>
                </div>
                <div className="rounded-md border p-3">
                  <p className="text-xs text-muted-foreground">Transactions page</p>
                  <p className="text-sm font-medium">
                    {transactionsPage}
                  </p>
                </div>
              </div>

              <div className="grid gap-4 md:grid-cols-[180px_1fr_auto]">
                <div className="space-y-2">
                  <Label htmlFor="adjustPoints">Adjust points</Label>
                  <Input
                    id="adjustPoints"
                    type="number"
                    step="0.01"
                    value={adjustPoints}
                    onChange={(event) => setAdjustPoints(event.target.value)}
                    placeholder="e.g. 15 or -10"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="adjustReason">Reason</Label>
                  <Textarea
                    id="adjustReason"
                    value={adjustReason}
                    onChange={(event) => setAdjustReason(event.target.value)}
                    placeholder="Admin adjustment reason"
                  />
                </div>

                <div className="flex items-end">
                  <Button
                    onClick={() => void handleAdjustBonus()}
                    disabled={adjusting || accountLoading}
                  >
                    {adjusting ? "Applying..." : "Apply adjustment"}
                  </Button>
                </div>
              </div>

              {transactionsLoading ? (
                <p className="py-6 text-center text-muted-foreground">Loading transactions...</p>
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
                            <TableCell className={transaction.points >= 0 ? "text-emerald-700" : "text-rose-700"}>
                              {transaction.points >= 0 ? "+" : ""}
                              {transaction.points}
                            </TableCell>
                            <TableCell>{transaction.orderId ?? "-"}</TableCell>
                            <TableCell>{transaction.description || "-"}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>

                  {transactions.length === 0 && (
                    <p className="py-6 text-center text-muted-foreground">
                      No transactions for this user yet.
                    </p>
                  )}

                  <div className="flex items-center justify-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={transactionsPage <= 1 || transactionsLoading || !selectedUser}
                      onClick={() => {
                        if (!selectedUser || transactionsPage <= 1) return;
                        void fetchAccountAndTransactions(selectedUser.id, transactionsPage - 1);
                      }}
                    >
                      Previous
                    </Button>
                    <span className="text-sm text-muted-foreground">{transactionsPage}</span>
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={!transactionsHasNext || transactionsLoading || !selectedUser}
                      onClick={() => {
                        if (!selectedUser || !transactionsHasNext) return;
                        void fetchAccountAndTransactions(selectedUser.id, transactionsPage + 1);
                      }}
                    >
                      Next
                    </Button>
                  </div>
                </>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </TabsContent>
  );
}
