import type { OrderStatus } from "@/shared/types";

export const adminTabs = [
  "dashboard",
  "products",
  "orders",
  "users",
  "reviews",
  "categories",
  "contentPages",
  "discounts",
  "promoCodes",
  "bonuses",
] as const;

export type AdminTab = (typeof adminTabs)[number];

export const orderSortOptions = [
  "date_desc",
  "date_asc",
  "status_desc",
  "status_asc",
  "customer_desc",
  "customer_asc",
] as const;

export type OrdersSort = (typeof orderSortOptions)[number];

export const orderStatuses: OrderStatus[] = [
  "Pending",
  "Processing",
  "Shipped",
  "Delivered",
  "Cancelled",
];

export const userSearchByOptions = [
  "UserName",
  "Email",
  "FirstName",
  "LastName",
] as const;

export type UserSearchBy = (typeof userSearchByOptions)[number];

export const userSortOptions = [
  "createdat_desc",
  "createdat_asc",
  "email_asc",
  "email_desc",
  "username_asc",
  "username_desc",
] as const;

export type UsersSort = (typeof userSortOptions)[number];

export const userRoles = ["Admin", "Manager", "Pharmacist", "Customer"] as const;
