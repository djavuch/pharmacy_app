import { useEffect } from "react";
import { useCartStore } from "@/shared/stores/cart";

export function useCart() {
  const cart = useCartStore((state) => state.cart);
  const isLoading = useCartStore((state) => state.isLoading);
  const isLoaded = useCartStore((state) => state.isLoaded);
  const loadCart = useCartStore((state) => state.loadCart);
  const addItem = useCartStore((state) => state.addItem);
  const removeItem = useCartStore((state) => state.removeItem);
  const updateQuantity = useCartStore((state) => state.updateQuantity);
  const clearCart = useCartStore((state) => state.clearCart);
  const itemsCountValue = useCartStore((state) => state.itemsCount());
  const totalPriceValue = useCartStore((state) => state.totalPrice());
  const quantityByProductId = useCartStore((state) => state.quantityByProductId);

  useEffect(() => {
    if (!isLoaded) {
      void loadCart();
    }
  }, [isLoaded, loadCart]);

  return {
    cart,
    isLoading,
    itemsCount: itemsCountValue,
    totalPrice: totalPriceValue,
    refreshCart: () => loadCart(true),
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    quantityByProductId,
  };
}
