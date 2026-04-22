"use client";

import { FormEvent, use, useEffect, useState } from "react";
import Link from "next/link";
import { productApi, reviewApi } from "@/entities/product";
import { useCart } from "@/features/cart/use-cart";
import { getAccessToken, wishlistApi } from "@/shared/api";
import { ProductDto, ProductReviewDto } from "@/shared/types";
import { ResilientImage } from "@/shared/ui/resilient-image";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import {
  ChevronRight,
  Heart,
  Minus,
  Package,
  Plus,
  ShoppingCart,
  Star,
} from "lucide-react";

const currencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  maximumFractionDigits: 2,
});

function formatPrice(value: number): string {
  return currencyFormatter.format(value);
}

function renderStars(rating: number, sizeClass = "h-4 w-4") {
  const rounded = Math.round(rating);
  return Array.from({ length: 5 }).map((_, index) => (
    <Star
      key={`star-${sizeClass}-${index}`}
      className={`${sizeClass} ${
        index < rounded ? "fill-amber-400 text-amber-400" : "text-muted-foreground/40"
      }`}
    />
  ));
}

export default function ProductPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [relatedProducts, setRelatedProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [isInWishlist, setIsInWishlist] = useState(false);
  const [wishlistLoading, setWishlistLoading] = useState(false);
  const [wishlistMessage, setWishlistMessage] = useState<string | null>(null);
  const [reviews, setReviews] = useState<ProductReviewDto[]>([]);
  const [reviewsLoading, setReviewsLoading] = useState(false);
  const [reviewsError, setReviewsError] = useState<string | null>(null);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewContent, setReviewContent] = useState("");
  const [reviewSubmitting, setReviewSubmitting] = useState(false);
  const [reviewMessage, setReviewMessage] = useState<string | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const { addItem, quantityByProductId } = useCart();

  useEffect(() => {
    let isCancelled = false;

    const fetchRelatedProducts = async (currentProduct: ProductDto) => {
      let sameCategory: ProductDto[] = [];

      if (currentProduct.categoryName.trim()) {
        try {
          const byCategory = await productApi.getByCategory(currentProduct.categoryName, {
            pageIndex: 1,
            pageSize: 20,
          });
          sameCategory = byCategory.data?.items ?? [];
        } catch {
          sameCategory = [];
        }
      }

      if (sameCategory.length <= 1) {
        try {
          const fallback = await productApi.getAll({
            pageIndex: 1,
            pageSize: 100,
          });
          sameCategory = (fallback.data?.items ?? []).filter(
            (item) => item.categoryId === currentProduct.categoryId,
          );
        } catch {
          sameCategory = [];
        }
      }

      return sameCategory
        .filter((item) => item.id !== currentProduct.id)
        .slice(0, 6);
    };

    const fetchReviews = async (productId: number) => {
      setReviewsLoading(true);
      setReviewsError(null);

      try {
        const reviewResult = await reviewApi.getByProduct(productId, {
          pageIndex: 1,
          pageSize: 30,
          sortBy: "createdat",
          isAscending: false,
        });
        if (isCancelled) return;
        setReviews(reviewResult.items ?? []);
      } catch (error) {
        if (isCancelled) return;
        setReviews([]);
        if (error instanceof Error && error.message) {
          setReviewsError(error.message);
        } else {
          setReviewsError("Failed to load reviews.");
        }
      } finally {
        if (isCancelled) return;
        setReviewsLoading(false);
      }
    };

    const fetchProductPage = async () => {
      setLoading(true);

      try {
        const productId = Number(id);
        if (Number.isNaN(productId)) {
          setProduct(null);
          setRelatedProducts([]);
          setIsInWishlist(false);
          setIsAuthenticated(false);
          setReviews([]);
          setReviewsError(null);
          setReviewMessage(null);
          return;
        }

        const currentProduct = await productApi.getById(productId);
        if (isCancelled) return;

        setProduct(currentProduct);
        setQuantity(1);
        setWishlistMessage(null);
        setReviewMessage(null);
        setReviewContent("");
        setReviewRating(5);

        const reviewsPromise = fetchReviews(currentProduct.id);

        const related = await fetchRelatedProducts(currentProduct);
        if (isCancelled) return;
        setRelatedProducts(related);

        const token = await getAccessToken();
        if (isCancelled) return;
        setIsAuthenticated(Boolean(token));

        if (!token) {
          setIsInWishlist(false);
          await reviewsPromise;
          return;
        }

        try {
          const wishlistItems = await wishlistApi.get();
          if (isCancelled) return;
          setIsInWishlist(
            wishlistItems.some((item) => item.productId === currentProduct.id),
          );
        } catch {
          if (isCancelled) return;
          setIsInWishlist(false);
        }

        await reviewsPromise;
      } catch {
        if (isCancelled) return;
        setProduct(null);
        setRelatedProducts([]);
        setIsInWishlist(false);
        setIsAuthenticated(false);
        setReviews([]);
        setReviewsError(null);
      } finally {
        if (isCancelled) return;
        setLoading(false);
      }
    };

    void fetchProductPage();

    return () => {
      isCancelled = true;
    };
  }, [id]);

  const handleWishlistClick = async () => {
    if (!product || wishlistLoading) {
      return;
    }

    setWishlistMessage(null);

    const token = await getAccessToken();
    if (!token) {
      setWishlistMessage("Log in to use wishlist.");
      return;
    }

    setWishlistLoading(true);
    try {
      if (isInWishlist) {
        await wishlistApi.remove(product.id);
        setIsInWishlist(false);
        setProduct((prev) =>
          prev
            ? { ...prev, wishlistCount: Math.max(0, prev.wishlistCount - 1) }
            : prev,
        );
      } else {
        await wishlistApi.add({
          productId: product.id,
          productName: product.name,
        });
        setIsInWishlist(true);
        setProduct((prev) =>
          prev ? { ...prev, wishlistCount: prev.wishlistCount + 1 } : prev,
        );
      }
    } catch (error) {
      if (error instanceof Error && error.message) {
        setWishlistMessage(error.message);
      } else {
        setWishlistMessage("Could not update wishlist.");
      }
    } finally {
      setWishlistLoading(false);
    }
  };

  const handleReviewSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!product || reviewSubmitting) {
      return;
    }

    setReviewMessage(null);

    const token = await getAccessToken();
    const hasToken = Boolean(token);
    setIsAuthenticated(hasToken);

    if (!hasToken) {
      setReviewMessage("Log in to leave a review.");
      return;
    }

    const trimmedContent = reviewContent.trim();
    if (!trimmedContent) {
      setReviewMessage("Comment is required.");
      return;
    }

    setReviewSubmitting(true);
    try {
      await reviewApi.add(product.id, {
        productId: product.id,
        rating: reviewRating,
        content: trimmedContent,
      });

      setReviewContent("");
      setReviewRating(5);
      setReviewMessage("Review submitted and sent for moderation.");
    } catch (error) {
      if (error instanceof Error && error.message) {
        setReviewMessage(error.message);
      } else {
        setReviewMessage("Could not submit review.");
      }
    } finally {
      setReviewSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Skeleton className="mb-4 h-4 w-48" />
        <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_330px]">
          <div className="space-y-6">
            <div className="rounded-2xl border p-6">
              <div className="grid gap-6 lg:grid-cols-[360px_minmax(0,1fr)]">
                <Skeleton className="aspect-square w-full rounded-xl" />
                <div className="space-y-4">
                  <Skeleton className="h-6 w-3/4" />
                  <Skeleton className="h-4 w-40" />
                  <Skeleton className="h-4 w-36" />
                  <Skeleton className="h-4 w-44" />
                </div>
              </div>
            </div>
            <div className="rounded-2xl border p-6">
              <Skeleton className="mb-4 h-5 w-32" />
              <Skeleton className="h-20 w-full" />
            </div>
          </div>
          <div className="space-y-4">
            <div className="rounded-2xl border p-4">
              <Skeleton className="mb-3 h-6 w-28" />
              <Skeleton className="mb-4 h-4 w-24" />
              <Skeleton className="mb-3 h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="rounded-2xl border p-4">
              <Skeleton className="mb-3 h-4 w-44" />
              <Skeleton className="h-16 w-full rounded-md" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="container mx-auto px-4 py-12 text-center">
        <h1 className="mb-2 text-xl font-medium">Product not found</h1>
        <p className="mb-4 text-muted-foreground">
          The product page could not be loaded.
        </p>
      </div>
    );
  }

  const inStock = product.stockQuantity > 0;
  const hasDiscount =
    product.discountedPrice != null && product.discountedPrice < product.price;
  const currentPrice = hasDiscount
    ? product.discountedPrice ?? product.price
    : product.price;
  const quantityInCart = quantityByProductId(product.id);
  const maxQuantity = Math.max(1, product.stockQuantity);
  const categoryLabel = product.categoryName.trim() || "Category";
  const productCode =
    product.productCode?.trim() || `PRD-${String(product.id).padStart(6, "0")}`;
  const categoryHref = product.categoryName.trim()
    ? `/catalog?category=${encodeURIComponent(product.categoryName)}`
    : "/catalog";
  const reviewCount = product.reviewCount ?? 0;
  const averageRating = product.averageRating ?? 0;

  return (
    <div className="container mx-auto px-4 py-6">
      <nav className="mb-4 flex flex-wrap items-center gap-1.5 text-xs text-muted-foreground">
        <Link href="/catalog" className="hover:text-foreground">
          Catalog
        </Link>
        <ChevronRight className="h-3.5 w-3.5" />
        <Link href={categoryHref} className="hover:text-foreground">
          {categoryLabel}
        </Link>
        <ChevronRight className="h-3.5 w-3.5" />
        <span className="line-clamp-1 text-foreground">{product.name}</span>
      </nav>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_330px]">
        <div className="space-y-6">
          <section className="rounded-2xl border bg-card p-5 shadow-sm sm:p-6">
            <div className="grid gap-6 lg:grid-cols-[360px_minmax(0,1fr)]">
              <div className="flex aspect-square items-center justify-center overflow-hidden rounded-xl border bg-muted">
                <ResilientImage
                  src={product.imageUrl}
                  alt={product.name}
                  className="h-full w-full object-cover"
                  fallback={<Package className="h-14 w-14 text-muted-foreground" />}
                />
              </div>

              <div className="min-w-0">
                <div className="mb-3 flex flex-wrap items-center gap-2">
                  <Badge variant="secondary" className="text-xs">
                    {categoryLabel}
                  </Badge>
                  {hasDiscount && (
                    <Badge className="bg-red-500 text-xs text-white">Sale</Badge>
                  )}
                  <Badge variant="outline" className="text-xs">
                    {inStock ? "In stock" : "Out of stock"} | Code {productCode}
                  </Badge>
                </div>

                <h1 className="text-2xl font-semibold leading-tight sm:text-3xl">
                  {product.name}
                </h1>

                <div className="mt-6 grid gap-3 rounded-xl border bg-muted/20 p-4 text-sm">
                  <div className="flex items-center justify-between gap-3">
                    <span className="text-muted-foreground">Category</span>
                    <span className="font-medium">{categoryLabel}</span>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <span className="text-muted-foreground">Availability</span>
                    <span className="font-medium">
                      {inStock ? `${product.stockQuantity} units` : "Unavailable"}
                    </span>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <span className="inline-flex items-center gap-1 text-muted-foreground">
                      <Heart className="h-4 w-4" />
                      Wishlist
                    </span>
                    <span className="font-medium">{product.wishlistCount}</span>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <span className="text-muted-foreground">Rating</span>
                    {reviewCount > 0 ? (
                      <span className="inline-flex items-center gap-1.5 font-medium">
                        <span className="inline-flex items-center gap-0.5">
                          {renderStars(averageRating, "h-3.5 w-3.5")}
                        </span>
                        {averageRating.toFixed(1)} ({reviewCount})
                      </span>
                    ) : (
                      <span className="font-medium text-muted-foreground">
                        No reviews yet
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section className="rounded-2xl border bg-card p-5 shadow-sm sm:p-6">
            <Tabs defaultValue="description" className="gap-4">
              <TabsList>
                <TabsTrigger value="description">Description</TabsTrigger>
                <TabsTrigger value="reviews">Reviews ({reviewCount})</TabsTrigger>
              </TabsList>

              <TabsContent value="description">
                <h2 className="text-lg font-semibold">Description</h2>
                {product.description?.trim() ? (
                  <p className="mt-3 whitespace-pre-line text-sm leading-relaxed text-muted-foreground">
                    {product.description}
                  </p>
                ) : (
                  <p className="mt-3 text-sm text-muted-foreground">
                    Description is not available for this product yet.
                  </p>
                )}
              </TabsContent>

              <TabsContent value="reviews" className="space-y-4">
                <div className="flex flex-wrap items-center justify-between gap-2 rounded-xl border bg-muted/20 p-3">
                  <span className="text-sm text-muted-foreground">
                    Customer feedback
                  </span>
                  {reviewCount > 0 ? (
                    <span className="inline-flex items-center gap-1.5 text-sm font-medium">
                      <span className="inline-flex items-center gap-0.5">
                        {renderStars(averageRating)}
                      </span>
                      {averageRating.toFixed(1)} / 5 ({reviewCount})
                    </span>
                  ) : (
                    <span className="text-sm text-muted-foreground">
                      No reviews yet
                    </span>
                  )}
                </div>

                {reviewsLoading ? (
                  <div className="space-y-3">
                    {Array.from({ length: 3 }).map((_, index) => (
                      <Skeleton key={`review-skeleton-${index}`} className="h-24 w-full" />
                    ))}
                  </div>
                ) : reviewsError ? (
                  <p className="text-sm text-destructive">{reviewsError}</p>
                ) : reviews.length > 0 ? (
                  <div className="space-y-3">
                    {reviews.map((review) => (
                      <article key={review.id} className="rounded-xl border p-4">
                        <div className="flex flex-wrap items-center justify-between gap-2">
                          <span className="text-sm font-semibold">
                            {review.fullName?.trim() || "Anonymous customer"}
                          </span>
                          <span className="text-xs text-muted-foreground">
                            {new Date(review.createdAt).toLocaleDateString("en-US", {
                              year: "numeric",
                              month: "short",
                              day: "numeric",
                            })}
                          </span>
                        </div>
                        <div className="mt-2 inline-flex items-center gap-0.5">
                          {renderStars(review.rating)}
                        </div>
                        {review.content?.trim() ? (
                          <p className="mt-2 whitespace-pre-line text-sm text-muted-foreground">
                            {review.content}
                          </p>
                        ) : (
                          <p className="mt-2 text-sm text-muted-foreground">
                            No comment left by customer.
                          </p>
                        )}
                      </article>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">
                    No approved reviews yet. Be the first to share your experience.
                  </p>
                )}

                <section className="rounded-xl border p-4">
                  <h3 className="text-sm font-semibold">Leave a review</h3>
                  <p className="mt-1 text-xs text-muted-foreground">
                    New reviews are published after moderation.
                  </p>

                  <form className="mt-3 space-y-3" onSubmit={handleReviewSubmit}>
                    <div>
                      <p className="text-sm font-medium">Rating</p>
                      <div className="mt-1 flex items-center gap-1.5">
                        {Array.from({ length: 5 }).map((_, index) => {
                          const ratingValue = index + 1;
                          const isSelected = ratingValue <= reviewRating;

                          return (
                            <button
                              key={`rating-${ratingValue}`}
                              type="button"
                              className="rounded-sm p-0.5"
                              onClick={() => setReviewRating(ratingValue)}
                              aria-label={`Set rating to ${ratingValue}`}
                            >
                              <Star
                                className={`h-5 w-5 ${
                                  isSelected
                                    ? "fill-amber-400 text-amber-400"
                                    : "text-muted-foreground/40"
                                }`}
                              />
                            </button>
                          );
                        })}
                        <span className="text-xs text-muted-foreground">
                          {reviewRating} / 5
                        </span>
                      </div>
                    </div>

                    <div>
                      <p className="text-sm font-medium">Comment</p>
                      <Textarea
                        className="mt-1"
                        placeholder="Write your feedback..."
                        value={reviewContent}
                        onChange={(event) => setReviewContent(event.target.value)}
                        maxLength={1000}
                        rows={4}
                        required
                      />
                    </div>

                    <Button
                      type="submit"
                      disabled={reviewSubmitting || !isAuthenticated || !reviewContent.trim()}
                    >
                      {reviewSubmitting ? "Submitting..." : "Submit review"}
                    </Button>
                  </form>

                  {!isAuthenticated && (
                    <p className="mt-2 text-xs text-muted-foreground">
                      Log in to submit a review.
                    </p>
                  )}

                  {reviewMessage && (
                    <p className="mt-2 text-xs text-muted-foreground">{reviewMessage}</p>
                  )}
                </section>
              </TabsContent>
            </Tabs>
          </section>
        </div>

        <aside className="self-start space-y-4 xl:sticky xl:top-6">
          <section className="rounded-2xl border bg-card p-4 shadow-sm">
            <h2 className="text-base font-semibold">Purchase</h2>

            <div className="mt-3 flex items-end gap-2">
              <span className="text-3xl font-bold">{formatPrice(currentPrice)}</span>
              {hasDiscount && (
                <span className="pb-1 text-sm text-muted-foreground line-through">
                  {formatPrice(product.price)}
                </span>
              )}
            </div>

            <p className="mt-2 text-xs text-muted-foreground">
              {inStock ? `${product.stockQuantity} units available` : "Currently unavailable"}
            </p>

            <div className="mt-4 flex items-center gap-2">
              <div className="inline-flex items-center rounded-md border">
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-9 w-9 p-0"
                  onClick={() => setQuantity((value) => Math.max(1, value - 1))}
                  disabled={!inStock || quantity <= 1}
                  aria-label="Decrease quantity"
                >
                  <Minus className="h-4 w-4" />
                </Button>
                <span className="w-9 text-center text-sm font-medium">{quantity}</span>
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-9 w-9 p-0"
                  onClick={() =>
                    setQuantity((value) => Math.min(maxQuantity, value + 1))
                  }
                  disabled={!inStock || quantity >= maxQuantity}
                  aria-label="Increase quantity"
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>

              <Button
                className="h-9 flex-1"
                onClick={() => addItem(product, quantity)}
                disabled={!inStock}
              >
                <ShoppingCart className="mr-2 h-4 w-4" />
                Add to cart
              </Button>
            </div>

            <Button
              variant={isInWishlist ? "secondary" : "outline"}
              className="mt-2 h-9 w-full"
              onClick={() => void handleWishlistClick()}
              disabled={wishlistLoading}
            >
              <Heart
                className={`mr-2 h-4 w-4 ${isInWishlist ? "fill-current" : ""}`}
              />
              {wishlistLoading
                ? "Updating..."
                : isInWishlist
                  ? "Remove from wishlist"
                  : "Add to wishlist"}
            </Button>

            {wishlistMessage && (
              <p className="mt-2 text-xs text-muted-foreground">{wishlistMessage}</p>
            )}

            {quantityInCart > 0 && (
              <p className="mt-2 text-xs text-muted-foreground">
                In your cart: {quantityInCart}
              </p>
            )}
          </section>

          <section className="rounded-2xl border bg-card p-4 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
              Products from this category
            </h2>

            {relatedProducts.length > 0 ? (
              <div className="mt-3 space-y-2">
                {relatedProducts.map((item) => {
                  const relatedHasDiscount =
                    item.discountedPrice != null && item.discountedPrice < item.price;
                  const relatedPrice = relatedHasDiscount
                    ? item.discountedPrice ?? item.price
                    : item.price;

                  return (
                    <Link
                      key={item.id}
                      href={`/products/${item.id}`}
                      className="group flex items-center gap-3 rounded-lg border p-2 transition-colors hover:border-primary/30 hover:bg-primary/5"
                    >
                      <div className="flex h-14 w-14 shrink-0 items-center justify-center overflow-hidden rounded-md bg-muted">
                        <ResilientImage
                          src={item.imageUrl}
                          alt={item.name}
                          className="h-full w-full object-cover"
                          fallback={<Package className="h-5 w-5 text-muted-foreground" />}
                        />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className="line-clamp-2 text-sm font-medium leading-tight group-hover:text-primary">
                          {item.name}
                        </p>
                        <div className="mt-1 flex items-baseline gap-2">
                          <span className="text-sm font-semibold">
                            {formatPrice(relatedPrice)}
                          </span>
                          {item.stockQuantity <= 0 && (
                            <span className="text-xs text-muted-foreground">
                              Out of stock
                            </span>
                          )}
                        </div>
                      </div>
                    </Link>
                  );
                })}
              </div>
            ) : (
              <p className="mt-3 text-sm text-muted-foreground">
                No other products in this category yet.
              </p>
            )}
          </section>
        </aside>
      </div>
    </div>
  );
}
