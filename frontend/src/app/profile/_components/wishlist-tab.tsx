import Link from "next/link";
import { Trash2 } from "lucide-react";
import type { WishlistDto } from "@/shared/types";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { TabsContent } from "@/components/ui/tabs";

type WishlistTabProps = {
  items: WishlistDto[];
  loading: boolean;
  actionId: number | null;
  message: string | null;
  error: string | null;
  onRefresh: () => void;
  onRemove: (productId: number) => void;
};

export function WishlistTab({
  items,
  loading,
  actionId,
  message,
  error,
  onRefresh,
  onRemove,
}: WishlistTabProps) {
  return (
    <TabsContent value="wishlist" className="space-y-5 pt-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-sm text-muted-foreground">
          Manage products you saved for later.
        </p>
        <Button
          variant="outline"
          size="sm"
          onClick={onRefresh}
          disabled={loading}
        >
          {loading ? "Refreshing..." : "Refresh"}
        </Button>
      </div>

      {message && (
        <Alert>
          <AlertTitle>Wishlist updated</AlertTitle>
          <AlertDescription>{message}</AlertDescription>
        </Alert>
      )}

      {error && (
        <Alert variant="destructive">
          <AlertTitle>Wishlist operation failed</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {loading ? (
        <p className="text-sm text-muted-foreground">Loading wishlist...</p>
      ) : items.length === 0 ? (
        <div className="rounded-2xl border border-border/70 bg-background/70 p-4">
          <p className="text-sm text-muted-foreground">
            Your wishlist is empty.
          </p>
          <div className="mt-3">
            <Link href="/catalog">
              <Button size="sm">Browse catalog</Button>
            </Link>
          </div>
        </div>
      ) : (
        <div className="space-y-3">
          {items.map((item) => (
            <div
              key={item.productId}
              className="rounded-2xl border border-border/70 bg-background/70 p-4"
            >
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="font-medium">
                    {item.productName || `Product #${item.productId}`}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    ID: {item.productId}
                  </p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <Link href={`/products/${item.productId}`}>
                    <Button variant="outline" size="sm">
                      Open product
                    </Button>
                  </Link>
                  <Button
                    variant="destructive"
                    size="sm"
                    disabled={actionId === item.productId}
                    onClick={() => onRemove(item.productId)}
                  >
                    <Trash2 className="mr-1 h-4 w-4" />
                    {actionId === item.productId ? "Removing..." : "Remove"}
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </TabsContent>
  );
}
