"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/entities/user";
import { getAccessToken, orderApi, userApi } from "@/shared/api";
import { OrderAddressDto, OrderDetailsDto, UserOrderSummaryDto } from "@/shared/types";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Separator } from "@/components/ui/separator";
import {
  ArrowLeft,
  ChevronDown,
  Gift,
  MapPin,
  Package,
  ShoppingBag,
} from "lucide-react";

function getStatusText(status: string): string {
  const map: Record<string, string> = {
    Pending: "New",
    Processing: "Processing",
    Shipped: "Shipped",
    Delivered: "Delivered",
    Cancelled: "Cancelled",
    Returned: "Returned",
  };

  return map[status] || status;
}

function getStatusVariant(status: string): "default" | "secondary" | "destructive" {
  if (status === "Delivered") {
    return "default";
  }

  if (status === "Pending" || status === "Processing" || status === "Shipped") {
    return "secondary";
  }

  return "destructive";
}

function formatCurrency(value: number): string {
  return `$${value.toFixed(2)}`;
}

function formatOrderAddress(address?: OrderAddressDto): string {
  if (!address) {
    return "Not specified";
  }

  const firstLine = [address.street, address.apartmentNumber]
    .filter(Boolean)
    .join(", ");
  const secondLine = [address.city, address.state, address.zipCode]
    .filter(Boolean)
    .join(", ");

  return [firstLine, secondLine, address.country, address.additionalInfo]
    .filter(Boolean)
    .join(" | ");
}

export default function OrdersPage() {
  const router = useRouter();
  const { isAuthenticated, loadProfile } = useAuthStore();

  const [orders, setOrders] = useState<UserOrderSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedOrderId, setExpandedOrderId] = useState<number | null>(null);
  const [orderDetailsById, setOrderDetailsById] = useState<Record<number, OrderDetailsDto>>({});
  const [detailsLoadingId, setDetailsLoadingId] = useState<number | null>(null);

  const [orderToCancelId, setOrderToCancelId] = useState<number | null>(null);
  const [cancelLoading, setCancelLoading] = useState(false);
  const [cancelMessage, setCancelMessage] = useState<string | null>(null);
  const [cancelError, setCancelError] = useState<string | null>(null);

  useEffect(() => {
    const fetchOrders = async () => {
      const token = await getAccessToken();

      if (!token) {
        router.push("/login?redirect=/orders");
        return;
      }

      try {
        await loadProfile();
        const result = await userApi.getOrders({ pageIndex: 1, pageSize: 50 });
        setOrders(result);
      } catch {
        setOrders([]);
      } finally {
        setLoading(false);
      }
    };

    void fetchOrders();
  }, [isAuthenticated, loadProfile, router]);

  useEffect(() => {
    if (orders.length === 0) {
      setExpandedOrderId(null);
      return;
    }

    setExpandedOrderId((current) =>
      current !== null && orders.some((order) => order.orderId === current)
        ? current
        : orders[0].orderId
    );
  }, [orders]);

  useEffect(() => {
    if (expandedOrderId === null || orderDetailsById[expandedOrderId]) {
      return;
    }

    let ignore = false;

    const loadOrderDetails = async () => {
      setDetailsLoadingId(expandedOrderId);

      try {
        const details = await orderApi.getById(expandedOrderId);
        if (!ignore) {
          setOrderDetailsById((current) => ({ ...current, [expandedOrderId]: details }));
        }
      } catch {
        // Keep summary view if details endpoint fails.
      } finally {
        if (!ignore) {
          setDetailsLoadingId(null);
        }
      }
    };

    void loadOrderDetails();

    return () => {
      ignore = true;
    };
  }, [expandedOrderId, orderDetailsById]);

  const orderToCancel = orderToCancelId !== null
    ? orders.find((order) => order.orderId === orderToCancelId) ?? null
    : null;

  const cancelOrder = async () => {
    if (orderToCancelId === null) {
      return;
    }

    const targetOrderId = orderToCancelId;

    setCancelLoading(true);
    setCancelError(null);
    setCancelMessage(null);

    try {
      await orderApi.cancel(targetOrderId);

      setOrders((current) =>
        current.map((order) =>
          order.orderId === targetOrderId
            ? { ...order, orderStatus: "Cancelled" }
            : order
        )
      );

      setOrderDetailsById((current) =>
        current[targetOrderId]
          ? {
              ...current,
              [targetOrderId]: {
                ...current[targetOrderId],
                orderStatus: "Cancelled",
              },
            }
          : current
      );

      setCancelMessage(`Order #${targetOrderId} was cancelled.`);
      setOrderToCancelId(null);
    } catch (error) {
      setCancelError(
        error instanceof Error ? error.message : "Failed to cancel order."
      );
    } finally {
      setCancelLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <p className="text-center text-muted-foreground">Loading orders...</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <Link
        href="/profile"
        className="mb-6 inline-flex items-center text-sm text-muted-foreground transition-colors hover:text-foreground"
      >
        <ArrowLeft className="mr-2 h-4 w-4" />
        Back to profile
      </Link>

      <div className="mb-8 flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.28em] text-primary">
            Orders
          </p>
          <h1 className="mt-2 text-4xl font-semibold tracking-tight">
            Review your purchase history
          </h1>
        </div>
        <Link href="/catalog">
          <Button variant="outline">Return to catalog</Button>
        </Link>
      </div>

      {cancelMessage && (
        <Alert className="mb-4">
          <AlertTitle>Order updated</AlertTitle>
          <AlertDescription>{cancelMessage}</AlertDescription>
        </Alert>
      )}

      {cancelError && (
        <Alert className="mb-4" variant="destructive">
          <AlertTitle>Order action failed</AlertTitle>
          <AlertDescription>{cancelError}</AlertDescription>
        </Alert>
      )}

      {orders.length === 0 ? (
        <Card className="rounded-[2rem] border-dashed border-primary/25 bg-background/75">
          <CardContent className="flex flex-col items-center justify-center py-16 text-center">
            <ShoppingBag className="mb-4 h-16 w-16 text-muted-foreground" />
            <h2 className="mb-2 text-2xl font-semibold">No orders yet</h2>
            <p className="mb-6 max-w-lg text-muted-foreground">
              Once you place your first order, it will appear here together with
              status and delivery details.
            </p>
            <Link href="/catalog">
              <Button>Browse catalog</Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {orders.map((order) => {
            const isOpen = expandedOrderId === order.orderId;
            const orderDetails = orderDetailsById[order.orderId];
            const orderStatus = orderDetails?.orderStatus ?? order.orderStatus;
            const orderItems = orderDetails?.orderItems ?? order.orderItems;
            const shippingAddress = orderDetails?.shippingAddress ?? order.shippingAddress;
            const appliedPromoCode = orderDetails?.appliedPromoCode ?? order.appliedPromoCode;
            const promoDiscount = orderDetails?.promoCodeDiscountAmount ?? order.promoCodeDiscountAmount;
            const bonusRedeemed = orderDetails?.bonusPointsRedeemed ?? order.bonusPointsRedeemed;
            const bonusEarned = orderDetails?.bonusPointsEarned ?? order.bonusPointsEarned;

            return (
              <Card key={order.orderId} className="overflow-hidden rounded-[1.6rem] border-border/70 bg-card/88">
                <button
                  type="button"
                  className="flex w-full items-center justify-between gap-4 p-5 text-left transition-colors hover:bg-background/40"
                  onClick={() =>
                    setExpandedOrderId((current) =>
                      current === order.orderId ? null : order.orderId
                    )
                  }
                >
                  <div className="space-y-1">
                    <div className="text-xl font-semibold tracking-tight">
                      Order #{order.orderId}
                    </div>
                    <div className="text-sm text-muted-foreground">
                      {new Date(order.orderDate).toLocaleDateString("en-US", {
                        day: "numeric",
                        month: "long",
                        year: "numeric",
                      })}
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <Badge variant={getStatusVariant(orderStatus)}>
                      {getStatusText(orderStatus)}
                    </Badge>
                    <div className="rounded-full bg-primary/10 px-3 py-1 text-sm font-semibold text-primary">
                      {order.itemsCount} item{order.itemsCount === 1 ? "" : "s"}
                    </div>
                    <div className="rounded-full border border-border/70 bg-background/70 px-3 py-1 text-sm font-semibold">
                      {formatCurrency(order.totalAmount)}
                    </div>
                    <ChevronDown
                      className={`h-4 w-4 text-muted-foreground transition-transform ${
                        isOpen ? "rotate-180" : ""
                      }`}
                    />
                  </div>
                </button>

                {isOpen && (
                  <>
                    <Separator />
                    <CardContent className="space-y-5 p-5">
                      <div className="grid gap-3 md:grid-cols-2">
                        <Card className="rounded-2xl border-border/70 bg-background/70">
                          <CardHeader className="pb-2">
                            <CardTitle className="flex items-center gap-2 text-base">
                              <MapPin className="h-4 w-4 text-primary" />
                              Delivery address
                            </CardTitle>
                          </CardHeader>
                          <CardContent className="text-sm text-muted-foreground">
                            {formatOrderAddress(shippingAddress)}
                          </CardContent>
                        </Card>

                        <Card className="rounded-2xl border-border/70 bg-background/70">
                          <CardHeader className="pb-2">
                            <CardTitle className="flex items-center gap-2 text-base">
                              <Gift className="h-4 w-4 text-primary" />
                              Discounts and rewards
                            </CardTitle>
                          </CardHeader>
                          <CardContent className="space-y-1 text-sm text-muted-foreground">
                            <p>
                              Promo code:{" "}
                              <span className="font-medium text-foreground">
                                {appliedPromoCode || "Not used"}
                              </span>
                            </p>
                            <p>
                              Promo discount:{" "}
                              <span className="font-medium text-foreground">
                                {formatCurrency(promoDiscount || 0)}
                              </span>
                            </p>
                            <p>
                              Bonus points redeemed:{" "}
                              <span className="font-medium text-foreground">
                                {bonusRedeemed || 0}
                              </span>
                            </p>
                            <p>
                              Bonus points earned:{" "}
                              <span className="font-medium text-foreground">
                                {bonusEarned || 0}
                              </span>
                            </p>
                          </CardContent>
                        </Card>
                      </div>

                      {orderStatus === "Pending" && (
                        <div className="flex justify-end">
                          <Button
                            type="button"
                            variant="destructive"
                            onClick={() => {
                              setOrderToCancelId(order.orderId);
                              setCancelError(null);
                              setCancelMessage(null);
                            }}
                            disabled={cancelLoading && orderToCancelId === order.orderId}
                          >
                            Cancel order
                          </Button>
                        </div>
                      )}

                      {detailsLoadingId === order.orderId ? (
                        <p className="text-sm text-muted-foreground">
                          Loading order details...
                        </p>
                      ) : (
                        <div className="space-y-3">
                          {orderItems.map((item, index) => (
                            <div
                              key={`${order.orderId}-${item.productId ?? "item"}-${index}`}
                              className="flex items-start justify-between gap-4 rounded-[1.2rem] border border-border/70 bg-background/70 px-4 py-3"
                            >
                              <div className="flex gap-3">
                                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                  <Package className="h-5 w-5" />
                                </div>
                                <div>
                                  <div className="font-medium">{item.productName}</div>
                                  <div className="text-sm text-muted-foreground">
                                    Quantity: {item.quantity} x {formatCurrency(item.price)}
                                  </div>
                                </div>
                              </div>
                              <div className="font-semibold">
                                {formatCurrency(item.subtotal ?? item.price * item.quantity)}
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                    </CardContent>
                  </>
                )}
              </Card>
            );
          })}
        </div>
      )}

      <Dialog
        open={orderToCancelId !== null}
        onOpenChange={(open) => {
          if (!open && !cancelLoading) {
            setOrderToCancelId(null);
          }
        }}
      >
        <DialogContent className="max-w-md" showCloseButton={!cancelLoading}>
          <DialogHeader>
            <DialogTitle>Cancel this order?</DialogTitle>
            <DialogDescription>
              {orderToCancel
                ? `Order #${orderToCancel.orderId} will be cancelled if the pharmacy has not confirmed it yet.`
                : "This order will be cancelled if it is still pending."}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => setOrderToCancelId(null)}
              disabled={cancelLoading}
            >
              Keep order
            </Button>
            <Button
              type="button"
              variant="destructive"
              onClick={() => void cancelOrder()}
              disabled={cancelLoading}
            >
              {cancelLoading ? "Cancelling..." : "Cancel order"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
