import { create } from "zustand";
import { CartDto, ProductDto } from "@/shared/types";
import { cartApi } from "@/shared/api";

interface CartState {
    cart: CartDto | null;
    isLoading: boolean;
    loadCart: () => Promise<void>;
    addItem: (product: ProductDto, quantity?: number) => Promise<void>;
    removeItem: (productId: number) => Promise<void>;
    updateQuantity: (productId: number, quantity: number) => Promise<void>;
    clearCart: () => Promise<void>;
    itemsCount: () => number;
    totalPrice: () => number;
    quantityByProductId: (productId: number) => number;
}

export const useCartStore = create<CartState>((set, get) => ({
    cart: null,
    isLoading: true,

    loadCart: async () => {
        try {
            const data = await cartApi.get();
            set({ cart: data, isLoading: false });
        } catch {
            set({ cart: null, isLoading: false });
        }
    },

    addItem: async (product, quantity = 1) => {
        const data = await cartApi.add({ productId: product.id, quantity });
        set({ cart: data });
    },

    removeItem: async (productId) => {
        await cartApi.remove(productId);
        await get().loadCart();
    },

    updateQuantity: async (productId, quantity) => {
        if (quantity <= 0) {
            await get().removeItem(productId);
            return;
        }

        const data = await cartApi.update({ productId, quantity });
        set({ cart: data });
    },

    clearCart: async () => {
        await cartApi.clear();
        set({ cart: null });
    },

    itemsCount: () => get().cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0,
    totalPrice: () => get().cart?.totalPrice ?? 0,
    quantityByProductId: (productId) =>
        get().cart?.items.find((item) => item.productId === productId)?.quantity ?? 0,
}));