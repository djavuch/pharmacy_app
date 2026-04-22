"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { promotionApi } from "@/entities/promotion";
import type { PromotionListItemDto } from "@/entities/promotion";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { ArrowRight, BadgePercent, CalendarClock, Package } from "lucide-react";

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "2-digit",
  year: "numeric",
});

function formatDiscountValue(promotion: PromotionListItemDto): string {
  if (promotion.discountType === "Percentage") {
    return `${promotion.value}%`;
  }

  return `$${promotion.value.toFixed(2)}`;
}

function formatDateRange(startDate: string, endDate: string): string {
  return `${dateFormatter.format(new Date(startDate))} - ${dateFormatter.format(new Date(endDate))}`;
}

export default function PromotionsPage() {
  const [promotions, setPromotions] = useState<PromotionListItemDto[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadPromotions = async () => {
      setLoading(true);
      setError(null);

      try {
        const result = await promotionApi.getAll();
        setPromotions(result);
      } catch (err) {
        setPromotions([]);
        setError(err instanceof Error ? err.message : "Failed to load promotions.");
      } finally {
        setLoading(false);
      }
    };

    void loadPromotions();
  }, []);

  const filteredPromotions = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) {
      return promotions;
    }

    return promotions.filter((promotion) =>
      [promotion.name, promotion.description]
        .join(" ")
        .toLowerCase()
        .includes(query),
    );
  }, [promotions, search]);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.28em] text-primary">Promotions</p>
          <h1 className="mt-2 text-3xl font-bold">Current pharmacy deals</h1>
          <p className="mt-2 max-w-2xl text-sm text-muted-foreground">
            Active campaigns with discounted products and categories.
          </p>
        </div>
        <div className="rounded-full bg-primary/10 px-4 py-2 text-sm font-semibold text-primary">
          {loading ? "Loading..." : `${filteredPromotions.length} active`}
        </div>
      </div>

      <div className="mb-6">
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search promotions..."
          className="max-w-md"
        />
      </div>

      {loading ? (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 6 }).map((_, index) => (
            <Skeleton key={index} className="h-64 rounded-2xl" />
          ))}
        </div>
      ) : error ? (
        <Card className="border-dashed border-destructive/40 bg-destructive/5">
          <CardContent className="p-6 text-sm text-destructive">{error}</CardContent>
        </Card>
      ) : filteredPromotions.length > 0 ? (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
          {filteredPromotions.map((promotion) => (
            <Card key={promotion.discountId} className="flex h-full flex-col">
              <CardHeader className="space-y-3">
                <div className="flex items-start justify-between gap-2">
                  <CardTitle className="line-clamp-2 text-xl">{promotion.name}</CardTitle>
                  <Badge className="shrink-0 bg-primary/15 text-primary">
                    {formatDiscountValue(promotion)}
                  </Badge>
                </div>
                <p className="line-clamp-3 text-sm text-muted-foreground">
                  {promotion.description?.trim() || "No description provided."}
                </p>
              </CardHeader>

              <CardContent className="flex flex-1 flex-col gap-3 text-sm text-muted-foreground">
                <div className="flex items-center gap-2">
                  <CalendarClock className="h-4 w-4 text-primary" />
                  <span>{formatDateRange(promotion.startDate, promotion.endDate)}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Package className="h-4 w-4 text-primary" />
                  <span>
                    Products: {promotion.productTargetsCount}, Categories: {promotion.categoryTargetsCount}
                  </span>
                </div>
              </CardContent>

              <CardFooter>
                <Link href={`/promotions/${promotion.slug}`} className="w-full">
                  <Button className="w-full">
                    Open details
                    <ArrowRight className="ml-2 h-4 w-4" />
                  </Button>
                </Link>
              </CardFooter>
            </Card>
          ))}
        </div>
      ) : (
        <Card className="border-dashed border-primary/30 bg-background/70">
          <CardContent className="flex flex-col items-start gap-4 p-8">
            <h2 className="flex items-center gap-2 text-xl font-semibold">
              <BadgePercent className="h-5 w-5 text-primary" />
              No active promotions right now
            </h2>
            <p className="text-sm text-muted-foreground">
              Activate discounts in admin and this page will fill automatically.
            </p>
            <Link href="/catalog">
              <Button variant="outline">Open catalog</Button>
            </Link>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
