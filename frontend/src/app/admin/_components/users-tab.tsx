import { useEffect, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Lock, LockOpen } from "lucide-react";
import { useAuthStore } from "@/entities/user";
import { adminUserApi } from "@/shared/api";
import type { AdminUserDto } from "@/shared/types";
import { TabsContent } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import {
  userRoles,
  userSearchByOptions,
  userSortOptions,
  type UserSearchBy,
  type UsersSort,
} from "./admin-constants";
import { buildUrlWithQuery, getCurrentUrl, parseEnumParam, parsePositiveIntParam } from "./query-state";
import { getUserRoleBadgeClass } from "./status-utils";

const userSearchByLabels: Record<UserSearchBy, string> = {
  Email: "Email",
  UserName: "Username",
  FirstName: "First name",
  LastName: "Last name",
};

const userSortLabels: Record<UsersSort, string> = {
  createdat_desc: "Newest first",
  createdat_asc: "Oldest first",
  email_asc: "Email A-Z",
  email_desc: "Email Z-A",
  username_asc: "Username A-Z",
  username_desc: "Username Z-A",
};

export function UsersTab({ isActive }: { isActive: boolean }) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const { profile } = useAuthStore();

  const [users, setUsers] = useState<AdminUserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState(() => searchParams.get("usersSearch") ?? "");
  const [searchBy, setSearchBy] = useState<UserSearchBy>(() =>
    parseEnumParam(searchParams.get("usersSearchBy"), userSearchByOptions, "Email"),
  );
  const [sort, setSort] = useState<UsersSort>(() =>
    parseEnumParam(searchParams.get("usersSort"), userSortOptions, "createdat_desc"),
  );

  const [page, setPage] = useState(() =>
    parsePositiveIntParam(searchParams.get("usersPage"), 1),
  );
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 15;

  const [roleDraftByUserId, setRoleDraftByUserId] = useState<Record<string, string>>({});
  const [roleSavingUserId, setRoleSavingUserId] = useState<string | null>(null);
  const [lockActionUserId, setLockActionUserId] = useState<string | null>(null);
  const [lockDialogUser, setLockDialogUser] = useState<AdminUserDto | null>(null);
  const [lockDuration, setLockDuration] = useState<"1d" | "7d" | "30d" | "90d" | "custom">("30d");
  const [customLockUntil, setCustomLockUntil] = useState("");

  useEffect(() => {
    if (!isActive) {
      return;
    }

    const nextUrl = buildUrlWithQuery(pathname, searchParams, {
      tab: "users",
      usersSearch: search || undefined,
      usersSearchBy: searchBy === "Email" ? undefined : searchBy,
      usersSort: sort === "createdat_desc" ? undefined : sort,
      usersPage: page > 1 ? page : undefined,
    });
    const currentUrl = getCurrentUrl(pathname, searchParams);

    if (nextUrl !== currentUrl) {
      router.replace(nextUrl, { scroll: false });
    }
  }, [isActive, page, pathname, router, search, searchBy, searchParams, sort]);

  const toDateTimeLocalValue = (date: Date): string => {
    const pad = (value: number) => String(value).padStart(2, "0");
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  };

  useEffect(() => {
    const fetchUsers = async () => {
      setLoading(true);
      setError(null);

      try {
        const [sortByKey, direction] = sort.split("_") as [string, "asc" | "desc"];
        const sortBy =
          sortByKey === "createdat"
            ? "CreatedAt"
            : sortByKey === "email"
              ? "Email"
              : "UserName";

        const result = await adminUserApi.getAll({
          pageIndex: page,
          pageSize,
          filterOn: search.trim() ? searchBy : undefined,
          filterQuery: search.trim() || undefined,
          sortBy,
          isAscending: direction === "asc",
        });

        const items = result.data?.items ?? [];
        setUsers(items);
        setTotalPages(result.data?.totalPages ?? 1);
        setRoleDraftByUserId((current) => {
          const next = { ...current };
          for (const user of items) {
            next[user.id] = user.role || "Customer";
          }
          return next;
        });
      } catch (err) {
        setUsers([]);
        setTotalPages(1);
        setError(err instanceof Error ? err.message : "Failed to load users");
      } finally {
        setLoading(false);
      }
    };

    void fetchUsers();
  }, [page, pageSize, search, searchBy, sort]);

  const handleRoleSave = async (user: AdminUserDto) => {
    const currentRole = user.role || "Customer";
    const nextRole = (roleDraftByUserId[user.id] || currentRole).trim();

    if (!nextRole) {
      setError("Role is required");
      return;
    }

    if (nextRole === currentRole) {
      return;
    }

    setRoleSavingUserId(user.id);
    setError(null);

    try {
      const response = await adminUserApi.changeRole(user.id, nextRole);
      const updatedUser = response.data ?? { ...user, role: nextRole };

      setUsers((current) =>
        current.map((entry) => (entry.id === user.id ? updatedUser : entry)),
      );
      setRoleDraftByUserId((current) => ({
        ...current,
        [user.id]: updatedUser.role || nextRole,
      }));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to change role");
    } finally {
      setRoleSavingUserId(null);
    }
  };

  const handleUnlock = async (user: AdminUserDto) => {
    setLockActionUserId(user.id);
    setError(null);

    try {
      const response = await adminUserApi.unlock(user.id);
      const updatedUser = response.data ?? {
        ...user,
        isLockedOut: false,
        lockoutEnd: undefined,
      };

      setUsers((current) =>
        current.map((entry) => (entry.id === user.id ? updatedUser : entry)),
      );
      setRoleDraftByUserId((current) => ({
        ...current,
        [user.id]: updatedUser.role || current[user.id] || "Customer",
      }));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update lock status");
    } finally {
      setLockActionUserId(null);
    }
  };

  const openLockDialog = (user: AdminUserDto) => {
    setLockDialogUser(user);
    setLockDuration("30d");
    setCustomLockUntil(
      toDateTimeLocalValue(new Date(Date.now() + 30 * 24 * 60 * 60 * 1000)),
    );
  };

  const resolveLockoutEndIso = (): string | null => {
    if (lockDuration === "custom") {
      const parsed = new Date(customLockUntil);
      if (Number.isNaN(parsed.getTime())) {
        return null;
      }
      if (parsed.getTime() <= Date.now()) {
        return null;
      }
      return parsed.toISOString();
    }

    const days = lockDuration === "1d" ? 1 : lockDuration === "7d" ? 7 : lockDuration === "90d" ? 90 : 30;
    return new Date(Date.now() + days * 24 * 60 * 60 * 1000).toISOString();
  };

  const handleConfirmLock = async () => {
    if (!lockDialogUser) return;

    const lockoutEnd = resolveLockoutEndIso();
    if (!lockoutEnd) {
      setError("Please select a valid lock period in the future.");
      return;
    }

    setLockActionUserId(lockDialogUser.id);
    setError(null);

    try {
      const response = await adminUserApi.lock(lockDialogUser.id, lockoutEnd);
      const updatedUser = response.data ?? {
        ...lockDialogUser,
        isLockedOut: true,
        lockoutEnd,
      };

      setUsers((current) =>
        current.map((entry) => (entry.id === lockDialogUser.id ? updatedUser : entry)),
      );
      setRoleDraftByUserId((current) => ({
        ...current,
        [lockDialogUser.id]: updatedUser.role || current[lockDialogUser.id] || "Customer",
      }));
      setLockDialogUser(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to lock user");
    } finally {
      setLockActionUserId(null);
    }
  };

  return (
    <TabsContent value="users">
      <Card>
        <CardHeader>
          <CardTitle>User Management</CardTitle>
          <CardDescription>Search users, manage roles, and lock or unlock accounts</CardDescription>

          <div className="mt-4 grid gap-3 md:grid-cols-3">
            <Input
              placeholder="Search users..."
              value={search}
              onChange={(event) => {
                setSearch(event.target.value);
                setPage(1);
              }}
            />

            <Select
              value={searchBy}
              onValueChange={(value) => {
                if (!value) return;
                setSearchBy(value as UserSearchBy);
                setPage(1);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Search by">
                  {(value) => (value ? userSearchByLabels[value as UserSearchBy] ?? value : "Search by")}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Email">Email</SelectItem>
                <SelectItem value="UserName">Username</SelectItem>
                <SelectItem value="FirstName">First name</SelectItem>
                <SelectItem value="LastName">Last name</SelectItem>
              </SelectContent>
            </Select>

            <Select
              value={sort}
              onValueChange={(value) => {
                if (!value) return;
                setSort(value as UsersSort);
                setPage(1);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Sort by">
                  {(value) => (value ? userSortLabels[value as UsersSort] ?? value : "Sort by")}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="createdat_desc">Newest first</SelectItem>
                <SelectItem value="createdat_asc">Oldest first</SelectItem>
                <SelectItem value="email_asc">Email A-Z</SelectItem>
                <SelectItem value="email_desc">Email Z-A</SelectItem>
                <SelectItem value="username_asc">Username A-Z</SelectItem>
                <SelectItem value="username_desc">Username Z-A</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>

        <CardContent>
          {error && (
            <div className="mb-3 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
              {error}
            </div>
          )}

          {loading ? (
            <p className="py-8 text-center text-muted-foreground">Loading...</p>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>User</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead>Role</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead>Failed logins</TableHead>
                    <TableHead>Role update</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((user) => {
                    const currentRole = user.role || "Customer";
                    const selectedRole = roleDraftByUserId[user.id] || currentRole;
                    const roleChanged = selectedRole !== currentRole;
                    const isCurrentAdmin = profile?.id === user.id;
                    const lockActionInProgress = lockActionUserId === user.id;
                    const roleActionInProgress = roleSavingUserId === user.id;

                    return (
                      <TableRow key={user.id}>
                        <TableCell>
                          <div className="font-medium">{user.email}</div>
                          <div className="text-xs text-muted-foreground">@{user.userName}</div>
                        </TableCell>

                        <TableCell>
                          <div>{`${user.firstName} ${user.lastName}`.trim() || "Not specified"}</div>
                          <div className="text-xs text-muted-foreground">{user.phoneNumber || "No phone"}</div>
                        </TableCell>

                        <TableCell>
                          <Badge className={getUserRoleBadgeClass(currentRole)}>
                            {currentRole}
                          </Badge>
                        </TableCell>

                        <TableCell>
                          <Badge className={user.isLockedOut ? "bg-rose-100 text-rose-800" : "bg-emerald-100 text-emerald-800"}>
                            {user.isLockedOut ? "Locked" : "Active"}
                          </Badge>
                          {user.isLockedOut && user.lockoutEnd && (
                            <div className="mt-1 text-xs text-muted-foreground">
                              Until {new Date(user.lockoutEnd).toLocaleString("en-US")}
                            </div>
                          )}
                        </TableCell>

                        <TableCell>{new Date(user.createdAt).toLocaleDateString("en-US")}</TableCell>
                        <TableCell>{user.accessFailedCount}</TableCell>

                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Select
                              value={selectedRole}
                              onValueChange={(value) => {
                                if (!value) return;
                                setRoleDraftByUserId((current) => ({
                                  ...current,
                                  [user.id]: value,
                                }));
                              }}
                              disabled={isCurrentAdmin || roleActionInProgress || lockActionInProgress}
                            >
                              <SelectTrigger className="w-[150px]">
                                <SelectValue>
                                  {(value) => (value ? value : "Select role")}
                                </SelectValue>
                              </SelectTrigger>
                              <SelectContent>
                                {userRoles.map((role) => (
                                  <SelectItem key={`user-role-${user.id}-${role}`} value={role}>
                                    {role}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                            <Button
                              variant="outline"
                              size="sm"
                              disabled={isCurrentAdmin || !roleChanged || roleActionInProgress || lockActionInProgress}
                              onClick={() => void handleRoleSave(user)}
                            >
                              {roleActionInProgress ? "Saving..." : "Save"}
                            </Button>
                          </div>
                          {isCurrentAdmin && (
                            <p className="mt-1 text-xs text-muted-foreground">
                              You cannot change your own role.
                            </p>
                          )}
                        </TableCell>

                        <TableCell className="text-right">
                          <Button
                            variant={user.isLockedOut ? "outline" : "destructive"}
                            size="sm"
                            disabled={isCurrentAdmin || lockActionInProgress || roleActionInProgress}
                            onClick={() => {
                              if (user.isLockedOut) {
                                void handleUnlock(user);
                                return;
                              }
                              openLockDialog(user);
                            }}
                          >
                            {user.isLockedOut ? (
                              <>
                                <LockOpen className="mr-1 h-3.5 w-3.5" />
                                {lockActionInProgress ? "Unlocking..." : "Unlock"}
                              </>
                            ) : (
                              <>
                                <Lock className="mr-1 h-3.5 w-3.5" />
                                {lockActionInProgress ? "Locking..." : "Lock"}
                              </>
                            )}
                          </Button>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>

              {users.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No users found for current filters.
                </p>
              )}

              {totalPages > 1 && (
                <div className="mt-4 flex items-center justify-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => setPage((value) => value - 1)}
                  >
                    Previous
                  </Button>
                  <span className="text-sm text-muted-foreground">
                    {page} / {totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= totalPages}
                    onClick={() => setPage((value) => value + 1)}
                  >
                    Next
                  </Button>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog
        open={Boolean(lockDialogUser)}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setLockDialogUser(null);
          }
        }}
      >
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Lock user account</DialogTitle>
            <DialogDescription>
              {lockDialogUser ? `Choose lock period for ${lockDialogUser.email}` : "Choose lock period"}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="lockDuration">Lock period</Label>
              <Select
                value={lockDuration}
                onValueChange={(value) => {
                  if (!value) return;
                  setLockDuration(value as "1d" | "7d" | "30d" | "90d" | "custom");
                }}
              >
                <SelectTrigger id="lockDuration">
                  <SelectValue>
                    {(value) => {
                      if (!value) return "Select period";
                      if (value === "1d") return "1 day";
                      if (value === "7d") return "7 days";
                      if (value === "30d") return "30 days";
                      if (value === "90d") return "90 days";
                      if (value === "custom") return "Custom date/time";
                      return value;
                    }}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="1d">1 day</SelectItem>
                  <SelectItem value="7d">7 days</SelectItem>
                  <SelectItem value="30d">30 days</SelectItem>
                  <SelectItem value="90d">90 days</SelectItem>
                  <SelectItem value="custom">Custom date/time</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {lockDuration === "custom" && (
              <div className="space-y-2">
                <Label htmlFor="customLockUntil">Lock until</Label>
                <Input
                  id="customLockUntil"
                  type="datetime-local"
                  value={customLockUntil}
                  min={toDateTimeLocalValue(new Date(Date.now() + 60 * 1000))}
                  onChange={(event) => setCustomLockUntil(event.target.value)}
                />
                <p className="text-xs text-muted-foreground">
                  The selected date/time must be in the future.
                </p>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setLockDialogUser(null)}
              disabled={!lockDialogUser || lockActionUserId === lockDialogUser.id}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => void handleConfirmLock()}
              disabled={!lockDialogUser || lockActionUserId === lockDialogUser.id}
            >
              {lockDialogUser && lockActionUserId === lockDialogUser.id ? "Locking..." : "Lock user"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </TabsContent>
  );
}
