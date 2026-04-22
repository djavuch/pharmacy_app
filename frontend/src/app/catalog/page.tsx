"use client";

import { useState, useEffect } from "react";
import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { productApi, categoryApi } from "@/entities/product";
import { ProductDto, CategoryDto } from "@/shared/types";
import { useCart } from "@/features/cart/use-cart";
import { ProductCard } from "@/entities/product/ui/product-card";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";

function CatalogContent() {
  const searchParams = useSearchParams();
  const { addItem, quantityByProductId } = useCart();
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [categoryName, setCategoryName] = useState<string | null>(null);
  const [saleOnly, setSaleOnly] = useState(false);
  const [page, setPage] = useState(1);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);

  // Initialize from URL query params
  useEffect(() => {
    const searchQuery = searchParams.get("search");
    const category = searchParams.get("category");
    const sale = searchParams.get("sale");
    setSearch(searchQuery?.trim() ?? "");
    setCategoryName(category && category.trim().length > 0 ? category : null);
    setSaleOnly(sale === "true");
    setPage(1);
  }, [searchParams]);

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const result = await categoryApi.getAll({ pageIndex: 1, pageSize: 50 });
        setCategories(result.items ?? []);
      } catch {
        setCategories([]);
      }
    };
    fetchCategories();
  }, []);

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      try {
        const result = await productApi.getAll({
          pageIndex: page,
          pageSize: 12,
          filterOn: search ? "Name" : undefined,
          filterQuery: search || undefined,
          categoryName: categoryName ?? undefined,
          saleOnly: saleOnly || undefined,
        });
        let items = result.data?.items ?? [];
        // Fallback client-side filtering until backend sale/category filtering is enabled.
        if (categoryName) {
          items = items.filter((p) => p.categoryName === categoryName);
        }
        if (saleOnly) {
          items = items.filter(
            (p) => p.discountedPrice != null && p.discountedPrice < p.price,
          );
        }
        setProducts(items);
        setTotalPages(result.data?.totalPages ?? 1);
      } catch {
        setProducts([]);
      } finally {
        setLoading(false);
      }
    };
    fetchProducts();
  }, [search, categoryName, page, saleOnly]);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6 flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.28em] text-primary">
            Catalog
          </p>
          <h1 className="mt-2 text-3xl font-bold">Browse the full assortment</h1>
        </div>
        {saleOnly && (
          <div className="rounded-full bg-primary/10 px-4 py-2 text-sm font-semibold text-primary">
            Sale filter is active
          </div>
        )}
      </div>

      {/* Filters */}
      <div className="mb-6 flex flex-col gap-4 md:flex-row">
        <Input
          placeholder="Search medicines..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          className="md:w-64"
        />
        <Select
          value={categoryName ?? "all"}
          onValueChange={(v) => {
            setCategoryName(v === "all" ? null : v);
            setPage(1);
          }}
        >
          <SelectTrigger className="md:w-48">
            <SelectValue placeholder="Category" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Categories</SelectItem>
            {categories.map((cat) => (
              <SelectItem key={cat.categoryId} value={cat.categoryName}>
                {cat.categoryName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Product Grid */}
      {loading ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-80 w-full rounded-lg" />
          ))}
        </div>
      ) : products.length > 0 ? (
        <>
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {products.map((product) => (
              <ProductCard key={product.id} product={product}
                           onAdd={addItem} quantityInCart={quantityByProductId(product.id)} />
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="mt-8 flex items-center justify-center gap-2">
              <button
                disabled={page === 1}
                onClick={() => setPage((p) => p - 1)}
                className="rounded border px-3 py-1 disabled:opacity-50"
              >
                Previous
              </button>
              <span className="px-3 py-1">
                {page} of {totalPages}
              </span>
              <button
                disabled={page === totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="rounded border px-3 py-1 disabled:opacity-50"
              >
                Next
              </button>
            </div>
          )}
        </>
      ) : (
        <div className="py-12 text-center text-muted-foreground">
          No products found for the selected filters
        </div>
      )}
    </div>
  );
}

export default function CatalogPage() {
  return (
    <Suspense fallback={
      <div className="container mx-auto px-4 py-8">
        <Skeleton className="mb-6 h-10 w-48" />
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-80 w-full rounded-lg" />
          ))}
        </div>
      </div>
    }>
      <CatalogContent />
    </Suspense>
  );
}
