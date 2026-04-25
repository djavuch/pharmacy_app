import { create } from "zustand";
import { CartDto, ProductDto } from "@/shared/types";
import { cartApi } from "@/shared/api";

let loadCartPromise: Promise<void> | null = null;

interface CartState {
    cart: CartDto | null;
    isLoading: boolean;
    isLoaded: boolean;
    loadCart: (force?: boolean) => Promise<void>;
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
    isLoaded: false,

    loadCart: async (force = false) => {
        if (loadCartPromise) {
            await loadCartPromise;
        }

        if (!force && get().isLoaded) {
            return;
        }

        if (loadCartPromise) {
            await loadCartPromise;
            return;
        }

        set({ isLoading: true });

        loadCartPromise = (async () => {
            try {
                const data = await cartApi.get();
                set({ cart: data, isLoading: false, isLoaded: true });
            } catch {
                set({ cart: null, isLoading: false, isLoaded: true });
            } finally {
                loadCartPromise = null;
            }
        })();

        await loadCartPromise;
    },

    addItem: async (product, quantity = 1) => {
        const data = await cartApi.add({ productId: product.id, quantity });
        set({ cart: data, isLoading: false, isLoaded: true });
    },

    removeItem: async (productId) => {
        const data = await cartApi.remove(productId);
        set({ cart: data, isLoading: false, isLoaded: true });
    },

    updateQuantity: async (productId, quantity) => {
        if (quantity <= 0) {
            await get().removeItem(productId);
            return;
        }

        const data = await cartApi.update({ productId, quantity });
        set({ cart: data, isLoading: false, isLoaded: true });
    },

    clearCart: async () => {
        await cartApi.clear();
        set({ cart: null, isLoading: false, isLoaded: true });
    },

    itemsCount: () => get().cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0,
    totalPrice: () => get().cart?.totalPrice ?? 0,
    quantityByProductId: (productId) =>
        get().cart?.items.find((item) => item.productId === productId)?.quantity ?? 0,
}));
