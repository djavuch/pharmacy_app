"use client";

import Link from "next/link";
import { ProductDto } from "@/shared/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ResilientImage } from "@/shared/ui/resilient-image";
import { ShoppingCart, Star } from "lucide-react";

interface ProductCardProps {
  product: ProductDto;
  onAdd: (product: ProductDto, quantity?: number) => void;
  quantityInCart?: number;
}

export function ProductCard({
  product,
  onAdd,
  quantityInCart = 0,
}: ProductCardProps) {
  const inStock = product.stockQuantity > 0;
  const hasDiscount =
    product.discountedPrice != null && product.discountedPrice < product.price;
  const currentPrice = hasDiscount
    ? product.discountedPrice ?? product.price
    : product.price;
  const reviewCount = product.reviewCount ?? 0;
  const averageRating = product.averageRating ?? 0;
  const hasRating = reviewCount > 0;
  const roundedRating = Math.round(averageRating);

  return (
    <Card className="group flex h-full flex-col overflow-hidden transition-all hover:shadow-md">
      {/* Image */}
      <Link href={`/products/${product.id}`} className="relative block">
        <div className="aspect-square w-full bg-muted">
          <ResilientImage
            src={product.imageUrl}
            alt={product.name}
            className="h-full w-full object-cover transition-transform group-hover:scale-105"
            fallback={
              <div className="flex h-full items-center justify-center text-muted-foreground">
                <ShoppingCart className="h-12 w-12" />
              </div>
            }
          />
        </div>
        {hasDiscount && (
          <Badge className="absolute left-2 top-2 bg-red-500 text-white">
            Sale
          </Badge>
        )}
      </Link>

      {/* Content */}
      <CardContent className="flex flex-1 flex-col gap-1 p-4">
        <Link href={`/products/${product.id}`}>
          <h3 className="line-clamp-2 font-medium leading-snug hover:text-primary">
            {product.name}
          </h3>
        </Link>
        <p className="line-clamp-1 text-xs text-muted-foreground">
          {product.categoryName}
        </p>
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <div className="flex items-center">
            {Array.from({ length: 5 }).map((_, index) => (
              <Star
                key={`${product.id}-star-${index}`}
                className={`h-3.5 w-3.5 ${
                  index < roundedRating
                    ? "fill-amber-400 text-amber-400"
                    : "text-muted-foreground/40"
                }`}
              />
            ))}
          </div>
          <span className="font-medium text-foreground">
            {hasRating ? averageRating.toFixed(1) : "No rating"}
          </span>
          {hasRating && <span>({reviewCount})</span>}
        </div>
        <div className="mt-auto flex items-baseline gap-2">
          <span className="text-lg font-semibold">${currentPrice}</span>
          {hasDiscount && (
            <span className="text-sm text-muted-foreground line-through">
              ${product.price}
            </span>
          )}
        </div>
      </CardContent>

      {/* Footer */}
      <CardFooter className="p-4 pt-0">
        <Button
          onClick={() => onAdd(product)}
          disabled={!inStock}
          size="sm"
          className="w-full"
        >
          <ShoppingCart className="mr-1 h-4 w-4" />
          {inStock
            ? quantityInCart > 0
              ? `In cart: ${quantityInCart}`
              : "Add to cart"
            : "Out of stock"}
        </Button>
      </CardFooter>
    </Card>
  );
}
