"use client";

import { useEffect, useMemo, useState, type Dispatch, type SetStateAction } from "react";
import { Edit, Plus, Search, Trash2 } from "lucide-react";
import { adminCategoryApi, adminProductApi, adminPromoCodeApi } from "@/shared/api";
import type {
  CreatePromoCodeDto,
  DiscountType,
  PromoCodeDto,
} from "@/shared/types";
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { AsyncSearchMultiSelect, type AsyncSearchOption } from "./async-search-multiselect";

const discountTypeOptions: DiscountType[] = ["Percentage", "FixedAmount"];
const promoCodePattern = /^[A-Z0-9_-]+$/;

type PromoCodeFormState = {
  code: string;
  description: string;
  discountType: DiscountType;
  value: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  maxUsageCount: string;
  maxUsagePerUser: string;
  minimumOrderAmount: string;
  maximumDiscountAmount: string;
  applicableToAllProducts: boolean;
  productIds: number[];
  categoryIds: number[];
};

function toDateTimeLocalValue(value: string | Date): string {
  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  const tzOffsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - tzOffsetMs).toISOString().slice(0, 16);
}

function defaultPromoCodeForm(): PromoCodeFormState {
  const now = new Date();
  const weekFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

  return {
    code: "",
    description: "",
    discountType: "Percentage",
    value: "",
    startDate: toDateTimeLocalValue(now),
    endDate: toDateTimeLocalValue(weekFromNow),
    isActive: true,
    maxUsageCount: "",
    maxUsagePerUser: "",
    minimumOrderAmount: "",
    maximumDiscountAmount: "",
    applicableToAllProducts: true,
    productIds: [],
    categoryIds: [],
  };
}

function promoCodeToFormState(promoCode: PromoCodeDto): PromoCodeFormState {
  return {
    code: promoCode.code,
    description: promoCode.description ?? "",
    discountType: promoCode.discountType,
    value: String(promoCode.value),
    startDate: toDateTimeLocalValue(promoCode.startDate),
    endDate: toDateTimeLocalValue(promoCode.endDate),
    isActive: promoCode.isActive,
    maxUsageCount: promoCode.maxUsageCount != null ? String(promoCode.maxUsageCount) : "",
    maxUsagePerUser: promoCode.maxUsagePerUser != null ? String(promoCode.maxUsagePerUser) : "",
    minimumOrderAmount: promoCode.minimumOrderAmount != null ? String(promoCode.minimumOrderAmount) : "",
    maximumDiscountAmount: promoCode.maximumDiscountAmount != null ? String(promoCode.maximumDiscountAmount) : "",
    applicableToAllProducts: promoCode.applicableToAllProducts,
    productIds: [...promoCode.productIds],
    categoryIds: [...promoCode.categoryIds],
  };
}

function validatePromoCodeForm(form: PromoCodeFormState): {
  payload?: CreatePromoCodeDto;
  error?: string;
} {
  const code = form.code.trim().toUpperCase();
  if (!code) {
    return { error: "Promo code is required." };
  }
  if (!promoCodePattern.test(code)) {
    return { error: "Promo code can only contain uppercase letters, numbers, underscores and hyphens." };
  }

  const value = Number.parseFloat(form.value);
  if (!Number.isFinite(value) || value <= 0) {
    return { error: "Discount value must be greater than 0." };
  }
  if (form.discountType === "Percentage" && value > 100) {
    return { error: "Percentage discount cannot exceed 100%." };
  }

  const startDate = new Date(form.startDate);
  const endDate = new Date(form.endDate);
  if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) {
    return { error: "Start date and end date are required." };
  }
  if (endDate.getTime() <= startDate.getTime()) {
    return { error: "End date must be later than start date." };
  }

  const maxUsageCount = form.maxUsageCount.trim() ? Number.parseInt(form.maxUsageCount, 10) : undefined;
  const maxUsagePerUser = form.maxUsagePerUser.trim() ? Number.parseInt(form.maxUsagePerUser, 10) : undefined;
  const minimumOrderAmount = form.minimumOrderAmount.trim()
    ? Number.parseFloat(form.minimumOrderAmount)
    : undefined;
  const maximumDiscountAmount = form.maximumDiscountAmount.trim()
    ? Number.parseFloat(form.maximumDiscountAmount)
    : undefined;

  if (maxUsageCount != null && (!Number.isFinite(maxUsageCount) || maxUsageCount <= 0)) {
    return { error: "Max usage count must be greater than 0." };
  }
  if (maxUsagePerUser != null && (!Number.isFinite(maxUsagePerUser) || maxUsagePerUser <= 0)) {
    return { error: "Max usage per user must be greater than 0." };
  }
  if (minimumOrderAmount != null && (!Number.isFinite(minimumOrderAmount) || minimumOrderAmount <= 0)) {
    return { error: "Minimum order amount must be greater than 0." };
  }
  if (maximumDiscountAmount != null && (!Number.isFinite(maximumDiscountAmount) || maximumDiscountAmount <= 0)) {
    return { error: "Maximum discount amount must be greater than 0." };
  }

  const productIds = form.applicableToAllProducts ? [] : [...form.productIds];
  const categoryIds = form.applicableToAllProducts ? [] : [...form.categoryIds];
  if (!form.applicableToAllProducts && productIds.length === 0 && categoryIds.length === 0) {
    return { error: "Specify at least one product/category or make promo code applicable to all products." };
  }

  return {
    payload: {
      code,
      description: form.description.trim(),
      discountType: form.discountType,
      value,
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      isActive: form.isActive,
      maxUsageCount,
      maxUsagePerUser,
      minimumOrderAmount,
      maximumDiscountAmount,
      applicableToAllProducts: form.applicableToAllProducts,
      productIds,
      categoryIds,
    },
  };
}

function formatPromoCodeValue(promoCode: PromoCodeDto): string {
  if (promoCode.discountType === "Percentage") {
    return `${promoCode.value}%`;
  }
  return `$${promoCode.value.toFixed(2)}`;
}

export function PromoCodesTab() {
  const [promoCodes, setPromoCodes] = useState<PromoCodeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [reloadToken, setReloadToken] = useState(0);

  const [createOpen, setCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState<PromoCodeFormState>(() => defaultPromoCodeForm());
  const [createError, setCreateError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const [editingPromoCode, setEditingPromoCode] = useState<PromoCodeDto | null>(null);
  const [editForm, setEditForm] = useState<PromoCodeFormState>(() => defaultPromoCodeForm());
  const [editError, setEditError] = useState<string | null>(null);
  const [updating, setUpdating] = useState(false);

  const [deletingPromoCode, setDeletingPromoCode] = useState<PromoCodeDto | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [toggleLoadingId, setToggleLoadingId] = useState<string | null>(null);

  const refresh = () => setReloadToken((value) => value + 1);

  useEffect(() => {
    const fetchPromoCodes = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await adminPromoCodeApi.getAll();
        const sorted = [...(response ?? [])].sort(
          (left, right) => new Date(right.startDate).getTime() - new Date(left.startDate).getTime(),
        );
        setPromoCodes(sorted);
      } catch (err) {
        setPromoCodes([]);
        setError(err instanceof Error ? err.message : "Failed to load promo codes.");
      } finally {
        setLoading(false);
      }
    };

    void fetchPromoCodes();
  }, [reloadToken]);

  useEffect(() => {
    if (!editingPromoCode) {
      setEditForm(defaultPromoCodeForm());
      setEditError(null);
      return;
    }

    setEditForm(promoCodeToFormState(editingPromoCode));
    setEditError(null);
  }, [editingPromoCode]);

  const filteredPromoCodes = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) {
      return promoCodes;
    }

    return promoCodes.filter((promoCode) => {
      const haystack = [
        promoCode.promoCodeId,
        promoCode.code,
        promoCode.description,
        promoCode.discountType,
      ]
        .join(" ")
        .toLowerCase();
      return haystack.includes(query);
    });
  }, [promoCodes, search]);

  const handleCreate = async () => {
    const validation = validatePromoCodeForm(createForm);
    if (!validation.payload) {
      setCreateError(validation.error ?? "Invalid promo code values.");
      return;
    }

    setCreating(true);
    setCreateError(null);
    setError(null);
    try {
      await adminPromoCodeApi.create(validation.payload);
      setCreateOpen(false);
      setCreateForm(defaultPromoCodeForm());
      refresh();
    } catch (err) {
      setCreateError(err instanceof Error ? err.message : "Failed to create promo code.");
    } finally {
      setCreating(false);
    }
  };

  const handleUpdate = async () => {
    if (!editingPromoCode) {
      return;
    }

    const validation = validatePromoCodeForm(editForm);
    if (!validation.payload) {
      setEditError(validation.error ?? "Invalid promo code values.");
      return;
    }

    setUpdating(true);
    setEditError(null);
    setError(null);
    try {
      await adminPromoCodeApi.update(editingPromoCode.promoCodeId, validation.payload);
      setEditingPromoCode(null);
      refresh();
    } catch (err) {
      setEditError(err instanceof Error ? err.message : "Failed to update promo code.");
    } finally {
      setUpdating(false);
    }
  };

  const handleDelete = async () => {
    if (!deletingPromoCode) {
      return;
    }

    setDeleting(true);
    setError(null);
    try {
      await adminPromoCodeApi.delete(deletingPromoCode.promoCodeId);
      setDeletingPromoCode(null);
      refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete promo code.");
    } finally {
      setDeleting(false);
    }
  };

  const handleToggleActive = async (promoCode: PromoCodeDto) => {
    setToggleLoadingId(promoCode.promoCodeId);
    setError(null);
    try {
      if (promoCode.isActive) {
        await adminPromoCodeApi.deactivate(promoCode.promoCodeId);
      } else {
        await adminPromoCodeApi.activate(promoCode.promoCodeId);
      }

      setPromoCodes((current) =>
        current.map((entry) =>
          entry.promoCodeId === promoCode.promoCodeId
            ? { ...entry, isActive: !promoCode.isActive }
            : entry,
        ),
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update promo code activity.");
    } finally {
      setToggleLoadingId(null);
    }
  };

  return (
    <TabsContent value="promoCodes">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle>Promo Code Management</CardTitle>
              <CardDescription>Create, activate, and maintain promo codes</CardDescription>
            </div>

            <Dialog
              open={createOpen}
              onOpenChange={(open) => {
                setCreateOpen(open);
                if (!open) {
                  setCreateForm(defaultPromoCodeForm());
                  setCreateError(null);
                }
              }}
            >
              <DialogTrigger
                render={(
                  <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    Add Promo Code
                  </Button>
                )}
              />
              <DialogContent className="max-h-[85vh] max-w-2xl overflow-y-auto">
                <DialogHeader>
                  <DialogTitle>Create promo code</DialogTitle>
                  <DialogDescription>Set discount and usage rules.</DialogDescription>
                </DialogHeader>

                <PromoCodeFormFields form={createForm} onChange={setCreateForm} />

                {createError && (
                  <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
                    {createError}
                  </div>
                )}

                <DialogFooter>
                  <Button
                    variant="outline"
                    onClick={() => setCreateOpen(false)}
                    disabled={creating}
                  >
                    Cancel
                  </Button>
                  <Button onClick={() => void handleCreate()} disabled={creating}>
                    {creating ? "Creating..." : "Create"}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>

          <div className="mt-4 flex items-center gap-2">
            <Search className="h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search by code, id, type..."
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              className="max-w-sm"
            />
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
                    <TableHead>Code</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Value</TableHead>
                    <TableHead>Usage</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Period</TableHead>
                    <TableHead>Scope</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredPromoCodes.map((promoCode) => (
                    <TableRow key={promoCode.promoCodeId}>
                      <TableCell>
                        <div className="font-medium">{promoCode.code}</div>
                        <div className="line-clamp-1 text-xs text-muted-foreground">
                          {promoCode.description?.trim() || "No description"}
                        </div>
                      </TableCell>
                      <TableCell>{promoCode.discountType}</TableCell>
                      <TableCell>{formatPromoCodeValue(promoCode)}</TableCell>
                      <TableCell>
                        {promoCode.currentUsageCount}
                        {promoCode.maxUsageCount != null ? ` / ${promoCode.maxUsageCount}` : " / unlimited"}
                      </TableCell>
                      <TableCell>
                        <Badge className={promoCode.isActive ? "bg-emerald-100 text-emerald-800" : "bg-slate-100 text-slate-800"}>
                          {promoCode.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="text-xs">
                          <div>{new Date(promoCode.startDate).toLocaleString("en-US")}</div>
                          <div className="text-muted-foreground">to {new Date(promoCode.endDate).toLocaleString("en-US")}</div>
                        </div>
                      </TableCell>
                      <TableCell>
                        {promoCode.applicableToAllProducts ? (
                          <span className="text-xs text-muted-foreground">All products</span>
                        ) : (
                          <div className="text-xs text-muted-foreground">
                            <div>Products: {promoCode.productIds.length}</div>
                            <div>Categories: {promoCode.categoryIds.length}</div>
                          </div>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            disabled={toggleLoadingId === promoCode.promoCodeId}
                            onClick={() => void handleToggleActive(promoCode)}
                          >
                            {toggleLoadingId === promoCode.promoCodeId
                              ? "Saving..."
                              : promoCode.isActive
                                ? "Deactivate"
                                : "Activate"}
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            title="Edit promo code"
                            onClick={() => setEditingPromoCode(promoCode)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            title="Delete promo code"
                            onClick={() => setDeletingPromoCode(promoCode)}
                          >
                            <Trash2 className="h-4 w-4 text-rose-600" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {filteredPromoCodes.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No promo codes found for current filters.
                </p>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog
        open={Boolean(editingPromoCode)}
        onOpenChange={(open) => {
          if (!open) {
            setEditingPromoCode(null);
          }
        }}
      >
        <DialogContent className="max-h-[85vh] max-w-2xl overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit promo code</DialogTitle>
            <DialogDescription>
              {editingPromoCode ? `Update ${editingPromoCode.code}` : "Update promo code"}
            </DialogDescription>
          </DialogHeader>

          <PromoCodeFormFields form={editForm} onChange={setEditForm} />

          {editError && (
            <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
              {editError}
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setEditingPromoCode(null)}
              disabled={updating}
            >
              Cancel
            </Button>
            <Button onClick={() => void handleUpdate()} disabled={updating}>
              {updating ? "Saving..." : "Save changes"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={Boolean(deletingPromoCode)}
        onOpenChange={(open) => {
          if (!open) {
            setDeletingPromoCode(null);
          }
        }}
      >
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Delete promo code</DialogTitle>
            <DialogDescription>This action cannot be undone.</DialogDescription>
          </DialogHeader>

          <div className="rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
            {deletingPromoCode
              ? `Delete promo code "${deletingPromoCode.code}"?`
              : "Delete selected promo code?"}
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeletingPromoCode(null)}
              disabled={deleting}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => void handleDelete()}
              disabled={deleting}
            >
              {deleting ? "Deleting..." : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </TabsContent>
  );
}

function PromoCodeFormFields({
  form,
  onChange,
}: {
  form: PromoCodeFormState;
  onChange: Dispatch<SetStateAction<PromoCodeFormState>>;
}) {
  const fetchProductsPage = async ({
    query,
    page,
    pageSize,
  }: {
    query: string;
    page: number;
    pageSize: number;
    signal: AbortSignal;
  }) => {
    const result = await adminProductApi.getAll({
      pageIndex: page,
      pageSize,
      filterOn: "name",
      filterQuery: query,
      sortBy: "name",
      isAscending: true,
    });

    const data = result.data;
    return {
      items: (data?.items ?? []).map((product) => ({
        id: product.id,
        label: product.name,
        description: `#${product.id}${product.productCode ? ` | ${product.productCode}` : ""}`,
      })),
      hasNextPage: data?.hasNextPage ?? false,
    };
  };

  const fetchCategoriesPage = async ({
    query,
    page,
    pageSize,
  }: {
    query: string;
    page: number;
    pageSize: number;
    signal: AbortSignal;
  }) => {
    const result = await adminCategoryApi.getAll({
      pageIndex: page,
      pageSize,
      filterOn: query ? "categoryName" : undefined,
      filterQuery: query || undefined,
      sortBy: "name",
      isAscending: true,
    });

    return {
      items: (result.items ?? []).map((category) => ({
        id: category.categoryId,
        label: category.categoryName,
        description: `#${category.categoryId}`,
      })),
      hasNextPage: result.hasNextPage ?? false,
    };
  };

  const loadSelectedProducts = async (ids: number[]): Promise<AsyncSearchOption[]> => {
    const results = await Promise.all(
      ids.map(async (id) => {
        try {
          const product = await adminProductApi.getById(id);
          return {
            id: product.id,
            label: product.name,
            description: `#${product.id}${product.productCode ? ` | ${product.productCode}` : ""}`,
          };
        } catch {
          return { id, label: `Product #${id}` };
        }
      }),
    );
    return results;
  };

  const loadSelectedCategories = async (ids: number[]): Promise<AsyncSearchOption[]> => {
    const results = await Promise.all(
      ids.map(async (id) => {
        try {
          const category = await adminCategoryApi.getById(id);
          return {
            id: category.categoryId,
            label: category.categoryName,
            description: `#${category.categoryId}`,
          };
        } catch {
          return { id, label: `Category #${id}` };
        }
      }),
    );
    return results;
  };

  return (
    <div className="space-y-4">
      <div className="grid gap-4 md:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="promoCode">Code *</Label>
          <Input
            id="promoCode"
            value={form.code}
            onChange={(event) =>
              onChange((current) => ({ ...current, code: event.target.value.toUpperCase() }))
            }
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="promoValue">Value *</Label>
          <Input
            id="promoValue"
            type="number"
            min={0}
            step="0.01"
            value={form.value}
            onChange={(event) => onChange((current) => ({ ...current, value: event.target.value }))}
          />
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="promoDescription">Description</Label>
        <Textarea
          id="promoDescription"
          value={form.description}
          onChange={(event) => onChange((current) => ({ ...current, description: event.target.value }))}
        />
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="promoDiscountType">Discount type *</Label>
          <Select
            value={form.discountType}
            onValueChange={(value) =>
              onChange((current) => ({ ...current, discountType: value as DiscountType }))
            }
          >
            <SelectTrigger id="promoDiscountType">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {discountTypeOptions.map((type) => (
                <SelectItem key={type} value={type}>
                  {type}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-2">
          <Label htmlFor="promoMinOrder">Minimum order amount</Label>
          <Input
            id="promoMinOrder"
            type="number"
            min={0}
            step="0.01"
            value={form.minimumOrderAmount}
            onChange={(event) =>
              onChange((current) => ({ ...current, minimumOrderAmount: event.target.value }))
            }
          />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="promoStartDate">Start date *</Label>
          <Input
            id="promoStartDate"
            type="datetime-local"
            value={form.startDate}
            onChange={(event) => onChange((current) => ({ ...current, startDate: event.target.value }))}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="promoEndDate">End date *</Label>
          <Input
            id="promoEndDate"
            type="datetime-local"
            value={form.endDate}
            onChange={(event) => onChange((current) => ({ ...current, endDate: event.target.value }))}
          />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <div className="space-y-2">
          <Label htmlFor="promoMaxUsageCount">Max usage count</Label>
          <Input
            id="promoMaxUsageCount"
            type="number"
            min={0}
            step={1}
            value={form.maxUsageCount}
            onChange={(event) => onChange((current) => ({ ...current, maxUsageCount: event.target.value }))}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="promoMaxUsagePerUser">Max usage per user</Label>
          <Input
            id="promoMaxUsagePerUser"
            type="number"
            min={0}
            step={1}
            value={form.maxUsagePerUser}
            onChange={(event) => onChange((current) => ({ ...current, maxUsagePerUser: event.target.value }))}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="promoMaxDiscountAmount">Maximum discount amount</Label>
          <Input
            id="promoMaxDiscountAmount"
            type="number"
            min={0}
            step="0.01"
            value={form.maximumDiscountAmount}
            onChange={(event) =>
              onChange((current) => ({ ...current, maximumDiscountAmount: event.target.value }))
            }
          />
        </div>
      </div>

      <label className="flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          checked={form.isActive}
          onChange={(event) => onChange((current) => ({ ...current, isActive: event.target.checked }))}
          className="h-4 w-4 rounded border-input"
        />
        Active promo code
      </label>

      <label className="flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          checked={form.applicableToAllProducts}
          onChange={(event) =>
            onChange((current) => ({
              ...current,
              applicableToAllProducts: event.target.checked,
              productIds: event.target.checked ? [] : current.productIds,
              categoryIds: event.target.checked ? [] : current.categoryIds,
            }))
          }
          className="h-4 w-4 rounded border-input"
        />
        Applicable to all products
      </label>

      <div className="grid gap-4 md:grid-cols-2">
        <AsyncSearchMultiSelect
          label="Products"
          placeholder="Type 2+ chars to search products..."
          hint="Async search with pagination (20 items per request)."
          selectedIds={form.productIds}
          onChange={(nextIds) => onChange((current) => ({ ...current, productIds: nextIds }))}
          fetchOptions={fetchProductsPage}
          loadSelectedOptions={loadSelectedProducts}
          minQueryLength={2}
          pageSize={20}
          disabled={form.applicableToAllProducts}
        />

        <AsyncSearchMultiSelect
          label="Categories"
          placeholder="Type 2+ chars to search categories..."
          hint="Search starts after 2 characters."
          selectedIds={form.categoryIds}
          onChange={(nextIds) => onChange((current) => ({ ...current, categoryIds: nextIds }))}
          fetchOptions={fetchCategoriesPage}
          loadSelectedOptions={loadSelectedCategories}
          minQueryLength={2}
          pageSize={20}
          disabled={form.applicableToAllProducts}
        />
      </div>
    </div>
  );
}
