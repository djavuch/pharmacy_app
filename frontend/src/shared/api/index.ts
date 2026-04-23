import {
  ApiResponse,
  PaginatedResponse,
  ProductDto,
  CategoryDto,
  QueryParams,
  CartDto,
  AddToCartDto,
  UpdateCartItemDto,
  OrderDetailsDto,
  OrderSummaryDto,
  CreateOrderDto,
  UserProfileDto,
  UserOrderSummaryDto,
  UserRegistrationDto,
  LoginResult,
  RefreshTokenResult,
  UserLoginDto,
  ResetPasswordRequestDto,
  UploadImageResponseDto,
  WishlistDto,
  SaveAddressDto,
  SavedAddressDto,
  ProductReviewDto,
  CreateProductReviewDto,
  AdminReviewDto,
  ReviewQueryParams,
  AdminDashboardSummaryDto,
  AdminUserDto,
  DiscountDto,
  CreateDiscountDto,
  UpdateDiscountDto,
  PromoCodeDto,
  CreatePromoCodeDto,
  UpdatePromoCodeDto,
  BonusSettingsDto,
  UpdateBonusSettingsDto,
  BonusAccountDto,
  BonusTransactionDto,
  AdjustBonusDto,
  ContentPageDto,
  AdminContentPageDto,
  UpdateContentPageDto,
  PromotionListItemDto,
  PromotionDetailsDto,
} from "@/shared/types";

const DEFAULT_API_URL = "https://localhost:7072";
// Backend routes are mounted at root (e.g. /account/login), so do not include "/api" here.
const API_URL = (process.env.NEXT_PUBLIC_API_URL || DEFAULT_API_URL).trim().replace(/\/+$/, "");

function extractApiErrorMessage(errorBody: unknown, status: number): string {
  if (typeof errorBody !== "object" || errorBody === null) {
    return `HTTP ${status}`;
  }

  const candidate = errorBody as {
    message?: string;
    title?: string;
    errors?: Record<string, string[] | string>;
  };

  if (candidate.errors && typeof candidate.errors === "object") {
    const validationMessages = Object.values(candidate.errors).flatMap((value) =>
      Array.isArray(value) ? value : [value]
    );

    if (validationMessages.length > 0) {
      return validationMessages.join(" ");
    }
  }

  if (candidate.message && candidate.message.trim().length > 0) {
    return candidate.message;
  }

  if (candidate.title && candidate.title.trim().length > 0) {
    return candidate.title;
  }

  return `HTTP ${status}`;
}

async function getAccessToken(): Promise<string | null> {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("accessToken");
}

async function getRefreshToken(): Promise<string | null> {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("refreshToken");
}

function setTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem("accessToken", accessToken);
  localStorage.setItem("refreshToken", refreshToken);
}

function clearTokens(): void {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
}

async function buildHeaders(requireAuth = false, hasJsonBody = false): Promise<Record<string, string>> {
  const headers: Record<string, string> = {};
  if (hasJsonBody) {
    headers["Content-Type"] = "application/json";
  }
  if (requireAuth) {
    const token = await getAccessToken();
    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }
  }
  return headers;
}

async function request<T>(endpoint: string, method: string, body?: unknown, requireAuth = false): Promise<T> {
  const hasJsonBody = body !== undefined && body !== null;
  const headers = await buildHeaders(requireAuth, hasJsonBody);

  const response = await fetch(`${API_URL}${endpoint}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
    credentials: "include",
  });

  // 401 — try refresh
  if (response.status === 401 && requireAuth) {
    const refreshToken = await getRefreshToken();
    if (refreshToken) {
      try {
        const refreshResponse = await fetch(`${API_URL}/account/refresh-token`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ refreshToken }),
          credentials: "include",
        });

        if (refreshResponse.ok) {
          const data: RefreshTokenResult = await refreshResponse.json();
          setTokens(data.accessToken, data.refreshToken);

          headers["Authorization"] = `Bearer ${data.accessToken}`;
          const retryResponse = await fetch(`${API_URL}${endpoint}`, {
            method,
            headers,
            body: body ? JSON.stringify(body) : undefined,
            credentials: "include",
          });

          if (!retryResponse.ok) {
            const err = await retryResponse.json().catch(() => ({ message: `HTTP ${retryResponse.status}` }));
            throw new Error(err.message || `HTTP ${retryResponse.status}`);
          }

          return retryResponse.json();
        }
      } catch {
        clearTokens();
        if (typeof window !== "undefined") window.location.href = "/login";
        throw new Error("Session expired");
      }
    }

    clearTokens();
    if (typeof window !== "undefined") window.location.href = "/login";
    throw new Error("Unauthorized");
  }

  if (!response.ok) {
    const err = await response.json().catch(() => null);
    throw new Error(extractApiErrorMessage(err, response.status));
  }

  // 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

async function requestFormData<T>(endpoint: string, method: string, body: FormData, requireAuth = false): Promise<T> {
  const headers: Record<string, string> = {};
  if (requireAuth) {
    const token = await getAccessToken();
    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }
  }

  const response = await fetch(`${API_URL}${endpoint}`, {
    method,
    headers,
    body,
    credentials: "include",
  });

  if (!response.ok) {
    const err = await response.json().catch(() => null);
    throw new Error(extractApiErrorMessage(err, response.status));
  }

  return response.json();
}

// ========== ACCOUNT ==========
export const accountApi = {
  login: (dto: UserLoginDto) =>
    request<LoginResult>("/account/login", "POST", dto),

  register: (dto: UserRegistrationDto) =>
    request<LoginResult>("/account/register", "POST", dto),

  resendConfirmation: (email: string) =>
    request<{ message: string }>(`/account/resend-confirmation?email=${encodeURIComponent(email)}`, "POST"),

  confirmEmail: (userId: string, token: string) =>
    request<{ message: string }>(
      `/account/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`,
      "GET"
    ),

  forgotPassword: (email: string) =>
    request<{ message: string }>("/account/forgot-password", "POST", { email }),

  resetPassword: (dto: ResetPasswordRequestDto) =>
    request<{ message: string }>("/account/reset-password", "POST", dto),

  refreshToken: (refreshToken: string) =>
    request<RefreshTokenResult>("/account/refresh-token", "POST", { refreshToken }),

  logout: (refreshToken: string) =>
    request<{ message: string }>("/account/logout", "POST", { refreshToken }, true),

  changePassword: (currentPassword: string, newPassword: string, confirmPassword: string) =>
    request<{ message: string }>("/account/change-password", "POST", {
      currentPassword,
      newPassword,
      confirmPassword,
    }, true),
};

// ========== USER ==========
export const userApi = {
  getProfile: () =>
    request<UserProfileDto>("/users/me", "GET", undefined, true),

  updateProfile: (dto: { firstName?: string; lastName?: string; phoneNumber?: string }) =>
    request<void>("/users/me", "PUT", dto, true),

  getOrders: async (params?: QueryParams) => {
    const qs = buildQueryString(params);
    const result = await request<
      PaginatedResponse<UserOrderSummaryDto | null> | UserOrderSummaryDto[]
    >(`/users/me/orders${qs}`, "GET", undefined, true);

    const items = Array.isArray(result) ? result : result.items ?? [];
    return items.filter((order): order is UserOrderSummaryDto => Boolean(order));
  },
};

// ========== ADDRESSES ==========
export const addressApi = {
  getAll: () =>
    request<SavedAddressDto[]>("/user/addresses", "GET", undefined, true),

  create: (dto: SaveAddressDto) =>
    request<SavedAddressDto>("/user/addresses", "POST", dto, true),

  update: (id: number, dto: SaveAddressDto) =>
    request<SavedAddressDto>(`/user/addresses/${id}`, "PUT", dto, true),

  remove: (id: number) =>
    request<void>(`/user/addresses/${id}`, "DELETE", undefined, true),

  setDefault: (id: number) =>
    request<void>(`/user/addresses/${id}/default`, "PATCH", undefined, true),
};

// ========== WISHLIST ==========
export const wishlistApi = {
  get: () =>
    request<WishlistDto[]>("/user/wishlist", "GET", undefined, true),

  add: (dto: WishlistDto) =>
    request<WishlistDto>("/user/wishlist", "POST", dto, true),

  remove: (productId: number) =>
    request<void>(`/user/wishlist/${productId}`, "DELETE", undefined, true),
};

// ========== PRODUCTS (Public) ==========
export const productApi = {
  getAll: (params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<ApiResponse<PaginatedResponse<ProductDto>>>(`/store/products${qs}`, "GET");
  },

  getById: (id: number) =>
    request<ProductDto>(`/store/products/${id}`, "GET"),

  getByCategory: (categoryName: string, params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<ApiResponse<PaginatedResponse<ProductDto>>>(`/store/products/category/${encodeURIComponent(categoryName)}${qs}`, "GET");
  },
};

// ========== REVIEWS ==========
export const reviewApi = {
  getByProduct: (productId: number, params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<PaginatedResponse<ProductReviewDto>>(
      `/products/${productId}/all-reviews${qs}`,
      "GET",
    );
  },

  add: (productId: number, dto: CreateProductReviewDto) =>
    request<ProductReviewDto>(
      `/products/${productId}/add-review`,
      "POST",
      dto,
      true,
    ),
};

// ========== CATEGORIES ==========
export const categoryApi = {
  getAll: (params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<PaginatedResponse<CategoryDto>>(`/store/categories${qs}`, "GET");
  },

  getById: (id: number) =>
    request<CategoryDto>(`/store/categories/${id}`, "GET"),
};

// ========== CONTENT PAGES (Public) ==========
export const contentPageApi = {
  getBySlug: (slug: string) =>
    request<ContentPageDto>(`/content-pages/${encodeURIComponent(slug)}`, "GET"),
};

// ========== PROMOTIONS (Public) ==========
export const promotionApi = {
  getAll: () =>
    request<PromotionListItemDto[]>("/promotions", "GET"),

  getBySlug: (slug: string) =>
    request<PromotionDetailsDto>(`/promotions/${encodeURIComponent(slug)}`, "GET"),
};

// ========== SHOPPING CART ==========
export const cartApi = {
  get: () =>
      request<CartDto>("/cart", "GET", undefined, true),

  add: (dto: AddToCartDto) =>
      request<CartDto>("/cart", "POST", dto, true),

  update: (dto: UpdateCartItemDto) =>
      request<CartDto>(`/cart/items/${dto.productId}`, "PUT", dto, true),

  remove: (productId: number) =>
      request<void>(`/cart/items/${productId}`, "DELETE", undefined, true),

  clear: () =>
      request<void>("/cart", "DELETE", undefined, true),
};

// ========== ORDERS ==========
export const orderApi = {
  create: (dto: CreateOrderDto) =>
    request<OrderDetailsDto>("/orders", "POST", dto, true),

  getById: (id: number) =>
    request<OrderDetailsDto>(`/orders/${id}`, "GET", undefined, true),

  cancel: (id: number) =>
    request<void>(`/orders/${id}/cancel`, "POST", undefined, true),
};

// ========== ADMIN: PRODUCTS ==========
export const adminProductApi = {
  getAll: (params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<ApiResponse<PaginatedResponse<ProductDto>>>(`/admin/products${qs}`, "GET", undefined, true);
  },

  getById: (id: number) =>
    request<ProductDto>(`/admin/products/${id}`, "GET", undefined, true),

  create: (dto: { name: string; description?: string; categoryId: number; price: number; stockQuantity: number; imageUrl?: string }) =>
    request<ProductDto>("/admin/products", "POST", dto, true),

  update: (dto: { productId: number; name?: string; description?: string; categoryId?: number; price?: number; stockQuantity?: number; imageUrl?: string }) =>
    request<void>(`/admin/products/${dto.productId}`, "PUT", dto, true),

  delete: (id: number) =>
    request<void>(`/admin/products/${id}`, "DELETE", undefined, true),

  uploadImage: (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return requestFormData<UploadImageResponseDto>("/admin/products/upload-image", "POST", formData, true);
  },
};

// ========== ADMIN: DASHBOARD ==========
export const adminDashboardApi = {
  getSummary: () =>
    request<ApiResponse<AdminDashboardSummaryDto>>("/admin/dashboard/summary", "GET", undefined, true),
};

// ========== ADMIN: ORDERS ==========
export const adminOrderApi = {
  getAll: (params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<ApiResponse<PaginatedResponse<OrderSummaryDto>>>(`/admin/orders${qs}`, "GET", undefined, true);
  },

  getById: (id: number) =>
    request<OrderDetailsDto>(`/orders/${id}`, "GET", undefined, true),

  updateStatus: (orderId: number, status: string) =>
    request<void>(`/admin/orders/${orderId}/status`, "PATCH", { orderId, status }, true),
};

// ========== ADMIN: USERS ==========
export const adminUserApi = {
  getAll: (params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<ApiResponse<PaginatedResponse<AdminUserDto>>>(`/admin/users${qs}`, "GET", undefined, true);
  },

  changeRole: (userId: string, role: string) =>
    request<ApiResponse<AdminUserDto>>(`/admin/users/${encodeURIComponent(userId)}/role`, "PATCH", { role }, true),

  lock: (userId: string, lockoutEnd?: string) =>
    request<ApiResponse<AdminUserDto>>(
      `/admin/users/${encodeURIComponent(userId)}/lock`,
      "POST",
      lockoutEnd ? { lockoutEnd } : {},
      true,
    ),

  unlock: (userId: string) =>
    request<ApiResponse<AdminUserDto>>(`/admin/users/${encodeURIComponent(userId)}/unlock`, "POST", {}, true),
};

// ========== ADMIN: DISCOUNTS ==========
export const adminDiscountApi = {
  getAll: () =>
    request<DiscountDto[]>("/admin/discounts", "GET", undefined, true),

  getById: (discountId: string) =>
    request<DiscountDto>(`/admin/discounts/${discountId}`, "GET", undefined, true),

  create: (dto: CreateDiscountDto) =>
    request<DiscountDto>("/admin/discounts", "POST", dto, true),

  update: (discountId: string, dto: UpdateDiscountDto) =>
    request<void>(`/admin/discounts/${discountId}`, "PUT", dto, true),

  delete: (discountId: string) =>
    request<void>(`/admin/discounts/${discountId}`, "DELETE", undefined, true),
};

// ========== ADMIN: PROMO CODES ==========
export const adminPromoCodeApi = {
  getAll: () =>
    request<PromoCodeDto[]>("/admin/promo-codes", "GET", undefined, true),

  getById: (promoCodeId: string) =>
    request<PromoCodeDto>(`/admin/promo-codes/${promoCodeId}`, "GET", undefined, true),

  create: (dto: CreatePromoCodeDto) =>
    request<PromoCodeDto>("/admin/promo-codes", "POST", dto, true),

  update: (promoCodeId: string, dto: UpdatePromoCodeDto) =>
    request<void>(`/admin/promo-codes/${promoCodeId}`, "PUT", dto, true),

  delete: (promoCodeId: string) =>
    request<void>(`/admin/promo-codes/${promoCodeId}`, "DELETE", undefined, true),

  activate: (promoCodeId: string) =>
    request<void>(`/admin/promo-codes/${promoCodeId}/activate`, "PATCH", undefined, true),

  deactivate: (promoCodeId: string) =>
    request<void>(`/admin/promo-codes/${promoCodeId}/deactivate`, "PATCH", undefined, true),
};

// ========== ADMIN: REVIEWS ==========
export const adminReviewApi = {
  getAll: (params?: ReviewQueryParams) => {
    const search = new URLSearchParams();
    if (params?.pageIndex != null) search.set("pageIndex", String(params.pageIndex));
    if (params?.pageSize != null) search.set("pageSize", String(params.pageSize));
    if (params?.filterOn) search.set("filterOn", params.filterOn);
    if (params?.filterQuery) search.set("filterQuery", params.filterQuery);
    if (params?.sortBy) search.set("sortBy", params.sortBy);
    if (params?.isAscending != null) search.set("isAscending", String(params.isAscending));
    if (params?.status) search.set("status", params.status);
    const qs = search.toString();

    return request<PaginatedResponse<AdminReviewDto>>(
      `/admin/reviews/all${qs ? `?${qs}` : ""}`,
      "GET",
      undefined,
      true,
    );
  },

  approve: (reviewId: number) =>
    request<{ message: string }>(`/admin/reviews/${reviewId}/approve`, "POST", undefined, true),

  reject: (reviewId: number) =>
    request<void>(`/admin/reviews/${reviewId}/reject`, "POST", undefined, true),
};

// ========== ADMIN: CATEGORIES ==========
export const adminCategoryApi = {
  getAll: (params?: QueryParams) => {
    const qs = buildQueryString(params);
    return request<PaginatedResponse<CategoryDto>>(`/admin/categories/all-categories${qs}`, "GET", undefined, true);
  },

  getById: (id: number) =>
    request<CategoryDto>(`/admin/categories/${id}`, "GET", undefined, true),

  create: (dto: { categoryName: string; categoryDescription?: string }) =>
    request<CategoryDto>("/admin/categories", "POST", dto, true),

  update: (dto: { categoryId: number; categoryName: string; categoryDescription?: string }) =>
    request<CategoryDto>(`/admin/categories/${dto.categoryId}`, "PUT", dto, true),

  delete: (id: number) =>
    request<void>(`/admin/categories/${id}`, "DELETE", undefined, true),
};

// ========== ADMIN: BONUS ==========
export const adminBonusApi = {
  getSettings: () =>
    request<BonusSettingsDto>("/bonus/settings", "GET"),

  updateSettings: (dto: UpdateBonusSettingsDto) =>
    request<BonusSettingsDto>("/admin/bonus/settings", "PUT", dto, true),

  getAccount: (userId: string) =>
    request<BonusAccountDto>(`/admin/bonus/accounts/${encodeURIComponent(userId)}`, "GET", undefined, true),

  getTransactions: (userId: string, pageIndex = 1, pageSize = 20) =>
    request<BonusTransactionDto[]>(
      `/admin/bonus/accounts/${encodeURIComponent(userId)}/transactions?pageIndex=${pageIndex}&pageSize=${pageSize}`,
      "GET",
      undefined,
      true,
    ),

  adjust: (userId: string, dto: AdjustBonusDto) =>
    request<BonusAccountDto>(
      `/admin/bonus/accounts/${encodeURIComponent(userId)}/adjust`,
      "POST",
      dto,
      true,
    ),
};

// ========== ADMIN: CONTENT PAGES ==========
export const adminContentPageApi = {
  getAll: () =>
    request<AdminContentPageDto[]>("/admin/content-pages", "GET", undefined, true),

  getBySlug: (slug: string) =>
    request<AdminContentPageDto>(`/admin/content-pages/${encodeURIComponent(slug)}`, "GET", undefined, true),

  update: (slug: string, dto: UpdateContentPageDto) =>
    request<AdminContentPageDto>(`/admin/content-pages/${encodeURIComponent(slug)}`, "PUT", dto, true),
};

// ========== USER: BONUS ==========
export const bonusApi = {
  getAccount: () =>
    request<BonusAccountDto>("/bonus/account", "GET", undefined, true),

  getTransactions: (pageIndex = 1, pageSize = 20) =>
    request<BonusTransactionDto[]>(
      `/bonus/transactions?pageIndex=${pageIndex}&pageSize=${pageSize}`,
      "GET",
      undefined,
      true,
    ),

  getSettings: () =>
    request<BonusSettingsDto>("/bonus/settings", "GET"),
};

// ========== HELPERS ==========
function buildQueryString(params?: QueryParams): string {
  if (!params) return "";
  const search = new URLSearchParams();
  if (params.pageIndex != null) search.set("pageIndex", String(params.pageIndex));
  if (params.pageSize != null) search.set("pageSize", String(params.pageSize));
  if (params.filterOn) search.set("filterOn", params.filterOn);
  if (params.filterQuery) search.set("filterQuery", params.filterQuery);
  if (params.sortBy) search.set("sortBy", params.sortBy);
  if (params.isAscending != null) search.set("isAscending", String(params.isAscending));
  if (params.categoryName) search.set("categoryName", params.categoryName);
  if (params.saleOnly != null) search.set("saleOnly", String(params.saleOnly));
  const qs = search.toString();
  return qs ? `?${qs}` : "";
}

export { setTokens, clearTokens, getAccessToken };
