"use client";

import { FormEvent, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCart } from "@/features/cart/use-cart";
import { useAuthStore } from "@/entities/user";
import { categoryApi } from "@/entities/product";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import {
  HeartPulse,
  LogOut,
  Menu,
  Package,
  Search,
  Settings,
  ShoppingCart,
  User,
} from "lucide-react";
import type { CategoryDto } from "@/shared/types";

export function Header() {
  const router = useRouter();
  const { itemsCount } = useCart();
  const { profile, isAuthenticated, loadProfile } = useAuthStore();
  const [catalogOpen, setCatalogOpen] = useState(false);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [catalogLoading, setCatalogLoading] = useState(true);
  const [catalogSearch, setCatalogSearch] = useState("");

  useEffect(() => {
    void loadProfile();
  }, [loadProfile]);

  useEffect(() => {
    const loadCategories = async () => {
      setCatalogLoading(true);
      try {
        const result = await categoryApi.getAll({ pageIndex: 1, pageSize: 40 });
        setCategories(result.items ?? []);
      } catch {
        setCategories([]);
      } finally {
        setCatalogLoading(false);
      }
    };

    void loadCategories();
  }, []);

  const handleLogout = async () => {
    await useAuthStore.getState().logout();
    router.push("/");
    router.refresh();
  };

  const handleCatalogSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const query = catalogSearch.trim();
    router.push(query ? `/catalog?search=${encodeURIComponent(query)}` : "/catalog");
  };

  const isAdmin = profile?.role === "Admin";

  return (
    <header className="sticky top-0 z-50 w-full border-b border-white/40 bg-background/85 backdrop-blur-xl supports-[backdrop-filter]:bg-background/70">
      <Sheet open={catalogOpen} onOpenChange={setCatalogOpen}>
        <div className="container mx-auto flex h-18 items-center justify-between gap-4 px-4">
          <div className="flex items-center gap-3">
            <Link href="/" className="flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary/12 text-primary">
                <HeartPulse className="h-5 w-5" />
              </div>
              <div>
                <div className="font-heading text-lg font-semibold tracking-tight text-foreground">
                  PharmacyApp
                </div>
                <div className="text-xs text-muted-foreground">
                  Pharmacy and wellness delivery
                </div>
              </div>
            </Link>
            <SheetTrigger
              render={
                <Button className="h-10 rounded-xl bg-[#4f7b66] px-3 text-white hover:bg-[#466f5c]">
                  <Menu className="mr-2 h-4 w-4" />
                  Catalog
                </Button>
              }
            />
          </div>

          <form onSubmit={handleCatalogSearch} className="hidden flex-1 md:flex md:max-w-3xl">
            <div className="relative w-full">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={catalogSearch}
                onChange={(event) => setCatalogSearch(event.target.value)}
                placeholder="Search products, medicines, vitamins..."
                className="h-10 rounded-xl border-border/70 bg-background pl-9 pr-24"
              />
              <Button
                type="submit"
                size="sm"
                className="absolute right-1 top-1/2 h-8 -translate-y-1/2 rounded-lg px-3"
              >
                Search
              </Button>
            </div>
          </form>

          <div className="flex items-center gap-2">
            <Link href="/catalog" className="md:hidden">
              <Button variant="outline" className="h-10 w-10 rounded-full p-0">
                <Search className="h-4 w-4" />
              </Button>
            </Link>

            <Link href="/cart">
              <Button variant="ghost" className="relative h-10 w-10 rounded-full p-0">
                <ShoppingCart className="h-5 w-5" />
                {itemsCount > 0 && (
                  <span className="absolute -top-1 -right-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-primary-foreground">
                    {itemsCount}
                  </span>
                )}
              </Button>
            </Link>

            {isAuthenticated ? (
              <DropdownMenu>
                <DropdownMenuTrigger>
                  <div className="inline-flex h-10 w-10 shrink-0 cursor-pointer items-center justify-center rounded-full border border-border/70 bg-card text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50">
                    <User className="h-5 w-5" />
                  </div>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => router.push("/profile")}>
                    <User className="mr-2 h-4 w-4" /> Profile
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => router.push("/orders")}>
                    <Package className="mr-2 h-4 w-4" /> Orders
                  </DropdownMenuItem>
                  {isAdmin && (
                    <DropdownMenuItem onClick={() => router.push("/admin")}>
                      <Settings className="mr-2 h-4 w-4" /> Admin
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuItem onClick={handleLogout}>
                    <LogOut className="mr-2 h-4 w-4" /> Logout
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Link href="/login">
                <Button>Log In</Button>
              </Link>
            )}
          </div>
        </div>

        <SheetContent
          side="left"
          className="w-[90vw] max-w-md border-r border-border/70 bg-background/98 p-0"
        >
          <SheetHeader className="border-b border-border/70 px-5 py-4">
            <SheetTitle className="text-lg font-semibold">Catalog</SheetTitle>
            <SheetDescription>Browse categories</SheetDescription>
          </SheetHeader>
          <div className="max-h-[calc(100vh-120px)] overflow-y-auto p-4">
            {catalogLoading ? (
              <div className="space-y-2">
                {Array.from({ length: 8 }).map((_, index) => (
                  <div key={index} className="h-10 animate-pulse rounded-lg bg-muted/70" />
                ))}
              </div>
            ) : categories.length > 0 ? (
              <div className="space-y-2">
                {categories.map((category) => (
                  <Link
                    key={category.categoryId}
                    href={`/catalog?category=${encodeURIComponent(category.categoryName)}`}
                    onClick={() => setCatalogOpen(false)}
                    className="block rounded-lg border border-border/60 bg-background px-3 py-2 text-sm font-medium transition-colors hover:border-primary/35 hover:bg-primary/5"
                  >
                    {category.categoryName}
                  </Link>
                ))}
                <Link
                  href="/promotions"
                  onClick={() => setCatalogOpen(false)}
                  className="mt-3 block rounded-lg border border-primary/25 bg-primary/10 px-3 py-2 text-sm font-semibold text-primary transition-colors hover:bg-primary/15"
                >
                  Promotions
                </Link>
              </div>
            ) : (
              <p className="rounded-lg border border-dashed border-border/70 p-4 text-sm text-muted-foreground">
                Categories are temporarily unavailable.
              </p>
            )}
          </div>
        </SheetContent>
      </Sheet>
    </header>
  );
}
