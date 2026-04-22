import { useEffect, useState } from "react";
import { ShoppingCart, Package } from "lucide-react";
import { adminDashboardApi } from "@/shared/api";
import type { AdminDashboardSummaryDto } from "@/shared/types";
import { TabsContent } from "@/components/ui/tabs";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export function DashboardTab() {
  const [summary, setSummary] = useState<AdminDashboardSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;

    const fetchSummary = async () => {
      setLoading(true);
      setError(null);

      try {
        const result = await adminDashboardApi.getSummary();
        if (!ignore) {
          setSummary(result.data ?? null);
        }
      } catch (err) {
        if (!ignore) {
          setSummary(null);
          setError(err instanceof Error ? err.message : "Failed to load dashboard metrics");
        }
      } finally {
        if (!ignore) {
          setLoading(false);
        }
      }
    };

    void fetchSummary();

    return () => {
      ignore = true;
    };
  }, []);

  const currency = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    maximumFractionDigits: 2,
  });

  const totalProducts = loading ? "..." : summary ? summary.totalProducts.toLocaleString("en-US") : "—";
  const ordersToday = loading ? "..." : summary ? summary.ordersToday.toLocaleString("en-US") : "—";
  const monthlyRevenue = loading ? "..." : summary ? currency.format(summary.monthlyRevenue) : "—";

  return (
    <TabsContent value="dashboard">
      {error && (
        <div className="mb-4 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
          {error}
        </div>
      )}

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Products</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalProducts}</div>
            <p className="text-xs text-muted-foreground">Current catalog size</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Orders Today</CardTitle>
            <ShoppingCart className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{ordersToday}</div>
            <p className="text-xs text-muted-foreground">Created today (UTC)</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Monthly Revenue</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{monthlyRevenue}</div>
            <p className="text-xs text-muted-foreground">Current month, excluding cancelled</p>
          </CardContent>
        </Card>
      </div>
    </TabsContent>
  );
}
