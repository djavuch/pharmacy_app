"use client";

import { useEffect, useMemo, useState, type Dispatch, type SetStateAction } from "react";
import { Edit, Plus, Search, Trash2 } from "lucide-react";
import { adminCategoryApi, adminDiscountApi, adminProductApi } from "@/shared/api";
import type {
  CreateDiscountDto,
  DiscountDto,
  DiscountType,
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

type DiscountFormState = {
  name: string;
  description: string;
  discountType: DiscountType;
  value: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  minimumOrderAmount: string;
  maximumOrderAmount: string;
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

function defaultDiscountForm(): DiscountFormState {
  const now = new Date();
  const weekFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

  return {
    name: "",
    description: "",
    discountType: "Percentage",
    value: "",
    startDate: toDateTimeLocalValue(now),
    endDate: toDateTimeLocalValue(weekFromNow),
    isActive: true,
    minimumOrderAmount: "",
    maximumOrderAmount: "",
    productIds: [],
    categoryIds: [],
  };
}

function discountToFormState(discount: DiscountDto): DiscountFormState {
  return {
    name: discount.name,
    description: discount.description ?? "",
    discountType: discount.discountType,
    value: String(discount.value),
    startDate: toDateTimeLocalValue(discount.startDate),
    endDate: toDateTimeLocalValue(discount.endDate),
    isActive: discount.isActive,
    minimumOrderAmount: discount.minimumOrderAmount != null ? String(discount.minimumOrderAmount) : "",
    maximumOrderAmount: discount.maximumOrderAmount != null ? String(discount.maximumOrderAmount) : "",
    productIds: [...discount.productIds],
    categoryIds: [...discount.categoryIds],
  };
}

function validateDiscountForm(form: DiscountFormState): {
  payload?: CreateDiscountDto;
  error?: string;
} {
  const name = form.name.trim();
  if (!name) {
    return { error: "Discount name is required." };
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

  const minimumOrderAmount = form.minimumOrderAmount.trim()
    ? Number.parseFloat(form.minimumOrderAmount)
    : undefined;
  const maximumOrderAmount = form.maximumOrderAmount.trim()
    ? Number.parseFloat(form.maximumOrderAmount)
    : undefined;

  if (minimumOrderAmount != null && (!Number.isFinite(minimumOrderAmount) || minimumOrderAmount <= 0)) {
    return { error: "Minimum order amount must be greater than 0." };
  }

  if (maximumOrderAmount != null && (!Number.isFinite(maximumOrderAmount) || maximumOrderAmount <= 0)) {
    return { error: "Maximum order amount must be greater than 0." };
  }

  if (
    minimumOrderAmount != null &&
    maximumOrderAmount != null &&
    maximumOrderAmount <= minimumOrderAmount
  ) {
    return { error: "Maximum order amount must be greater than minimum order amount." };
  }

  return {
    payload: {
      name,
      description: form.description.trim(),
      discountType: form.discountType,
      value,
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      isActive: form.isActive,
      minimumOrderAmount,
      maximumOrderAmount,
      productIds: [...form.productIds],
      categoryIds: [...form.categoryIds],
    },
  };
}

function formatDiscountValue(discount: DiscountDto): string {
  if (discount.discountType === "Percentage") {
    return `${discount.value}%`;
  }
  return `$${discount.value.toFixed(2)}`;
}

export function DiscountsTab() {
  const [discounts, setDiscounts] = useState<DiscountDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [reloadToken, setReloadToken] = useState(0);

  const [createOpen, setCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState<DiscountFormState>(() => defaultDiscountForm());
  const [createError, setCreateError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const [editingDiscount, setEditingDiscount] = useState<DiscountDto | null>(null);
  const [editForm, setEditForm] = useState<DiscountFormState>(() => defaultDiscountForm());
  const [editError, setEditError] = useState<string | null>(null);
  const [updating, setUpdating] = useState(false);

  const [deletingDiscount, setDeletingDiscount] = useState<DiscountDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const refresh = () => setReloadToken((value) => value + 1);

  useEffect(() => {
    const fetchDiscounts = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await adminDiscountApi.getAll();
        const sorted = [...(response ?? [])].sort(
          (left, right) => new Date(right.startDate).getTime() - new Date(left.startDate).getTime(),
        );
        setDiscounts(sorted);
      } catch (err) {
        setDiscounts([]);
        setError(err instanceof Error ? err.message : "Failed to load discounts.");
      } finally {
        setLoading(false);
      }
    };

    void fetchDiscounts();
  }, [reloadToken]);

  useEffect(() => {
    if (!editingDiscount) {
      setEditForm(defaultDiscountForm());
      setEditError(null);
      return;
    }

    setEditForm(discountToFormState(editingDiscount));
    setEditError(null);
  }, [editingDiscount]);

  const filteredDiscounts = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) {
      return discounts;
    }

    return discounts.filter((discount) => {
      const haystack = [
        discount.discountId,
        discount.name,
        discount.description,
        discount.discountType,
      ]
        .join(" ")
        .toLowerCase();
      return haystack.includes(query);
    });
  }, [discounts, search]);

  const handleCreate = async () => {
    const validation = validateDiscountForm(createForm);
    if (!validation.payload) {
      setCreateError(validation.error ?? "Invalid discount values.");
      return;
    }

    setCreating(true);
    setCreateError(null);
    setError(null);
    try {
      await adminDiscountApi.create(validation.payload);
      setCreateOpen(false);
      setCreateForm(defaultDiscountForm());
      refresh();
    } catch (err) {
      setCreateError(err instanceof Error ? err.message : "Failed to create discount.");
    } finally {
      setCreating(false);
    }
  };

  const handleUpdate = async () => {
    if (!editingDiscount) {
      return;
    }

    const validation = validateDiscountForm(editForm);
    if (!validation.payload) {
      setEditError(validation.error ?? "Invalid discount values.");
      return;
    }

    setUpdating(true);
    setEditError(null);
    setError(null);
    try {
      await adminDiscountApi.update(editingDiscount.discountId, validation.payload);
      setEditingDiscount(null);
      refresh();
    } catch (err) {
      setEditError(err instanceof Error ? err.message : "Failed to update discount.");
    } finally {
      setUpdating(false);
    }
  };

  const handleDelete = async () => {
    if (!deletingDiscount) {
      return;
    }

    setDeleting(true);
    setError(null);
    try {
      await adminDiscountApi.delete(deletingDiscount.discountId);
      setDeletingDiscount(null);
      refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete discount.");
    } finally {
      setDeleting(false);
    }
  };

  return (
    <TabsContent value="discounts">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle>Discount Management</CardTitle>
              <CardDescription>Create and maintain product or category discounts</CardDescription>
            </div>

            <Dialog
              open={createOpen}
              onOpenChange={(open) => {
                setCreateOpen(open);
                if (!open) {
                  setCreateForm(defaultDiscountForm());
                  setCreateError(null);
                }
              }}
            >
              <DialogTrigger
                render={(
                  <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    Add Discount
                  </Button>
                )}
              />
              <DialogContent className="max-h-[90vh] w-[98vw] max-w-[calc(100%-1rem)] sm:max-w-[1200px] overflow-y-auto">
                <DialogHeader>
                  <DialogTitle>Create discount</DialogTitle>
                  <DialogDescription>Configure a discount rule for products or categories.</DialogDescription>
                </DialogHeader>

                <DiscountFormFields form={createForm} onChange={setCreateForm} />

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
              placeholder="Search by id, name, type..."
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
                    <TableHead>Name</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Value</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Period</TableHead>
                    <TableHead>Scope</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredDiscounts.map((discount) => (
                    <TableRow key={discount.discountId}>
                      <TableCell>
                        <div className="font-medium">{discount.name}</div>
                        <div className="line-clamp-1 text-xs text-muted-foreground">
                          {discount.description?.trim() || "No description"}
                        </div>
                      </TableCell>
                      <TableCell>{discount.discountType}</TableCell>
                      <TableCell>{formatDiscountValue(discount)}</TableCell>
                      <TableCell>
                        <Badge className={discount.isActive ? "bg-emerald-100 text-emerald-800" : "bg-slate-100 text-slate-800"}>
                          {discount.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="text-xs">
                          <div>{new Date(discount.startDate).toLocaleString("en-US")}</div>
                          <div className="text-muted-foreground">to {new Date(discount.endDate).toLocaleString("en-US")}</div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="text-xs text-muted-foreground">
                          Products: {discount.productIds.length}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          Categories: {discount.categoryIds.length}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="icon"
                            title="Edit discount"
                            onClick={() => setEditingDiscount(discount)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            title="Delete discount"
                            onClick={() => setDeletingDiscount(discount)}
                          >
                            <Trash2 className="h-4 w-4 text-rose-600" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {filteredDiscounts.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No discounts found for current filters.
                </p>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog
        open={Boolean(editingDiscount)}
        onOpenChange={(open) => {
          if (!open) {
            setEditingDiscount(null);
          }
        }}
      >
        <DialogContent className="max-h-[90vh] w-[98vw] max-w-[calc(100%-1rem)] sm:max-w-[1200px] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit discount</DialogTitle>
            <DialogDescription>
              {editingDiscount ? `Update ${editingDiscount.name}` : "Update discount"}
            </DialogDescription>
          </DialogHeader>

          <DiscountFormFields form={editForm} onChange={setEditForm} />

          {editError && (
            <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
              {editError}
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setEditingDiscount(null)}
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
        open={Boolean(deletingDiscount)}
        onOpenChange={(open) => {
          if (!open) {
            setDeletingDiscount(null);
          }
        }}
      >
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Delete discount</DialogTitle>
            <DialogDescription>This action cannot be undone.</DialogDescription>
          </DialogHeader>

          <div className="rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
            {deletingDiscount
              ? `Delete "${deletingDiscount.name}"?`
              : "Delete selected discount?"}
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeletingDiscount(null)}
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

function DiscountFormFields({
  form,
  onChange,
}: {
  form: DiscountFormState;
  onChange: Dispatch<SetStateAction<DiscountFormState>>;
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
    <div className="grid gap-5 xl:grid-cols-[minmax(0,1.1fr)_minmax(0,1fr)]">
      <div className="space-y-4">
        <div className="space-y-2">
          <Label htmlFor="discountName">Name *</Label>
          <Input
            id="discountName"
            value={form.name}
            onChange={(event) => onChange((current) => ({ ...current, name: event.target.value }))}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="discountDescription">Description</Label>
          <Textarea
            id="discountDescription"
            className="min-h-24"
            value={form.description}
            onChange={(event) => onChange((current) => ({ ...current, description: event.target.value }))}
          />
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2">
            <Label htmlFor="discountType">Type *</Label>
            <Select
              value={form.discountType}
              onValueChange={(value) =>
                onChange((current) => ({ ...current, discountType: value as DiscountType }))
              }
            >
              <SelectTrigger id="discountType">
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
            <Label htmlFor="discountValue">Value *</Label>
            <Input
              id="discountValue"
              type="number"
              min={0}
              step="0.01"
              value={form.value}
              onChange={(event) => onChange((current) => ({ ...current, value: event.target.value }))}
            />
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2">
            <Label htmlFor="discountStartDate">Start date *</Label>
            <Input
              id="discountStartDate"
              type="datetime-local"
              value={form.startDate}
              onChange={(event) => onChange((current) => ({ ...current, startDate: event.target.value }))}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="discountEndDate">End date *</Label>
            <Input
              id="discountEndDate"
              type="datetime-local"
              value={form.endDate}
              onChange={(event) => onChange((current) => ({ ...current, endDate: event.target.value }))}
            />
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2">
            <Label htmlFor="discountMinOrder">Minimum order amount</Label>
            <Input
              id="discountMinOrder"
              type="number"
              min={0}
              step="0.01"
              value={form.minimumOrderAmount}
              onChange={(event) => onChange((current) => ({ ...current, minimumOrderAmount: event.target.value }))}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="discountMaxOrder">Maximum order amount</Label>
            <Input
              id="discountMaxOrder"
              type="number"
              min={0}
              step="0.01"
              value={form.maximumOrderAmount}
              onChange={(event) => onChange((current) => ({ ...current, maximumOrderAmount: event.target.value }))}
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
          Active discount
        </label>
      </div>

      <div className="space-y-4">
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
        />
      </div>
    </div>
  );
}
