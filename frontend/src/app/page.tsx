"use client";

import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { productApi } from "@/entities/product";
import { promotionApi } from "@/entities/promotion";
import type { PromotionDetailsDto } from "@/entities/promotion";
import { ProductCard } from "@/entities/product/ui/product-card";
import { useCart } from "@/features/cart/use-cart";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ProductDto } from "@/shared/types";
import {
  ArrowRight,
  BadgePercent,
  ChevronLeft,
  ChevronRight,
  HeartPulse,
  ShieldCheck,
  Sparkles,
  Truck,
} from "lucide-react";

const benefits = [
  {
    icon: ShieldCheck,
    title: "Trusted assortment",
    text: "A clean catalog with clear availability and pricing.",
  },
  {
    icon: Truck,
    title: "Fast ordering flow",
    text: "Go from product page to checkout without extra screens.",
  },
  {
    icon: HeartPulse,
    title: "Health essentials",
    text: "Medicines, vitamins, and daily wellness products in one place.",
  },
];

function seededScore(value: string, seed: number): number {
  const hash = value
    .split("")
    .reduce((accumulator, character) => accumulator + character.charCodeAt(0), 0);
  const score = Math.sin(hash * 12989.37 + seed * 78233.91) * 43758.5453;
  return score - Math.floor(score);
}

export default function HomePage() {
  const { addItem, quantityByProductId } = useCart();
  const savingsTrackRef = useRef<HTMLDivElement | null>(null);
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [savingsProducts, setSavingsProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [discountShuffleSeed] = useState(() => Math.random());

  useEffect(() => {
    const loadHome = async () => {
      setLoading(true);

      try {
        const [productsResult, promotions] = await Promise.all([
          productApi.getAll({ pageIndex: 1, pageSize: 24 }),
          promotionApi.getAll(),
        ]);

        setProducts(productsResult.data?.items ?? []);

        let discountedItems: ProductDto[] = [];

        if (promotions.length > 0) {
          const shuffledPromotions = [...promotions]
            .sort((left, right) => {
              return seededScore(left.slug, discountShuffleSeed) - seededScore(right.slug, discountShuffleSeed);
            })
            .slice(0, 6);

          const details = await Promise.allSettled(
            shuffledPromotions.map((promotion) => promotionApi.getBySlug(promotion.slug)),
          );

          discountedItems = details
            .map((result) => (result.status === "fulfilled" ? result.value : null))
            .filter((result): result is PromotionDetailsDto => result !== null)
            .flatMap((result) => result.products)
            .filter((product) => product.discountedPrice != null && product.discountedPrice < product.price);
        }

        // Fallback when promotions have no product list yet.
        if (discountedItems.length === 0) {
          const saleProductsResult = await productApi.getAll({
            pageIndex: 1,
            pageSize: 48,
            saleOnly: true,
          });

          discountedItems = (saleProductsResult.data?.items ?? []).filter(
            (product) =>
              product.discountedPrice != null &&
              product.discountedPrice < product.price,
          );
        }

        // Fallback when saleOnly is ignored by backend or first pages are sparse.
        if (discountedItems.length === 0) {
          const fallbackItems: ProductDto[] = [];

          for (let pageIndex = 1; pageIndex <= 5; pageIndex += 1) {
            const fallbackResult = await productApi.getAll({ pageIndex, pageSize: 48 });
            const currentPageItems = (fallbackResult.data?.items ?? []).filter(
              (product) =>
                product.discountedPrice != null &&
                product.discountedPrice < product.price,
            );

            fallbackItems.push(...currentPageItems);

            if (fallbackItems.length >= 12 || !fallbackResult.data?.hasNextPage) {
              break;
            }
          }

          discountedItems = fallbackItems;
        }

        const uniqueProducts = new Map<number, ProductDto>();
        for (const product of discountedItems) {
          if (!uniqueProducts.has(product.id)) {
            uniqueProducts.set(product.id, product);
          }
        }

        setSavingsProducts([...uniqueProducts.values()]);
      } catch {
        setProducts([]);
        setSavingsProducts([]);
      } finally {
        setLoading(false);
      }
    };

    void loadHome();
  }, [discountShuffleSeed]);

  const featuredProducts = [...products]
    .sort((left, right) => right.wishlistCount - left.wishlistCount)
    .slice(0, 4);
  const discountedProductsAll = savingsProducts
    .filter(
      (product) =>
        product.discountedPrice != null &&
        product.discountedPrice < product.price,
    );
  const discountedProducts = [...discountedProductsAll]
    .sort((left, right) => {
      const leftScore = Math.sin(left.id * 12989.37 + discountShuffleSeed * 78233.91) * 43758.5453;
      const rightScore = Math.sin(right.id * 12989.37 + discountShuffleSeed * 78233.91) * 43758.5453;
      return (leftScore - Math.floor(leftScore)) - (rightScore - Math.floor(rightScore));
    })
    .slice(0, 12);

  const scrollSavings = (direction: "prev" | "next") => {
    const track = savingsTrackRef.current;
    if (!track) {
      return;
    }

    const delta = Math.round(track.clientWidth * 0.9);
    track.scrollBy({
      left: direction === "next" ? delta : -delta,
      behavior: "smooth",
    });
  };

  return (
    <div className="pb-16">
      <section className="container mx-auto px-4 pt-8">
        <div className="grid gap-6 lg:grid-cols-[1.35fr,0.9fr]">
          <Card className="overflow-hidden border-none bg-[linear-gradient(135deg,rgba(27,149,162,0.95),rgba(115,196,155,0.9),rgba(255,244,221,0.9))] text-primary-foreground shadow-[0_30px_80px_rgba(21,65,77,0.18)]">
            <CardContent className="relative px-6 py-8 sm:px-10 sm:py-12">
              <div className="absolute right-6 top-6 rounded-full border border-white/30 bg-white/10 px-4 py-1 text-xs uppercase tracking-[0.3em] text-white/80">
                Online pharmacy
              </div>
              <div className="max-w-2xl space-y-6">
                <div className="inline-flex items-center gap-2 rounded-full bg-white/16 px-4 py-2 text-sm text-white/90">
                  <Sparkles className="h-4 w-4" />
                  Daily essentials, vitamins, and medicines in one storefront
                </div>
                <div className="space-y-4">
                  <h1 className="max-w-xl text-4xl font-semibold tracking-tight sm:text-5xl">
                    Pharmacy storefront designed around a fast ordering flow.
                  </h1>
                  <p className="max-w-xl text-base text-white/88 sm:text-lg">
                    Browse the catalog, check availability, add what you need,
                    and complete the order in a single clear path.
                  </p>
                </div>
                <div className="flex flex-col gap-3 sm:flex-row">
                  <Link href="/catalog">
                    <Button
                      size="lg"
                      className="min-w-44 bg-white text-slate-900 hover:bg-white/92"
                    >
                      Browse catalog
                      <ArrowRight className="ml-2 h-4 w-4" />
                    </Button>
                  </Link>
                  <Link href="#featured">
                    <Button
                      size="lg"
                      variant="outline"
                      className="min-w-44 border-white/35 bg-white/10 text-white hover:bg-white/18 hover:text-white"
                    >
                      View featured picks
                    </Button>
                  </Link>
                </div>
                <div className="grid gap-3 pt-2 text-sm text-white/88 sm:grid-cols-3">
                  <div className="rounded-2xl border border-white/20 bg-white/10 px-4 py-3">
                    Clear availability
                  </div>
                  <div className="rounded-2xl border border-white/20 bg-white/10 px-4 py-3">
                    Checkout in minutes
                  </div>
                  <div className="rounded-2xl border border-white/20 bg-white/10 px-4 py-3">
                    Order history included
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          <div className="grid gap-6">
            <Card className="border-primary/15 bg-card/80 shadow-sm">
              <CardContent className="space-y-3 p-6">
                <div className="inline-flex rounded-full bg-primary/12 p-2 text-primary">
                  <BadgePercent className="h-5 w-5" />
                </div>
                <div>
                  <h2 className="text-xl font-semibold">Deals and price drops</h2>
                  <p className="mt-2 text-sm text-muted-foreground">
                    Build a dedicated section for discounted products without
                    touching backend contracts.
                  </p>
                </div>
                <Link href="/promotions">
                  <Button variant="ghost" className="px-0 text-primary hover:bg-transparent">
                    Explore promotions
                    <ArrowRight className="ml-2 h-4 w-4" />
                  </Button>
                </Link>
              </CardContent>
            </Card>
          </div>
        </div>
      </section>

      <section className="container mx-auto px-4 pt-10">
        <div className="grid gap-4 md:grid-cols-3">
          {benefits.map(({ icon: Icon, title, text }) => (
            <Card key={title} className="border-border/70 bg-card/78">
              <CardContent className="flex gap-4 p-5">
                <div className="mt-1 rounded-2xl bg-primary/12 p-3 text-primary">
                  <Icon className="h-5 w-5" />
                </div>
                <div>
                  <h2 className="mb-1 text-lg font-semibold">{title}</h2>
                  <p className="text-sm text-muted-foreground">{text}</p>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </section>

      <section className="container mx-auto px-4 pt-12" id="featured">
        <div className="mb-6 flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-primary">
              Featured
            </p>
            <h2 className="mt-2 text-3xl font-semibold tracking-tight">
              Popular items from the current catalog
            </h2>
          </div>
          <Link href="/catalog">
            <Button variant="outline">
              Open full catalog
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </Link>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-4">
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton key={index} className="h-[26rem] rounded-[2rem]" />
            ))}
          </div>
        ) : featuredProducts.length > 0 ? (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-4">
            {featuredProducts.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                onAdd={addItem}
                quantityInCart={quantityByProductId(product.id)}
              />
            ))}
          </div>
        ) : (
          <EmptyHomeState
            title="Catalog is available, but featured picks are still empty."
            linkHref="/catalog"
          />
        )}
      </section>

      <section className="container mx-auto px-4 pt-12">
        <div className="mb-6 flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-primary">
              Savings
            </p>
            <h2 className="mt-2 text-3xl font-semibold tracking-tight">
              Random sale products from active promotions
            </h2>
          </div>
          <Link href="/promotions">
            <Button variant="ghost" className="px-0 text-primary hover:bg-transparent">
              See all promotions
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </Link>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-4">
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton key={index} className="h-[22rem] rounded-[2rem]" />
            ))}
          </div>
        ) : discountedProducts.length > 0 ? (
          <div className="space-y-4">
            <div className="flex items-center justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => scrollSavings("prev")}
                className="rounded-full"
              >
                <ChevronLeft className="mr-1 h-4 w-4" />
                Prev
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => scrollSavings("next")}
                className="rounded-full"
              >
                Next
                <ChevronRight className="ml-1 h-4 w-4" />
              </Button>
            </div>

            <div
              ref={savingsTrackRef}
              className="flex snap-x snap-mandatory gap-4 overflow-x-auto pb-2 [scrollbar-width:thin]"
            >
              {discountedProducts.map((product) => (
                <div
                  key={product.id}
                  className="min-w-[82%] snap-start sm:min-w-[48%] lg:min-w-[36%] xl:min-w-[30%] 2xl:min-w-[24%]"
                >
                  <ProductCard
                    product={product}
                    onAdd={addItem}
                    quantityInCart={quantityByProductId(product.id)}
                  />
                </div>
              ))}
            </div>
          </div>
        ) : (
          <Card className="border-dashed border-primary/35 bg-[linear-gradient(120deg,rgba(255,255,255,0.92),rgba(246,255,250,0.9))]">
            <CardContent className="flex flex-col gap-4 p-8 text-center sm:items-center">
              <h3 className="flex items-center justify-center gap-2 text-xl font-semibold">
                <BadgePercent className="h-5 w-5 text-primary" />
                No active sale products right now.
              </h3>
              <p className="max-w-2xl text-sm text-muted-foreground">
                Add or activate promotions in admin, and this block will be
                filled with random discounted products automatically.
              </p>
              <Link href="/promotions">
                <Button variant="outline">
                  Open promotions page
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              </Link>
            </CardContent>
          </Card>
        )}
      </section>
    </div>
  );
}

function EmptyHomeState({
  title,
  linkHref,
}: {
  title: string;
  linkHref: string;
}) {
  return (
    <Card className="border-dashed border-primary/30 bg-background/70">
      <CardContent className="flex flex-col gap-4 p-8 text-center sm:items-center">
        <h3 className="text-xl font-semibold">{title}</h3>
        <p className="max-w-2xl text-sm text-muted-foreground">
          The storefront layout is in place. Once the catalog has more products,
          these editorial sections will populate from the same frontend data
          already in use.
        </p>
        <Link href={linkHref}>
          <Button>Open catalog</Button>
        </Link>
      </CardContent>
    </Card>
  );
}
