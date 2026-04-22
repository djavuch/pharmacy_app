import { useEffect, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { adminOrderApi } from "@/shared/api";
import type { OrderDetailsDto, OrderStatus, OrderSummaryDto } from "@/shared/types";
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
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { orderSortOptions, orderStatuses, type OrdersSort } from "./admin-constants";
import { buildUrlWithQuery, getCurrentUrl, parseEnumParam, parsePositiveIntParam } from "./query-state";
import { getOrderStatusBadgeClass, getStatusText } from "./status-utils";

const orderSortLabels: Record<OrdersSort, string> = {
  date_desc: "Newest first",
  date_asc: "Oldest first",
  status_asc: "Status A-Z",
  status_desc: "Status Z-A",
  customer_asc: "Customer A-Z",
  customer_desc: "Customer Z-A",
};

export function OrdersTab({ isActive }: { isActive: boolean }) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [orders, setOrders] = useState<OrderSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState(() => searchParams.get("ordersSearch") ?? "");
  const [statusFilter, setStatusFilter] = useState<"all" | OrderStatus>(() => {
    const value = searchParams.get("ordersStatus");
    if (value === "all") return "all";
    return orderStatuses.includes(value as OrderStatus) ? (value as OrderStatus) : "all";
  });
  const [sort, setSort] = useState<OrdersSort>(() =>
    parseEnumParam(searchParams.get("ordersSort"), orderSortOptions, "date_desc"),
  );

  const [page, setPage] = useState(() =>
    parsePositiveIntParam(searchParams.get("ordersPage"), 1),
  );
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 15;

  const [statusDraftByOrderId, setStatusDraftByOrderId] = useState<Record<number, OrderStatus>>({});
  const [statusSavingOrderId, setStatusSavingOrderId] = useState<number | null>(null);

  const [detailsOrderId, setDetailsOrderId] = useState<number | null>(null);
  const [selectedOrder, setSelectedOrder] = useState<OrderDetailsDto | null>(null);
  const [detailsLoading, setDetailsLoading] = useState(false);

  useEffect(() => {
    if (!isActive) {
      return;
    }

    const nextUrl = buildUrlWithQuery(pathname, searchParams, {
      tab: "orders",
      ordersSearch: search || undefined,
      ordersStatus: statusFilter === "all" ? undefined : statusFilter,
      ordersSort: sort === "date_desc" ? undefined : sort,
      ordersPage: page > 1 ? page : undefined,
    });
    const currentUrl = getCurrentUrl(pathname, searchParams);

    if (nextUrl !== currentUrl) {
      router.replace(nextUrl, { scroll: false });
    }
  }, [isActive, page, pathname, router, search, searchParams, sort, statusFilter]);

  useEffect(() => {
    const fetchOrders = async () => {
      setLoading(true);
      setError(null);

      try {
        const [sortByKey, direction] = sort.split("_") as [string, "asc" | "desc"];

        const sortBy =
          sortByKey === "date"
            ? "OrderDate"
            : sortByKey === "status"
              ? "OrderStatus"
              : "User";

        const filterOn = search.trim()
          ? "User"
          : statusFilter !== "all"
            ? "OrderStatus"
            : undefined;
        const filterQuery = search.trim()
          ? search.trim()
          : statusFilter !== "all"
            ? statusFilter
            : undefined;

        const result = await adminOrderApi.getAll({
          pageIndex: page,
          pageSize,
          filterOn,
          filterQuery,
          sortBy,
          isAscending: direction === "asc",
        });

        let items = result.data?.items ?? [];
        if (search.trim() && statusFilter !== "all") {
          items = items.filter((order) => order.orderStatus === statusFilter);
        }

        setOrders(items);
        setTotalPages(result.data?.totalPages ?? 1);
        setStatusDraftByOrderId((prev) => {
          const next = { ...prev };
          for (const order of items) {
            if (order.orderStatus === "Cancelled") {
              next[order.id] = "Cancelled";
              continue;
            }

            if (!next[order.id]) {
              next[order.id] = order.orderStatus;
            }
          }
          return next;
        });
      } catch (err) {
        setOrders([]);
        setTotalPages(1);
        setError(err instanceof Error ? err.message : "Failed to load orders");
      } finally {
        setLoading(false);
      }
    };

    void fetchOrders();
  }, [page, pageSize, search, statusFilter, sort]);

  const handleStatusSave = async (order: OrderSummaryDto) => {
    if (order.orderStatus === "Cancelled") {
      setStatusDraftByOrderId((current) => ({
        ...current,
        [order.id]: "Cancelled",
      }));
      return;
    }

    const nextStatus = statusDraftByOrderId[order.id] ?? order.orderStatus;
    if (nextStatus === order.orderStatus) {
      return;
    }

    setStatusSavingOrderId(order.id);
    setError(null);
    try {
      await adminOrderApi.updateStatus(order.id, nextStatus);

      setOrders((current) =>
        current.map((entry) =>
          entry.id === order.id ? { ...entry, orderStatus: nextStatus } : entry,
        ),
      );
      setSelectedOrder((current) =>
        current && current.id === order.id
          ? { ...current, orderStatus: nextStatus }
          : current,
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update order status");
    } finally {
      setStatusSavingOrderId(null);
    }
  };

  const handleOpenDetails = async (orderId: number) => {
    setDetailsOrderId(orderId);
    setDetailsLoading(true);
    setSelectedOrder(null);
    setError(null);

    try {
      const details = await adminOrderApi.getById(orderId);
      setSelectedOrder(details);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load order details");
    } finally {
      setDetailsLoading(false);
    }
  };

  const closeDetailsDialog = () => {
    setDetailsOrderId(null);
    setSelectedOrder(null);
    setDetailsLoading(false);
  };

  return (
    <TabsContent value="orders">
      <Card>
        <CardHeader>
          <CardTitle>Order Management</CardTitle>
          <CardDescription>Search, inspect, and update order statuses</CardDescription>

          <div className="mt-4 grid gap-3 md:grid-cols-3">
            <Input
              placeholder="Search by customer..."
              value={search}
              onChange={(event) => {
                setSearch(event.target.value);
                setPage(1);
              }}
            />

            <Select
              value={statusFilter}
              onValueChange={(value) => {
                if (!value) return;
                setStatusFilter(value as "all" | OrderStatus);
                setPage(1);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Filter by status">
                  {(value) => {
                    if (!value || value === "all") {
                      return "All statuses";
                    }
                    return getStatusText(value as OrderStatus);
                  }}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                {orderStatuses.map((status) => (
                  <SelectItem key={`status-filter-${status}`} value={status}>
                    {getStatusText(status)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select
              value={sort}
              onValueChange={(value) => {
                if (!value) return;
                setSort(value as OrdersSort);
                setPage(1);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Sort by">
                  {(value) => {
                    if (!value) {
                      return "Sort by";
                    }
                    return orderSortLabels[value as OrdersSort] ?? value;
                  }}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="date_desc">Newest first</SelectItem>
                <SelectItem value="date_asc">Oldest first</SelectItem>
                <SelectItem value="status_asc">Status A-Z</SelectItem>
                <SelectItem value="status_desc">Status Z-A</SelectItem>
                <SelectItem value="customer_asc">Customer A-Z</SelectItem>
                <SelectItem value="customer_desc">Customer Z-A</SelectItem>
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
                    <TableHead>ID</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Customer</TableHead>
                    <TableHead>Items</TableHead>
                    <TableHead>Total</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Update status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {orders.map((order) => {
                    const selectedStatus =
                      statusDraftByOrderId[order.id] ?? order.orderStatus;
                    const statusChanged = selectedStatus !== order.orderStatus;
                    const isUpdating = statusSavingOrderId === order.id;
                    const isCancelledOrder = order.orderStatus === "Cancelled";

                    return (
                      <TableRow key={order.id}>
                        <TableCell>{order.id}</TableCell>
                        <TableCell>
                          {new Date(order.orderDate).toLocaleDateString("en-US")}
                        </TableCell>
                        <TableCell>{order.buyerFullName}</TableCell>
                        <TableCell>{order.itemsCount}</TableCell>
                        <TableCell>${order.totalAmount}</TableCell>
                        <TableCell>
                          <Badge className={getOrderStatusBadgeClass(order.orderStatus)}>
                            {getStatusText(order.orderStatus)}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Select
                              value={selectedStatus}
                              disabled={isCancelledOrder}
                              onValueChange={(value) => {
                                if (!value) return;
                                if (isCancelledOrder) return;
                                setStatusDraftByOrderId((current) => ({
                                  ...current,
                                  [order.id]: value as OrderStatus,
                                }));
                              }}
                            >
                              <SelectTrigger className="w-[150px]">
                                <SelectValue>
                                  {(value) => (value ? getStatusText(value as OrderStatus) : "Select status")}
                                </SelectValue>
                              </SelectTrigger>
                              <SelectContent>
                                {orderStatuses.map((status) => (
                                  <SelectItem key={`order-status-${order.id}-${status}`} value={status}>
                                    {getStatusText(status)}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                            <Button
                              variant="outline"
                              size="sm"
                              disabled={isCancelledOrder || !statusChanged || isUpdating}
                              onClick={() => void handleStatusSave(order)}
                            >
                              {isUpdating ? "Saving..." : "Save"}
                            </Button>
                          </div>
                        </TableCell>
                        <TableCell className="text-right">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => void handleOpenDetails(order.id)}
                          >
                            View
                          </Button>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>

              {orders.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No orders found for current filters.
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
        open={detailsOrderId !== null}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            closeDetailsDialog();
          }
        }}
      >
        <DialogContent className="max-h-[85vh] w-[96vw] max-w-[1200px] sm:max-w-[1200px] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Order details</DialogTitle>
            <DialogDescription>
              {detailsOrderId ? `Order #${detailsOrderId}` : "Order"}
            </DialogDescription>
          </DialogHeader>

          {detailsLoading ? (
            <p className="py-6 text-center text-muted-foreground">Loading details...</p>
          ) : selectedOrder ? (
            <div className="space-y-4">
              <div className="grid gap-3 rounded-lg border bg-muted/30 p-4 md:grid-cols-2 lg:grid-cols-3">
                <div>
                  <p className="text-xs text-muted-foreground">Customer</p>
                  <p className="font-medium">{selectedOrder.buyerFullName}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Order date</p>
                  <p className="font-medium">
                    {new Date(selectedOrder.orderDate).toLocaleString("en-US")}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Status</p>
                  <Badge className={getOrderStatusBadgeClass(selectedOrder.orderStatus)}>
                    {getStatusText(selectedOrder.orderStatus)}
                  </Badge>
                </div>
              </div>

              <div className="grid gap-3 rounded-lg border bg-muted/30 p-4 md:grid-cols-2 lg:grid-cols-3">
                <div>
                  <p className="text-xs text-muted-foreground">Subtotal</p>
                  <p className="font-medium">
                    ${(selectedOrder.totalAmount + selectedOrder.promoCodeDiscountAmount).toFixed(2)}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Promo discount</p>
                  <p className="font-medium text-emerald-700">
                    -${selectedOrder.promoCodeDiscountAmount.toFixed(2)}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Total</p>
                  <p className="font-semibold">${selectedOrder.totalAmount.toFixed(2)}</p>
                </div>
              </div>

              <div className="rounded-lg border p-4">
                <p className="text-sm font-semibold">Shipping address</p>
                <p className="mt-1 break-words text-sm text-muted-foreground">
                  {selectedOrder.shippingAddress
                    ? [
                        selectedOrder.shippingAddress.street,
                        selectedOrder.shippingAddress.apartmentNumber,
                        selectedOrder.shippingAddress.city,
                        selectedOrder.shippingAddress.state,
                        selectedOrder.shippingAddress.zipCode,
                        selectedOrder.shippingAddress.country,
                      ]
                        .filter(Boolean)
                        .join(", ")
                    : "Address not specified"}
                </p>
              </div>

              <div className="rounded-lg border p-4">
                <p className="mb-3 text-sm font-semibold">Items</p>
                <div className="overflow-x-auto">
                  <Table className="min-w-[620px]">
                    <TableHeader>
                      <TableRow>
                        <TableHead>Product</TableHead>
                        <TableHead>Qty</TableHead>
                        <TableHead>Price</TableHead>
                        <TableHead>Subtotal</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {selectedOrder.orderItems.map((item, index) => (
                        <TableRow key={`${selectedOrder.id}-${item.productId}-${index}`}>
                          <TableCell>{item.productName}</TableCell>
                          <TableCell>{item.quantity}</TableCell>
                          <TableCell>${item.price.toFixed(2)}</TableCell>
                          <TableCell>${item.subtotal.toFixed(2)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            </div>
          ) : (
            <p className="py-6 text-center text-muted-foreground">
              Could not load order details.
            </p>
          )}
        </DialogContent>
      </Dialog>
    </TabsContent>
  );
}
