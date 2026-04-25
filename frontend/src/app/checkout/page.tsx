"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useCart } from "@/features/cart/use-cart";
import { useAuthStore } from "@/entities/user";
import { addressApi, bonusApi, orderApi } from "@/shared/api";
import {
  BonusAccountDto,
  BonusSettingsDto,
  CreateOrderDto,
  SavedAddressDto,
} from "@/shared/types";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Textarea } from "@/components/ui/textarea";
import {
  ArrowLeft,
  CheckCircle2,
  CreditCard,
  MapPin,
  ShieldCheck,
  Truck,
} from "lucide-react";

type AddressMode = "saved" | "new";

function round2(value: number): number {
  return Math.round(value * 100) / 100;
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

function formatMoney(value: number): string {
  return round2(value).toFixed(2);
}

function formatSavedAddress(address: SavedAddressDto): string {
  const firstLine = [address.street, address.apartmentNumber]
    .filter(Boolean)
    .join(", ");
  const secondLine = [address.city, address.state, address.zipCode]
    .filter(Boolean)
    .join(", ");
  return [firstLine, secondLine, address.country].filter(Boolean).join(" | ");
}

export default function CheckoutPage() {
  const router = useRouter();
  const { cart, totalPrice, clearCart } = useCart();
  const { isAuthenticated } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [addressMode, setAddressMode] = useState<AddressMode>("new");
  const [savedAddresses, setSavedAddresses] = useState<SavedAddressDto[]>([]);
  const [savedAddressesLoading, setSavedAddressesLoading] = useState(false);
  const [savedAddressesError, setSavedAddressesError] = useState<string | null>(null);
  const [selectedSavedAddressId, setSelectedSavedAddressId] = useState<number | null>(null);

  const [street, setStreet] = useState("");
  const [city, setCity] = useState("");
  const [state, setState] = useState("");
  const [zipCode, setZipCode] = useState("");
  const [country, setCountry] = useState("");
  const [apartmentNumber, setApartmentNumber] = useState("");
  const [additionalInfo, setAdditionalInfo] = useState("");
  const [saveAddress, setSaveAddress] = useState(false);
  const [savedLabel, setSavedLabel] = useState("");
  const [promoCode, setPromoCode] = useState("");
  const [bonusAccount, setBonusAccount] = useState<BonusAccountDto | null>(null);
  const [bonusSettings, setBonusSettings] = useState<BonusSettingsDto | null>(null);
  const [bonusLoading, setBonusLoading] = useState(false);
  const [bonusError, setBonusError] = useState<string | null>(null);
  const [useBonusPoints, setUseBonusPoints] = useState(false);
  const [redeemBonusInput, setRedeemBonusInput] = useState("");
  const cartId = cart?.id ?? 0;
  const hasCartItems = Boolean(cart && cart.items.length > 0);

  useEffect(() => {
    let ignore = false;

    if (!isAuthenticated) {
      setSavedAddresses([]);
      setSavedAddressesError(null);
      setSelectedSavedAddressId(null);
      setAddressMode("new");
      return () => {
        ignore = true;
      };
    }

    const loadSavedAddresses = async () => {
      setSavedAddressesLoading(true);
      setSavedAddressesError(null);

      try {
        const addresses = await addressApi.getAll();

        if (ignore) {
          return;
        }

        setSavedAddresses(addresses);

        if (addresses.length === 0) {
          setSelectedSavedAddressId(null);
          setAddressMode((current) => (current === "saved" ? "new" : current));
          return;
        }

        const defaultAddress = addresses.find((item) => item.isDefault) ?? addresses[0];
        setSelectedSavedAddressId((current) =>
          current !== null && addresses.some((item) => item.id === current)
            ? current
            : defaultAddress.id
        );
      } catch {
        if (!ignore) {
          setSavedAddressesError("Failed to load saved addresses.");
        }
      } finally {
        if (!ignore) {
          setSavedAddressesLoading(false);
        }
      }
    };

    void loadSavedAddresses();

    return () => {
      ignore = true;
    };
  }, [isAuthenticated]);

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

  const totalWithBonuses = round2(Math.max(0, totalPrice - redeemBonusPoints));

  if (!cart || cart.items.length === 0) {
    if (!success) {
      return (
        <div className="container mx-auto px-4 py-16">
          <Card className="mx-auto max-w-xl rounded-[2rem] border-dashed border-primary/25 bg-background/75">
            <CardContent className="space-y-4 p-8 text-center">
              <h1 className="text-2xl font-semibold">Your cart is empty</h1>
              <p className="text-muted-foreground">
                Add products from the catalog before continuing to checkout.
              </p>
              <Link href="/catalog">
                <Button>Browse catalog</Button>
              </Link>
            </CardContent>
          </Card>
        </div>
      );
    }
  }

  if (success) {
    return (
      <div className="container mx-auto px-4 py-16">
        <Card className="mx-auto max-w-xl rounded-[2rem] border-primary/20 bg-card/85 text-center">
          <CardContent className="space-y-5 p-10">
            <CheckCircle2 className="mx-auto h-16 w-16 text-emerald-600" />
            <div className="space-y-2">
              <h1 className="text-3xl font-semibold">Order placed</h1>
              <p className="text-muted-foreground">
                The order was submitted successfully. You can review it in your
                orders page.
              </p>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
              <Link href="/orders">
                <Button variant="outline">Open my orders</Button>
              </Link>
              <Link href="/catalog">
                <Button>Continue browsing</Button>
              </Link>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);

    if (!isAuthenticated) {
      router.push("/login?redirect=/checkout");
      return;
    }

    if (addressMode === "saved") {
      if (!selectedSavedAddressId) {
        setError("Select one saved address to continue.");
        return;
      }
    } else {
      if (
        !street.trim() ||
        !city.trim() ||
        !state.trim() ||
        !zipCode.trim() ||
        !country.trim()
      ) {
        setError("Fill in all required address fields.");
        return;
      }

      if (saveAddress && !savedLabel.trim()) {
        setError("Provide a label to save the new address.");
        return;
      }
    }

    setLoading(true);

    try {
      const payload: CreateOrderDto = {
        saveAddress: false,
        promoCode: promoCode.trim() || undefined,
        redeemBonusPoints: redeemBonusPoints > 0 ? redeemBonusPoints : undefined,
      };

      if (addressMode === "saved") {
        payload.savedAddressId = selectedSavedAddressId!;
      } else {
        payload.newAddress = {
          street: street.trim(),
          city: city.trim(),
          state: state.trim(),
          zipCode: zipCode.trim(),
          country: country.trim(),
          apartmentNumber: apartmentNumber.trim() || undefined,
          additionalInfo: additionalInfo.trim() || undefined,
        };
        payload.saveAddress = saveAddress;
        payload.savedLabel = saveAddress ? savedLabel.trim() : undefined;
      }

      await orderApi.create(payload);

      await clearCart();
      setSuccess(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error placing order.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <Link
        href="/cart"
        className="mb-6 inline-flex items-center text-sm text-muted-foreground transition-colors hover:text-foreground"
      >
        <ArrowLeft className="mr-2 h-4 w-4" />
        Back to cart
      </Link>

      <div className="mb-8 flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.28em] text-primary">
            Checkout
          </p>
          <h1 className="mt-2 text-4xl font-semibold tracking-tight">
            Confirm delivery details and place the order
          </h1>
        </div>
        <div className="grid gap-3 text-sm text-muted-foreground sm:grid-cols-3">
          <div className="rounded-full border border-border/70 bg-card/70 px-4 py-2">
            Cart review
          </div>
          <div className="rounded-full border border-primary/30 bg-primary/10 px-4 py-2 text-primary">
            Address details
          </div>
          <div className="rounded-full border border-border/70 bg-card/70 px-4 py-2">
            Confirmation
          </div>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.1fr,0.9fr]">
        <form onSubmit={handleSubmit} className="space-y-6">
          <Card className="rounded-[2rem] border-border/70 bg-card/85">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5 text-primary" />
                Delivery address
              </CardTitle>
              <CardDescription>
                Choose a saved address or provide a new one for this order.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-2 sm:grid-cols-2">
                <Button
                  type="button"
                  variant={addressMode === "saved" ? "default" : "outline"}
                  className="w-full"
                  onClick={() => {
                    setAddressMode("saved");
                    setError(null);
                  }}
                  disabled={savedAddressesLoading || savedAddresses.length === 0}
                >
                  Use saved address
                </Button>
                <Button
                  type="button"
                  variant={addressMode === "new" ? "default" : "outline"}
                  className="w-full"
                  onClick={() => {
                    setAddressMode("new");
                    setError(null);
                  }}
                >
                  Use new address
                </Button>
              </div>

              {savedAddressesLoading && (
                <p className="text-sm text-muted-foreground">
                  Loading saved addresses...
                </p>
              )}

              {savedAddressesError && (
                <Alert variant="destructive">
                  <AlertTitle>Address list unavailable</AlertTitle>
                  <AlertDescription>{savedAddressesError}</AlertDescription>
                </Alert>
              )}

              {addressMode === "saved" && (
                <>
                  {savedAddresses.length === 0 ? (
                    <Alert>
                      <AlertTitle>No saved addresses</AlertTitle>
                      <AlertDescription>
                        Add an address in your profile or switch to new address mode.
                      </AlertDescription>
                    </Alert>
                  ) : (
                    <div className="space-y-3">
                      {savedAddresses.map((savedAddress) => {
                        const isSelected = selectedSavedAddressId === savedAddress.id;

                        return (
                          <label
                            key={savedAddress.id}
                            className={`flex cursor-pointer items-start gap-3 rounded-xl border p-3 transition-colors ${
                              isSelected
                                ? "border-primary bg-primary/5"
                                : "border-border/70 bg-background/70"
                            }`}
                          >
                            <input
                              type="radio"
                              name="savedAddressId"
                              className="mt-1 h-4 w-4 accent-primary"
                              checked={isSelected}
                              onChange={() => setSelectedSavedAddressId(savedAddress.id)}
                            />
                            <div className="space-y-1">
                              <div className="flex items-center gap-2">
                                <span className="font-medium">{savedAddress.label}</span>
                                {savedAddress.isDefault && (
                                  <span className="rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                                    Default
                                  </span>
                                )}
                              </div>
                              <p className="text-sm text-muted-foreground">
                                {formatSavedAddress(savedAddress)}
                              </p>
                            </div>
                          </label>
                        );
                      })}
                    </div>
                  )}
                </>
              )}

              {addressMode === "new" && (
                <>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="space-y-2">
                      <Label htmlFor="country">Country *</Label>
                      <Input
                        id="country"
                        value={country}
                        onChange={(e) => setCountry(e.target.value)}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="city">City *</Label>
                      <Input
                        id="city"
                        value={city}
                        onChange={(e) => setCity(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="street">Street *</Label>
                    <Input
                      id="street"
                      value={street}
                      onChange={(e) => setStreet(e.target.value)}
                      required
                    />
                  </div>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="space-y-2">
                      <Label htmlFor="state">State / Province *</Label>
                      <Input
                        id="state"
                        value={state}
                        onChange={(e) => setState(e.target.value)}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="zipCode">ZIP / Postal code *</Label>
                      <Input
                        id="zipCode"
                        value={zipCode}
                        onChange={(e) => setZipCode(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="apartmentNumber">Apartment / Suite</Label>
                    <Input
                      id="apartmentNumber"
                      value={apartmentNumber}
                      onChange={(e) => setApartmentNumber(e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="additionalInfo">Delivery notes</Label>
                    <Textarea
                      id="additionalInfo"
                      value={additionalInfo}
                      onChange={(e) => setAdditionalInfo(e.target.value)}
                      placeholder="Entrance, floor, or anything useful for delivery."
                    />
                  </div>
                  <div className="flex items-center gap-2">
                    <input
                      id="saveAddress"
                      type="checkbox"
                      className="h-4 w-4 accent-primary"
                      checked={saveAddress}
                      onChange={(e) => setSaveAddress(e.target.checked)}
                    />
                    <Label htmlFor="saveAddress">Save this address to profile</Label>
                  </div>
                  {saveAddress && (
                    <div className="space-y-2">
                      <Label htmlFor="savedLabel">Address label *</Label>
                      <Input
                        id="savedLabel"
                        placeholder="Home, Office, etc."
                        value={savedLabel}
                        onChange={(e) => setSavedLabel(e.target.value)}
                        required={saveAddress}
                      />
                    </div>
                  )}
                </>
              )}
            </CardContent>
          </Card>

          <Card className="rounded-[2rem] border-border/70 bg-card/85">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <CreditCard className="h-5 w-5 text-primary" />
                Order extras
              </CardTitle>
              <CardDescription>
                Optional information applied to the current order.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <Label htmlFor="promoCode">Promo code</Label>
                <Input
                  id="promoCode"
                  placeholder="Add a promo code if you have one"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value)}
                />
              </div>
              <div className="mt-4 space-y-3 rounded-lg border border-border/70 p-4">
                <div className="text-sm font-medium">Bonus points</div>

                {!isAuthenticated && (
                  <p className="text-sm text-muted-foreground">
                    Sign in to use bonus points during checkout.
                  </p>
                )}

                {isAuthenticated && bonusLoading && (
                  <p className="text-sm text-muted-foreground">
                    Loading bonus data...
                  </p>
                )}

                {isAuthenticated && !bonusLoading && bonusError && (
                  <p className="text-sm text-destructive">{bonusError}</p>
                )}

                {isAuthenticated && !bonusLoading && !bonusError && bonusAccount && bonusSettings && (
                  <>
                    <div className="text-sm text-muted-foreground">
                      Available: {formatMoney(bonusAccount.balance)} points
                    </div>
                    <div className="text-sm text-muted-foreground">
                      Max usable now: {formatMoney(maxBonusUsable)} points
                    </div>

                    {!bonusSettings.isRedemptionEnabled && (
                      <p className="text-sm text-muted-foreground">
                        Bonus redemption is currently disabled.
                      </p>
                    )}

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
            </CardContent>
          </Card>

          {error && (
            <Alert variant="destructive">
              <AlertTitle>Checkout error</AlertTitle>
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          )}

          <Button type="submit" size="lg" disabled={loading} className="w-full rounded-full">
            {loading ? "Placing order..." : `Place order for $${formatMoney(totalWithBonuses)}`}
          </Button>
        </form>

        <div className="space-y-6 lg:sticky lg:top-24 lg:self-start">
          <Card className="rounded-[2rem] border-border/70 bg-card/88">
            <CardHeader>
              <CardTitle>Your order</CardTitle>
              <CardDescription>
                Review the current cart before submitting.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {cart?.items.map((item) => (
                <div key={item.productId} className="flex items-start justify-between gap-4">
                  <div>
                    <div className="font-medium">{item.productName}</div>
                    <div className="text-sm text-muted-foreground">
                      {item.quantity} × ${item.price}
                    </div>
                  </div>
                  <div className="font-semibold">${item.subtotal}</div>
                </div>
              ))}
              <Separator />
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Shipping</span>
                <span className="text-emerald-700">Included</span>
              </div>
              {redeemBonusPoints > 0 && (
                <div className="flex items-center justify-between text-sm text-emerald-700">
                  <span>Bonus discount</span>
                  <span>-${formatMoney(redeemBonusPoints)}</span>
                </div>
              )}
              <div className="flex items-center justify-between text-lg font-semibold">
                <span>Total</span>
                <span>${formatMoney(totalWithBonuses)}</span>
              </div>
            </CardContent>
          </Card>

          <Card className="rounded-[2rem] border-primary/15 bg-background/80">
            <CardContent className="grid gap-4 p-6 sm:grid-cols-3 lg:grid-cols-1">
              <div className="flex gap-3">
                <ShieldCheck className="mt-1 h-5 w-5 text-primary" />
                <div>
                  <div className="font-medium">Protected flow</div>
                  <p className="text-sm text-muted-foreground">
                    Checkout stays inside the current storefront.
                  </p>
                </div>
              </div>
              <div className="flex gap-3">
                <Truck className="mt-1 h-5 w-5 text-primary" />
                <div>
                  <div className="font-medium">Delivery details</div>
                  <p className="text-sm text-muted-foreground">
                    Address fields match the existing backend contract.
                  </p>
                </div>
              </div>
              <div className="flex gap-3">
                <CheckCircle2 className="mt-1 h-5 w-5 text-primary" />
                <div>
                  <div className="font-medium">Orders page ready</div>
                  <p className="text-sm text-muted-foreground">
                    After placement, the next step is already wired to `/orders`.
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
