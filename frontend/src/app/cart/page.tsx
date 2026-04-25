"use client";

import Link from "next/link";
import { useCart } from "@/features/cart/use-cart";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Minus, Plus, Trash2, ShoppingCart, ArrowRight } from "lucide-react";

function round2(value: number): number {
  return Math.round(value * 100) / 100;
}

function formatMoney(value: number): string {
  return round2(value).toFixed(2);
}

export default function CartPage() {
  const { cart, isLoading, itemsCount, totalPrice, removeItem, updateQuantity, clearCart } = useCart();

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <p className="text-center text-muted-foreground">Loading cart...</p>
      </div>
    );
  }

  if (!cart || itemsCount === 0) {
    return (
      <div className="container mx-auto px-4 py-16">
        <div className="flex flex-col items-center justify-center text-center">
          <ShoppingCart className="mb-4 h-24 w-24 text-muted-foreground" />
          <h1 className="mb-2 text-2xl font-bold">Your Cart is Empty</h1>
          <p className="mb-6 text-muted-foreground">
            Add products from the catalog
          </p>
          <Link href="/catalog">
            <Button>Browse Catalog</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-3xl font-bold">
          Cart{" "}
          <span className="text-muted-foreground">({itemsCount} items)</span>
        </h1>
        <Button variant="outline" onClick={clearCart}>
          <Trash2 className="mr-1 h-4 w-4" />
          Clear
        </Button>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          {cart.items.map((item) => (
            <Card key={item.productId}>
              <CardContent className="flex items-center gap-4 p-4">
                <div className="flex h-20 w-20 shrink-0 items-center justify-center rounded-md bg-muted">
                  <span className="text-2xl">💊</span>
                </div>

                <div className="flex flex-1 flex-col">
                  <Link
                    href={`/products/${item.productId}`}
                    className="font-medium hover:text-primary"
                  >
                    {item.productName}
                  </Link>
                  <span className="text-sm text-muted-foreground">
                    In stock: {item.availableStock}
                  </span>
                </div>

                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() =>
                      updateQuantity(item.productId, item.quantity - 1)
                    }
                  >
                    <Minus className="h-3 w-3" />
                  </Button>
                  <span className="w-8 text-center">{item.quantity}</span>
                  <Button
                    variant="outline"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() =>
                      updateQuantity(item.productId, item.quantity + 1)
                    }
                  >
                    <Plus className="h-3 w-3" />
                  </Button>
                </div>

                <div className="text-right">
                  <div className="font-semibold">${formatMoney(item.subtotal)}</div>
                  <div className="text-sm text-muted-foreground">
                    ${formatMoney(item.price)} / unit
                  </div>
                </div>

                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => removeItem(item.productId)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>

        <Card className="h-fit lg:sticky lg:top-24">
          <CardHeader>
            <CardTitle>Summary</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Subtotal</span>
              <span>${formatMoney(totalPrice)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Shipping</span>
              <span className="text-green-600">Free</span>
            </div>
            <div className="rounded-lg border border-border/70 p-3 text-sm text-muted-foreground">
              Apply promo codes and bonus points during checkout.
            </div>

            <Separator />
            <div className="flex justify-between text-lg font-bold">
              <span>Total</span>
              <span>${formatMoney(totalPrice)}</span>
            </div>
          </CardContent>
          <CardFooter>
            <Link href="/checkout" className="w-full">
              <Button size="lg" className="w-full">
                Checkout
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            </Link>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
