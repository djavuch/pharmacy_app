"use client";

import { FormEvent, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/entities/user";
import { accountApi, addressApi, getAccessToken, userApi, wishlistApi } from "@/shared/api";
import { SaveAddressDto, SavedAddressDto, WishlistDto } from "@/shared/types";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import { BonusTab } from "./_components/bonus-tab";
import { WishlistTab } from "./_components/wishlist-tab";
import {
  CalendarDays,
  KeyRound,
  MapPin,
  Pencil,
  Phone,
  Plus,
  Save,
  ShieldCheck,
  Trash2,
  UserRound,
  Heart,
} from "lucide-react";

const emptyAddressForm: SaveAddressDto = {
  label: "",
  street: "",
  apartmentNumber: "",
  city: "",
  state: "",
  zipCode: "",
  country: "",
  additionalInfo: "",
  isDefault: false,
};

function getErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback;
}

function formatSavedAddress(address: SavedAddressDto): string {
  const firstLine = [address.street, address.apartmentNumber]
    .filter(Boolean)
    .join(", ");
  const secondLine = [address.city, address.state, address.zipCode]
    .filter(Boolean)
    .join(", ");
  return [firstLine, secondLine, address.country].filter(Boolean).join(" • ");
}

export default function ProfilePage() {
  const router = useRouter();
  const { isAuthenticated, profile, loadProfile } = useAuthStore();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [loading, setLoading] = useState(true);
  const [profileSaving, setProfileSaving] = useState(false);
  const [profileMessage, setProfileMessage] = useState<string | null>(null);
  const [profileError, setProfileError] = useState<string | null>(null);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordSaving, setPasswordSaving] = useState(false);
  const [passwordMessage, setPasswordMessage] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);

  const [addresses, setAddresses] = useState<SavedAddressDto[]>([]);
  const [addressesLoading, setAddressesLoading] = useState(false);
  const [addressSubmitting, setAddressSubmitting] = useState(false);
  const [addressActionId, setAddressActionId] = useState<number | null>(null);
  const [addressToDelete, setAddressToDelete] = useState<SavedAddressDto | null>(null);
  const [editingAddressId, setEditingAddressId] = useState<number | null>(null);
  const [addressForm, setAddressForm] = useState<SaveAddressDto>(emptyAddressForm);
  const [addressMessage, setAddressMessage] = useState<string | null>(null);
  const [addressError, setAddressError] = useState<string | null>(null);
  const [wishlistItems, setWishlistItems] = useState<WishlistDto[]>([]);
  const [wishlistLoading, setWishlistLoading] = useState(false);
  const [wishlistActionId, setWishlistActionId] = useState<number | null>(null);
  const [wishlistMessage, setWishlistMessage] = useState<string | null>(null);
  const [wishlistError, setWishlistError] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;

    const load = async () => {
      setLoading(true);
      setAddressesLoading(true);
      setWishlistLoading(true);

      try {
        const token = await getAccessToken();

        if (!token) {
          router.push("/login?redirect=/profile");
          return;
        }

        await loadProfile();
        const [addressesResult, wishlistResult] = await Promise.allSettled([
          addressApi.getAll(),
          wishlistApi.get(),
        ]);

        if (ignore) {
          return;
        }

        if (addressesResult.status === "fulfilled") {
          setAddresses(addressesResult.value);
          setAddressError(null);
        } else {
          setAddressError(getErrorMessage(addressesResult.reason, "Failed to load addresses."));
        }

        if (wishlistResult.status === "fulfilled") {
          setWishlistItems(wishlistResult.value);
          setWishlistError(null);
        } else {
          setWishlistError(getErrorMessage(wishlistResult.reason, "Failed to load wishlist."));
        }
      } catch (err) {
        if (!ignore) {
          const message = getErrorMessage(err, "Failed to load profile.");
          setAddressError(message);
          setWishlistError(message);
        }
      } finally {
        if (!ignore) {
          setLoading(false);
          setAddressesLoading(false);
          setWishlistLoading(false);
        }
      }
    };

    void load();

    return () => {
      ignore = true;
    };
  }, [loadProfile, router]);

  useEffect(() => {
    if (!loading && !isAuthenticated) {
      router.push("/login?redirect=/profile");
    }
  }, [isAuthenticated, loading, router]);

  useEffect(() => {
    if (!profile) {
      return;
    }

    setFirstName(profile.firstName || "");
    setLastName(profile.lastName || "");
    setPhoneNumber(profile.phoneNumber || "");
  }, [profile]);

  const loadAddresses = async () => {
    setAddressesLoading(true);

    try {
      const savedAddresses = await addressApi.getAll();
      setAddresses(savedAddresses);
      return savedAddresses;
    } finally {
      setAddressesLoading(false);
    }
  };

  const loadWishlist = async () => {
    setWishlistLoading(true);

    try {
      const items = await wishlistApi.get();
      setWishlistItems(items);
      setWishlistError(null);
      return items;
    } catch (err) {
      setWishlistError(getErrorMessage(err, "Failed to load wishlist."));
      return [];
    } finally {
      setWishlistLoading(false);
    }
  };

  const resetAddressForm = () => {
    setAddressForm(emptyAddressForm);
    setEditingAddressId(null);
  };

  const handleSaveProfile = async () => {
    setProfileSaving(true);
    setProfileError(null);
    setProfileMessage(null);

    try {
      await userApi.updateProfile({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phoneNumber: phoneNumber.trim(),
      });
      await loadProfile();
      setProfileMessage("Personal information updated.");
    } catch (err) {
      setProfileError(getErrorMessage(err, "Failed to update profile."));
    } finally {
      setProfileSaving(false);
    }
  };

  const handleChangePassword = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setPasswordError(null);
    setPasswordMessage(null);

    if (newPassword !== confirmPassword) {
      setPasswordError("New password and confirmation do not match.");
      return;
    }

    setPasswordSaving(true);

    try {
      await accountApi.changePassword(currentPassword, newPassword, confirmPassword);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      setPasswordMessage("Password changed successfully.");
    } catch (err) {
      setPasswordError(getErrorMessage(err, "Failed to change password."));
    } finally {
      setPasswordSaving(false);
    }
  };

  const startEditAddress = (savedAddress: SavedAddressDto) => {
    setEditingAddressId(savedAddress.id);
    setAddressMessage(null);
    setAddressError(null);
    setAddressForm({
      label: savedAddress.label,
      street: savedAddress.street,
      apartmentNumber: savedAddress.apartmentNumber ?? "",
      city: savedAddress.city,
      state: savedAddress.state,
      zipCode: savedAddress.zipCode,
      country: savedAddress.country,
      additionalInfo: savedAddress.additionalInfo ?? "",
      isDefault: savedAddress.isDefault,
    });
  };

  const handleAddressSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setAddressError(null);
    setAddressMessage(null);

    if (
      !addressForm.label.trim() ||
      !addressForm.street.trim() ||
      !addressForm.city.trim() ||
      !addressForm.state.trim() ||
      !addressForm.zipCode.trim() ||
      !addressForm.country.trim()
    ) {
      setAddressError("Fill in all required address fields.");
      return;
    }

    const payload: SaveAddressDto = {
      label: addressForm.label.trim(),
      street: addressForm.street.trim(),
      apartmentNumber: addressForm.apartmentNumber?.trim() || undefined,
      city: addressForm.city.trim(),
      state: addressForm.state.trim(),
      zipCode: addressForm.zipCode.trim(),
      country: addressForm.country.trim(),
      additionalInfo: addressForm.additionalInfo?.trim() || undefined,
      isDefault: addressForm.isDefault,
    };

    setAddressSubmitting(true);

    try {
      if (editingAddressId !== null) {
        await addressApi.update(editingAddressId, payload);
        setAddressMessage("Address updated.");
      } else {
        await addressApi.create(payload);
        setAddressMessage("Address added.");
      }

      resetAddressForm();
      await loadAddresses();
    } catch (err) {
      setAddressError(getErrorMessage(err, "Failed to save address."));
    } finally {
      setAddressSubmitting(false);
    }
  };

  const requestAddressDelete = (address: SavedAddressDto) => {
    setAddressError(null);
    setAddressMessage(null);
    setAddressToDelete(address);
  };

  const handleDeleteAddress = async () => {
    if (!addressToDelete) {
      return;
    }

    setAddressActionId(addressToDelete.id);
    setAddressError(null);
    setAddressMessage(null);

    try {
      await addressApi.remove(addressToDelete.id);
      if (editingAddressId === addressToDelete.id) {
        resetAddressForm();
      }
      await loadAddresses();
      setAddressMessage("Address deleted.");
    } catch (err) {
      setAddressError(getErrorMessage(err, "Failed to delete address."));
    } finally {
      setAddressActionId(null);
      setAddressToDelete(null);
    }
  };

  const handleSetDefaultAddress = async (id: number) => {
    setAddressActionId(id);
    setAddressError(null);
    setAddressMessage(null);

    try {
      await addressApi.setDefault(id);
      await loadAddresses();
      setAddressMessage("Default address updated.");
    } catch (err) {
      setAddressError(getErrorMessage(err, "Failed to set default address."));
    } finally {
      setAddressActionId(null);
    }
  };

  const handleRemoveWishlistItem = async (productId: number) => {
    setWishlistActionId(productId);
    setWishlistError(null);
    setWishlistMessage(null);

    try {
      await wishlistApi.remove(productId);
      setWishlistItems((current) =>
        current.filter((entry) => entry.productId !== productId),
      );
      setWishlistMessage("Item removed from wishlist.");
    } catch (err) {
      setWishlistError(getErrorMessage(err, "Failed to remove wishlist item."));
    } finally {
      setWishlistActionId(null);
    }
  };

  const defaultSavedAddress = addresses.find((address) => address.isDefault);
  const savedAddressPreview = defaultSavedAddress
    ? formatSavedAddress(defaultSavedAddress)
    : "Not specified";
  const isDeleteInProgress =
    addressToDelete !== null && addressActionId === addressToDelete.id;

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <p className="text-center text-muted-foreground">Loading profile...</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8 flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.28em] text-primary">
            Profile
          </p>
          <h1 className="mt-2 text-4xl font-semibold tracking-tight">
            Manage your account details
          </h1>
        </div>
        <div className="flex gap-3">
          <Link href="/orders">
            <Button variant="outline">Open orders</Button>
          </Link>
          <Link href="/catalog">
            <Button>Browse catalog</Button>
          </Link>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-[0.85fr,1.15fr]">
        <Card className="rounded-[2rem] border-border/70 bg-card/88">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <UserRound className="h-5 w-5 text-primary" />
              Account snapshot
            </CardTitle>
            <CardDescription>
              Basic information currently returned by the existing API.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <div className="text-sm text-muted-foreground">Email</div>
              <div className="font-medium">{profile?.email}</div>
            </div>
            <Separator />
            <div className="flex gap-3">
              <CalendarDays className="mt-0.5 h-5 w-5 text-primary" />
              <div>
                <div className="text-sm text-muted-foreground">Member since</div>
                <div className="font-medium">
                  {profile?.createdAt
                    ? new Date(profile.createdAt).toLocaleDateString("en-US", {
                        day: "numeric",
                        month: "long",
                        year: "numeric",
                      })
                    : "Not available"}
                </div>
              </div>
            </div>
            <div className="flex gap-3">
              <CalendarDays className="mt-0.5 h-5 w-5 text-primary" />
              <div>
                <div className="text-sm text-muted-foreground">Date of birth</div>
                <div className="font-medium">
                  {profile?.dateOfBirth
                    ? new Date(profile.dateOfBirth).toLocaleDateString("en-US", {
                        day: "numeric",
                        month: "long",
                        year: "numeric",
                      })
                    : "Not specified"}
                </div>
              </div>
            </div>
            <div className="flex gap-3">
              <MapPin className="mt-0.5 h-5 w-5 text-primary" />
              <div>
                <div className="text-sm text-muted-foreground">Saved address</div>
                <div className="font-medium">{savedAddressPreview}</div>
              </div>
            </div>
            <div className="flex gap-3">
              <Phone className="mt-0.5 h-5 w-5 text-primary" />
              <div>
                <div className="text-sm text-muted-foreground">Phone</div>
                <div className="font-medium">
                  {profile?.phoneNumber || "Not specified"}
                </div>
              </div>
            </div>
            <div className="flex gap-3">
              <Heart className="mt-0.5 h-5 w-5 text-primary" />
              <div>
                <div className="text-sm text-muted-foreground">Wishlist items</div>
                <div className="font-medium">
                  {wishlistLoading ? "Loading..." : wishlistItems.length}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-[2rem] border-border/70 bg-card/88">
          <CardHeader>
            <CardTitle>Account settings</CardTitle>
            <CardDescription>
              Update personal info, password, and delivery addresses.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <Tabs defaultValue="profile" className="w-full">
              <TabsList className="grid h-auto w-full grid-cols-2 gap-2 lg:grid-cols-5">
                <TabsTrigger value="profile">Personal info</TabsTrigger>
                <TabsTrigger value="security">Password</TabsTrigger>
                <TabsTrigger value="addresses">Addresses</TabsTrigger>
                <TabsTrigger value="wishlist">Wishlist</TabsTrigger>
                <TabsTrigger value="bonuses">Bonuses</TabsTrigger>
              </TabsList>

              <TabsContent value="profile" className="space-y-4 pt-4">
                <div className="grid gap-4 sm:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="firstName">First name</Label>
                    <Input
                      id="firstName"
                      value={firstName}
                      onChange={(e) => setFirstName(e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="lastName">Last name</Label>
                    <Input
                      id="lastName"
                      value={lastName}
                      onChange={(e) => setLastName(e.target.value)}
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phoneNumber">Phone number</Label>
                  <Input
                    id="phoneNumber"
                    value={phoneNumber}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                  />
                </div>

                {profileMessage && (
                  <Alert>
                    <AlertTitle>Saved</AlertTitle>
                    <AlertDescription>{profileMessage}</AlertDescription>
                  </Alert>
                )}

                {profileError && (
                  <Alert variant="destructive">
                    <AlertTitle>Update failed</AlertTitle>
                    <AlertDescription>{profileError}</AlertDescription>
                  </Alert>
                )}

                <Button
                  onClick={handleSaveProfile}
                  disabled={profileSaving}
                  className="rounded-full"
                >
                  <Save className="mr-2 h-4 w-4" />
                  {profileSaving ? "Saving..." : "Save changes"}
                </Button>
              </TabsContent>

              <TabsContent value="security" className="space-y-4 pt-4">
                <div className="rounded-2xl border border-border/70 bg-background/70 p-4">
                  <div className="mb-2 text-sm text-muted-foreground">
                    Current account email
                  </div>
                  <div className="font-medium">{profile?.email || "Not available"}</div>
                </div>

                <form onSubmit={handleChangePassword} className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="currentPassword">Current password</Label>
                    <Input
                      id="currentPassword"
                      type="password"
                      value={currentPassword}
                      onChange={(e) => setCurrentPassword(e.target.value)}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="newPassword">New password</Label>
                    <Input
                      id="newPassword"
                      type="password"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="confirmPassword">Confirm new password</Label>
                    <Input
                      id="confirmPassword"
                      type="password"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      required
                    />
                  </div>

                  {passwordMessage && (
                    <Alert>
                      <AlertTitle>Password updated</AlertTitle>
                      <AlertDescription>{passwordMessage}</AlertDescription>
                    </Alert>
                  )}

                  {passwordError && (
                    <Alert variant="destructive">
                      <AlertTitle>Change failed</AlertTitle>
                      <AlertDescription>{passwordError}</AlertDescription>
                    </Alert>
                  )}

                  <Button type="submit" disabled={passwordSaving} className="rounded-full">
                    <KeyRound className="mr-2 h-4 w-4" />
                    {passwordSaving ? "Changing..." : "Change password"}
                  </Button>
                </form>
              </TabsContent>

              <TabsContent value="addresses" className="space-y-5 pt-4">
                <div className="space-y-3">
                  {addressesLoading ? (
                    <p className="text-sm text-muted-foreground">Loading addresses...</p>
                  ) : addresses.length === 0 ? (
                    <p className="text-sm text-muted-foreground">
                      No saved addresses yet.
                    </p>
                  ) : (
                    addresses.map((savedAddress) => (
                      <div
                        key={savedAddress.id}
                        className="rounded-2xl border border-border/70 bg-background/70 p-4"
                      >
                        <div className="flex flex-wrap items-center justify-between gap-3">
                          <div className="flex items-center gap-2">
                            <p className="font-medium">{savedAddress.label}</p>
                            {savedAddress.isDefault && (
                              <Badge className="rounded-full">Default</Badge>
                            )}
                          </div>
                          <div className="flex flex-wrap gap-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => startEditAddress(savedAddress)}
                            >
                              <Pencil className="mr-1 h-4 w-4" />
                              Edit
                            </Button>
                            <Button
                              variant="outline"
                              size="sm"
                              disabled={savedAddress.isDefault || addressActionId === savedAddress.id}
                              onClick={() => void handleSetDefaultAddress(savedAddress.id)}
                            >
                              <ShieldCheck className="mr-1 h-4 w-4" />
                              Default
                            </Button>
                            <Button
                              variant="destructive"
                              size="sm"
                              disabled={addressActionId === savedAddress.id}
                              onClick={() => requestAddressDelete(savedAddress)}
                            >
                              <Trash2 className="mr-1 h-4 w-4" />
                              Delete
                            </Button>
                          </div>
                        </div>
                        <div className="mt-3 space-y-1 text-sm text-muted-foreground">
                          <p>
                            {savedAddress.street}
                            {savedAddress.apartmentNumber
                              ? `, ${savedAddress.apartmentNumber}`
                              : ""}
                          </p>
                          <p>
                            {savedAddress.city}, {savedAddress.state}, {savedAddress.zipCode}
                          </p>
                          <p>{savedAddress.country}</p>
                          {savedAddress.additionalInfo && <p>{savedAddress.additionalInfo}</p>}
                        </div>
                      </div>
                    ))
                  )}
                </div>

                <Separator />

                <form onSubmit={handleAddressSubmit} className="space-y-4">
                  <div className="text-sm font-medium">
                    {editingAddressId !== null ? "Edit address" : "Add new address"}
                  </div>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="space-y-2">
                      <Label htmlFor="label">Label *</Label>
                      <Input
                        id="label"
                        value={addressForm.label}
                        onChange={(e) =>
                          setAddressForm((current) => ({ ...current, label: e.target.value }))
                        }
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="country">Country *</Label>
                      <Input
                        id="country"
                        value={addressForm.country}
                        onChange={(e) =>
                          setAddressForm((current) => ({
                            ...current,
                            country: e.target.value,
                          }))
                        }
                        required
                      />
                    </div>
                  </div>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="space-y-2">
                      <Label htmlFor="city">City *</Label>
                      <Input
                        id="city"
                        value={addressForm.city}
                        onChange={(e) =>
                          setAddressForm((current) => ({ ...current, city: e.target.value }))
                        }
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="state">State / Province *</Label>
                      <Input
                        id="state"
                        value={addressForm.state}
                        onChange={(e) =>
                          setAddressForm((current) => ({ ...current, state: e.target.value }))
                        }
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="street">Street *</Label>
                    <Input
                      id="street"
                      value={addressForm.street}
                      onChange={(e) =>
                        setAddressForm((current) => ({ ...current, street: e.target.value }))
                      }
                      required
                    />
                  </div>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="space-y-2">
                      <Label htmlFor="zipCode">ZIP / Postal code *</Label>
                      <Input
                        id="zipCode"
                        value={addressForm.zipCode}
                        onChange={(e) =>
                          setAddressForm((current) => ({ ...current, zipCode: e.target.value }))
                        }
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="apartmentNumber">Apartment / Suite</Label>
                      <Input
                        id="apartmentNumber"
                        value={addressForm.apartmentNumber ?? ""}
                        onChange={(e) =>
                          setAddressForm((current) => ({
                            ...current,
                            apartmentNumber: e.target.value,
                          }))
                        }
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="additionalInfo">Additional info</Label>
                    <Textarea
                      id="additionalInfo"
                      value={addressForm.additionalInfo ?? ""}
                      onChange={(e) =>
                        setAddressForm((current) => ({
                          ...current,
                          additionalInfo: e.target.value,
                        }))
                      }
                      placeholder="Entrance, floor, delivery notes."
                    />
                  </div>
                  <div className="flex items-center gap-2">
                    <input
                      id="isDefaultAddress"
                      type="checkbox"
                      className="h-4 w-4 accent-primary"
                      checked={addressForm.isDefault}
                      onChange={(e) =>
                        setAddressForm((current) => ({
                          ...current,
                          isDefault: e.target.checked,
                        }))
                      }
                    />
                    <Label htmlFor="isDefaultAddress">Set as default address</Label>
                  </div>

                  {addressMessage && (
                    <Alert>
                      <AlertTitle>Address saved</AlertTitle>
                      <AlertDescription>{addressMessage}</AlertDescription>
                    </Alert>
                  )}

                  {addressError && (
                    <Alert variant="destructive">
                      <AlertTitle>Address operation failed</AlertTitle>
                      <AlertDescription>{addressError}</AlertDescription>
                    </Alert>
                  )}

                  <div className="flex flex-wrap gap-2">
                    <Button type="submit" disabled={addressSubmitting}>
                      {editingAddressId !== null ? (
                        <>
                          <Save className="mr-2 h-4 w-4" />
                          {addressSubmitting ? "Saving..." : "Save address"}
                        </>
                      ) : (
                        <>
                          <Plus className="mr-2 h-4 w-4" />
                          {addressSubmitting ? "Adding..." : "Add address"}
                        </>
                      )}
                    </Button>
                    {editingAddressId !== null && (
                      <Button
                        type="button"
                        variant="outline"
                        onClick={resetAddressForm}
                        disabled={addressSubmitting}
                      >
                        Cancel edit
                      </Button>
                    )}
                  </div>
                </form>
              </TabsContent>

              <WishlistTab
                items={wishlistItems}
                loading={wishlistLoading}
                actionId={wishlistActionId}
                message={wishlistMessage}
                error={wishlistError}
                onRefresh={() => {
                  void loadWishlist();
                }}
                onRemove={(productId) => {
                  void handleRemoveWishlistItem(productId);
                }}
              />

              <BonusTab />
            </Tabs>
          </CardContent>
        </Card>
      </div>

      <Dialog
        open={addressToDelete !== null}
        onOpenChange={(open) => {
          if (!open && !isDeleteInProgress) {
            setAddressToDelete(null);
          }
        }}
      >
        <DialogContent className="max-w-md" showCloseButton={!isDeleteInProgress}>
          <DialogHeader>
            <DialogTitle>Delete address?</DialogTitle>
            <DialogDescription>
              {addressToDelete
                ? `This will remove "${addressToDelete.label}" from your saved addresses.`
                : "This action cannot be undone."}
            </DialogDescription>
          </DialogHeader>
          {addressToDelete && (
            <div className="rounded-xl border border-border/70 bg-background/70 p-3 text-sm text-muted-foreground">
              {formatSavedAddress(addressToDelete)}
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setAddressToDelete(null)}
              disabled={isDeleteInProgress}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => void handleDeleteAddress()}
              disabled={isDeleteInProgress}
            >
              {isDeleteInProgress ? "Deleting..." : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
