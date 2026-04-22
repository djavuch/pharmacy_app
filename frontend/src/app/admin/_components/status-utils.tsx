import { Star } from "lucide-react";
import type { OrderStatus, ReviewStatus } from "@/shared/types";

export function getStatusText(status: string): string {
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

export function getOrderStatusBadgeClass(status: OrderStatus): string {
  if (status === "Delivered") return "bg-emerald-100 text-emerald-800";
  if (status === "Cancelled") return "bg-rose-100 text-rose-800";
  if (status === "Shipped") return "bg-blue-100 text-blue-800";
  if (status === "Processing") return "bg-amber-100 text-amber-800";
  return "bg-slate-100 text-slate-800";
}

export function getReviewStatusBadgeClass(status: ReviewStatus): string {
  if (status === "Approved") return "bg-green-100 text-green-800";
  if (status === "Rejected") return "bg-rose-100 text-rose-800";
  return "bg-amber-100 text-amber-800";
}

export function getUserRoleBadgeClass(role?: string): string {
  if (!role) return "bg-slate-100 text-slate-800";
  if (role === "Admin") return "bg-violet-100 text-violet-800";
  if (role === "Manager") return "bg-blue-100 text-blue-800";
  if (role === "Pharmacist") return "bg-emerald-100 text-emerald-800";
  return "bg-slate-100 text-slate-800";
}

export function renderStars(rating: number) {
  return Array.from({ length: 5 }).map((_, index) => (
    <Star
      key={`review-star-${rating}-${index}`}
      className={`h-3.5 w-3.5 ${
        index < rating ? "fill-amber-400 text-amber-400" : "text-muted-foreground/30"
      }`}
    />
  ));
}
