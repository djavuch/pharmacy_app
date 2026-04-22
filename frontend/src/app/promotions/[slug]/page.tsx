"use client";

import Link from "next/link";
import { use, useEffect, useMemo, useState } from "react";
import { ProductCard } from "@/entities/product/ui/product-card";
import { promotionApi } from "@/entities/promotion";
import type { PromotionDetailsDto } from "@/entities/promotion";
import { useCart } from "@/features/cart/use-cart";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { ArrowLeft, CalendarClock, Package } from "lucide-react";

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "2-digit",
  year: "numeric",
});

function formatDiscountValue(promotion: PromotionDetailsDto): string {
  if (promotion.discountType === "Percentage") {
    return `${promotion.value}%`;
  }

  return `$${promotion.value.toFixed(2)}`;
}

function formatDateRange(startDate: string, endDate: string): string {
  return `${dateFormatter.format(new Date(startDate))} - ${dateFormatter.format(new Date(endDate))}`;
}

export default function PromotionDetailsPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = use(params);
  const { addItem, quantityByProductId } = useCart();
  const [promotion, setPromotion] = useState<PromotionDetailsDto | null>(null);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isCancelled = false;

    const loadPromotion = async () => {
      setLoading(true);
      setError(null);

      try {
        const result = await promotionApi.getBySlug(slug);
        if (isCancelled) {
          return;
        }

        setPromotion(result);
      } catch (err) {
        if (isCancelled) {
          return;
        }

        setPromotion(null);
        setError(err instanceof Error ? err.message : "Failed to load promotion details.");
      } finally {
        if (!isCancelled) {
          setLoading(false);
        }
      }
    };

    void loadPromotion();

    return () => {
      isCancelled = true;
    };
  }, [slug]);

  const filteredProducts = useMemo(() => {
    if (!promotion) {
      return [];
    }

    const query = search.trim().toLowerCase();
    if (!query) {
      return promotion.products;
    }

    return promotion.products.filter((product) =>
      [product.name, product.categoryName, product.description]
        .filter(Boolean)
        .join(" ")
        .toLowerCase()
        .includes(query),
    );
  }, [promotion, search]);

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Skeleton className="mb-4 h-6 w-56" />
        <Skeleton className="mb-8 h-44 w-full rounded-2xl" />
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, index) => (
            <Skeleton key={index} className="h-[22rem] rounded-[2rem]" />
          ))}
        </div>
      </div>
    );
  }

  if (!promotion) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card className="border-dashed border-destructive/40 bg-destructive/5">
          <CardContent className="space-y-4 p-6">
            <h1 className="text-xl font-semibold">Promotion not found</h1>
            <p className="text-sm text-muted-foreground">
              {error ?? "The requested promotion could not be loaded."}
            </p>
            <Link href="/promotions">
              <Button variant="outline">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Back to promotions
              </Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6">
        <Link href="/promotions">
          <Button variant="ghost" className="px-0 text-primary hover:bg-transparent">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to promotions
          </Button>
        </Link>
      </div>

      <Card className="mb-8 border-primary/20 bg-[linear-gradient(135deg,rgba(27,149,162,0.09),rgba(115,196,155,0.1),rgba(255,244,221,0.5))]">
        <CardHeader className="space-y-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <p className="text-sm uppercase tracking-[0.22em] text-primary">Promotion</p>
              <CardTitle className="mt-2 text-3xl">{promotion.name}</CardTitle>
            </div>
            <Badge className="bg-primary/15 text-primary">
              {formatDiscountValue(promotion)}
            </Badge>
          </div>
          <p className="max-w-3xl text-sm text-muted-foreground">
            {promotion.description?.trim() || "No description provided."}
          </p>
        </CardHeader>
        <CardContent className="grid gap-3 text-sm text-muted-foreground sm:grid-cols-2">
          <div className="inline-flex items-center gap-2 rounded-xl border border-border/70 bg-background/80 px-3 py-2">
            <CalendarClock className="h-4 w-4 text-primary" />
            {formatDateRange(promotion.startDate, promotion.endDate)}
          </div>
          <div className="inline-flex items-center gap-2 rounded-xl border border-border/70 bg-background/80 px-3 py-2">
            <Package className="h-4 w-4 text-primary" />
            {promotion.products.length} products in this promotion
          </div>
        </CardContent>
      </Card>

      <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <h2 className="text-2xl font-semibold">Products in this promotion</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Filter and add discounted items directly to cart.
          </p>
        </div>
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search products..."
          className="max-w-md"
        />
      </div>

      {filteredProducts.length > 0 ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-4">
          {filteredProducts.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              onAdd={addItem}
              quantityInCart={quantityByProductId(product.id)}
            />
          ))}
        </div>
      ) : (
        <Card className="border-dashed border-primary/35">
          <CardContent className="p-8 text-center text-sm text-muted-foreground">
            No products match current filters.
          </CardContent>
        </Card>
      )}
    </div>
  );
}
