"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useCart } from "@/features/cart/use-cart";
import { useAuthStore } from "@/entities/user";
import { bonusApi } from "@/shared/api";
import { type BonusAccountDto, type BonusSettingsDto } from "@/shared/types";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Minus, Plus, Trash2, ShoppingCart, ArrowRight } from "lucide-react";

function round2(value: number): number {
  return Math.round(value * 100) / 100;
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

function formatMoney(value: number): string {
  return round2(value).toFixed(2);
}

export default function CartPage() {
  const { cart, isLoading, itemsCount, totalPrice, removeItem, updateQuantity, clearCart } = useCart();
  const { isAuthenticated } = useAuthStore();
  const cartId = cart?.id ?? 0;
  const hasCartItems = Boolean(cart && cart.items.length > 0);
  const [bonusAccount, setBonusAccount] = useState<BonusAccountDto | null>(null);
  const [bonusSettings, setBonusSettings] = useState<BonusSettingsDto | null>(null);
  const [bonusLoading, setBonusLoading] = useState(false);
  const [bonusError, setBonusError] = useState<string | null>(null);
  const [useBonusPoints, setUseBonusPoints] = useState(false);
  const [redeemBonusInput, setRedeemBonusInput] = useState("");

  useEffect(() => {
    let ignore = false;

    if (!isAuthenticated || cartId === 0 || !hasCartItems) {
      setBonusAccount(null);
      setBonusSettings(null);
      setBonusError(null);
      setBonusLoading(false);
      setUseBonusPoints(false);
      setRedeemBonusInput("");
      return () => {
        ignore = true;
      };
    }

    const loadBonusData = async () => {
      setBonusLoading(true);
      setBonusError(null);

      try {
        const [account, settings] = await Promise.all([
          bonusApi.getAccount(),
          bonusApi.getSettings(),
        ]);

        if (ignore) {
          return;
        }

        setBonusAccount(account);
        setBonusSettings(settings);
      } catch (error) {
        if (ignore) {
          return;
        }

        setBonusAccount(null);
        setBonusSettings(null);
        setBonusError(error instanceof Error ? error.message : "Failed to load bonus data.");
      } finally {
        if (!ignore) {
          setBonusLoading(false);
        }
      }
    };

    void loadBonusData();

    return () => {
      ignore = true;
    };
  }, [isAuthenticated, cartId, hasCartItems]);

  const maxBonusUsable = useMemo(() => {
    if (!bonusAccount || !bonusSettings || !bonusSettings.isRedemptionEnabled) {
      return 0;
    }

    const byPercent = round2((totalPrice * bonusSettings.maxRedeemPercent) / 100);
    return round2(Math.max(0, Math.min(bonusAccount.balance, totalPrice, byPercent)));
  }, [bonusAccount, bonusSettings, totalPrice]);

  const parsedBonusInput = useMemo(() => {
    const normalized = redeemBonusInput.trim().replace(",", ".");
    const parsed = Number.parseFloat(normalized);
    return Number.isFinite(parsed) ? parsed : 0;
  }, [redeemBonusInput]);

  const redeemBonusPoints = useMemo(() => {
    if (!useBonusPoints) {
      return 0;
    }

    return round2(clamp(parsedBonusInput, 0, maxBonusUsable));
  }, [useBonusPoints, parsedBonusInput, maxBonusUsable]);

  useEffect(() => {
    if (!useBonusPoints) {
      return;
    }

    if (maxBonusUsable <= 0) {
      setUseBonusPoints(false);
      setRedeemBonusInput("");
      return;
    }

    if (parsedBonusInput > maxBonusUsable) {
      setRedeemBonusInput(String(maxBonusUsable));
    }
  }, [useBonusPoints, maxBonusUsable, parsedBonusInput]);

  const finalTotal = round2(Math.max(0, totalPrice - redeemBonusPoints));

  const checkoutHref = useMemo(() => {
    if (redeemBonusPoints <= 0) {
      return "/checkout";
    }

    const params = new URLSearchParams();
    params.set("redeemBonusPoints", String(redeemBonusPoints));
    return `/checkout?${params.toString()}`;
  }, [redeemBonusPoints]);

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

            {isAuthenticated ? (
              <div className="space-y-3 rounded-lg border border-border/70 p-3">
                <div className="text-sm font-medium">Bonus points</div>

                {bonusLoading && (
                  <p className="text-sm text-muted-foreground">
                    Loading bonus data...
                  </p>
                )}

                {!bonusLoading && bonusError && (
                  <p className="text-sm text-destructive">{bonusError}</p>
                )}

                {!bonusLoading && !bonusError && bonusAccount && bonusSettings && (
                  <>
                    <div className="text-sm text-muted-foreground">
                      Available: {formatMoney(bonusAccount.balance)} points
                    </div>
                    <div className="text-sm text-muted-foreground">
                      Max usable now: {formatMoney(maxBonusUsable)} points
                    </div>

                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        className="h-4 w-4 accent-primary"
                        checked={useBonusPoints}
                        disabled={!bonusSettings.isRedemptionEnabled || maxBonusUsable <= 0}
                        onChange={(event) => {
                          const checked = event.target.checked;
                          setUseBonusPoints(checked);
                          if (checked && !redeemBonusInput) {
                            setRedeemBonusInput(String(maxBonusUsable));
                          }
                        }}
                      />
                      Use bonus points for this order
                    </label>

                    {useBonusPoints && (
                      <div className="space-y-1">
                        <Label htmlFor="redeemBonusPointsInput">Points to redeem</Label>
                        <Input
                          id="redeemBonusPointsInput"
                          type="number"
                          step="0.01"
                          min="0"
                          max={maxBonusUsable}
                          value={redeemBonusInput}
                          onChange={(event) => setRedeemBonusInput(event.target.value)}
                        />
                      </div>
                    )}
                  </>
                )}
              </div>
            ) : (
              <div className="rounded-lg border border-border/70 p-3 text-sm text-muted-foreground">
                Sign in to use bonus points in checkout.
              </div>
            )}

            {redeemBonusPoints > 0 && (
              <div className="flex justify-between text-emerald-700">
                <span>Bonus discount</span>
                <span>-${formatMoney(redeemBonusPoints)}</span>
              </div>
            )}

            <Separator />
            <div className="flex justify-between text-lg font-bold">
              <span>Total</span>
              <span>${formatMoney(finalTotal)}</span>
            </div>
          </CardContent>
          <CardFooter>
            <Link href={checkoutHref} className="w-full">
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
