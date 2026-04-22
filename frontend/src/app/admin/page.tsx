"use client";

import { Suspense, useEffect, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useAuthStore } from "@/entities/user";
import { getAccessToken } from "@/shared/api";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  BadgePercent,
  Coins,
  FileText,
  LayoutDashboard,
  MessageSquare,
  Package,
  Search,
  ShoppingCart,
  TicketPercent,
  Users,
} from "lucide-react";
import { DashboardTab } from "./_components/dashboard-tab";
import { ProductsTab } from "./_components/products-tab";
import { OrdersTab } from "./_components/orders-tab";
import { UsersTab } from "./_components/users-tab";
import { ReviewsTab } from "./_components/reviews-tab";
import { CategoriesTab } from "./_components/categories-tab";
import { ContentPagesTab } from "./_components/content-pages-tab";
import { DiscountsTab } from "./_components/discounts-tab";
import { PromoCodesTab } from "./_components/promo-codes-tab";
import { BonusTab } from "./_components/bonus-tab";
import { adminTabs, type AdminTab } from "./_components/admin-constants";
import { buildUrlWithQuery, getCurrentUrl, parseEnumParam } from "./_components/query-state";

export default function AdminPage() {
  return (
    <Suspense fallback={null}>
      <AdminPageContent />
    </Suspense>
  );
}

function AdminPageContent() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const { profile, isAuthenticated, loadProfile } = useAuthStore();
  const [authResolved, setAuthResolved] = useState(false);
  const activeTab: AdminTab = parseEnumParam(searchParams.get("tab"), adminTabs, "dashboard");
  const tabQueryKeys: Record<AdminTab, string[]> = {
    dashboard: [],
    products: ["productsSearch", "productsPage"],
    orders: ["ordersSearch", "ordersStatus", "ordersSort", "ordersPage"],
    users: ["usersSearch", "usersSearchBy", "usersSort", "usersPage"],
    reviews: ["reviewsSearch", "reviewsSearchBy", "reviewsStatus", "reviewsSort", "reviewsPage"],
    categories: [],
    contentPages: [],
    discounts: [],
    promoCodes: [],
    bonuses: [],
  };

  const handleTabChange = (nextValue: string) => {
    const nextTab = parseEnumParam(nextValue, adminTabs, "dashboard");
    const updates: Record<string, string | null> = { tab: nextTab };

    for (const tab of adminTabs) {
      if (tab === nextTab) {
        continue;
      }

      for (const key of tabQueryKeys[tab]) {
        updates[key] = null;
      }
    }

    const nextUrl = buildUrlWithQuery(pathname, searchParams, updates);
    const currentUrl = getCurrentUrl(pathname, searchParams);

    if (nextUrl !== currentUrl) {
      router.replace(nextUrl, { scroll: false });
    }
  };

  useEffect(() => {
    let ignore = false;

    const resolveAuth = async () => {
      const token = await getAccessToken();

      if (!token) {
        if (!ignore) {
          setAuthResolved(true);
          router.replace("/login?redirect=/admin");
        }
        return;
      }

      try {
        await loadProfile();
      } finally {
        if (!ignore) {
          setAuthResolved(true);
        }
      }
    };

    void resolveAuth();

    return () => {
      ignore = true;
    };
  }, [loadProfile, router]);

  useEffect(() => {
    if (!authResolved) {
      return;
    }

    if (!isAuthenticated) {
      router.replace("/login?redirect=/admin");
      return;
    }

    const isAdmin = profile?.role === "Admin";
    if (!isAdmin) {
      router.replace("/");
    }
  }, [authResolved, isAuthenticated, profile, router]);

  if (!authResolved || !isAuthenticated) {
    return null;
  }

  const isAdmin = profile?.role === "Admin";
  if (!isAdmin) {
    return null;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-6 text-3xl font-bold">Admin Panel</h1>

      <Tabs value={activeTab} onValueChange={handleTabChange}>
        <TabsList className="mb-6 h-auto flex-wrap justify-start">
          <TabsTrigger value="dashboard">
            <LayoutDashboard className="mr-2 h-4 w-4" />
            Dashboard
          </TabsTrigger>
          <TabsTrigger value="products">
            <Package className="mr-2 h-4 w-4" />
            Products
          </TabsTrigger>
          <TabsTrigger value="orders">
            <ShoppingCart className="mr-2 h-4 w-4" />
            Orders
          </TabsTrigger>
          <TabsTrigger value="users">
            <Users className="mr-2 h-4 w-4" />
            Users
          </TabsTrigger>
          <TabsTrigger value="reviews">
            <MessageSquare className="mr-2 h-4 w-4" />
            Reviews
          </TabsTrigger>
          <TabsTrigger value="categories">
            <Search className="mr-2 h-4 w-4" />
            Categories
          </TabsTrigger>
          <TabsTrigger value="contentPages">
            <FileText className="mr-2 h-4 w-4" />
            Pages
          </TabsTrigger>
          <TabsTrigger value="discounts">
            <BadgePercent className="mr-2 h-4 w-4" />
            Discounts
          </TabsTrigger>
          <TabsTrigger value="promoCodes">
            <TicketPercent className="mr-2 h-4 w-4" />
            Promo Codes
          </TabsTrigger>
          <TabsTrigger value="bonuses">
            <Coins className="mr-2 h-4 w-4" />
            Bonuses
          </TabsTrigger>
        </TabsList>

        <DashboardTab />
        <ProductsTab isActive={activeTab === "products"} />
        <OrdersTab isActive={activeTab === "orders"} />
        <UsersTab isActive={activeTab === "users"} />
        <ReviewsTab isActive={activeTab === "reviews"} />
        <CategoriesTab />
        <ContentPagesTab />
        <DiscountsTab />
        <PromoCodesTab />
        <BonusTab />
      </Tabs>
    </div>
  );
}
