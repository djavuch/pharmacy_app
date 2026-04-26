// === Product ===
export interface ProductDto {
  id: number;
  productCode?: string;
  name: string;
  description?: string;
  price: number;
  discountedPrice?: number;
  stockQuantity: number;
  categoryId: number;
  categoryName: string;
  wishlistCount: number;
  averageRating?: number;
  reviewCount?: number;
  imageUrl?: string;
}

export interface UploadImageResponseDto {
  url: string;
}

export interface ProductReviewDto {
  id: number;
  fullName?: string;
  productId: number;
  productName?: string;
  content?: string;
  rating: number;
  createdAt: string;
}

export interface CreateProductReviewDto {
  productId: number;
  rating: number;
  content: string;
}

export type ReviewStatus = "Pending" | "Approved" | "Rejected";

export interface AdminReviewDto {
  id: number;
  productId: number;
  productName?: string;
  userId?: string;
  userName?: string;
  rating: number;
  content?: string;
  createdAt: string;
  status: ReviewStatus;
}

export interface ReviewQueryParams extends QueryParams {
  status?: ReviewStatus;
}

export interface AdminDashboardSummaryDto {
  totalProducts: number;
  ordersToday: number;
  monthlyRevenue: number;
}

export type UserRole = "Admin" | "Pharmacist" | "Manager" | "Customer";

export interface AdminUserDto {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  dateOfBirth: string;
  createdAt: string;
  role?: UserRole | string;
  isLockedOut: boolean;
  lockoutEnd?: string;
  accessFailedCount: number;
}

// === Content pages ===
export interface ContentPageDto {
  slug: string;
  title: string;
  content: string;
  updatedAt: string;
}

export interface AdminContentPageDto extends ContentPageDto {
  id: number;
  isPublished: boolean;
  createdAt: string;
  updatedBy?: string;
}

export interface UpdateContentPageDto {
  title: string;
  content: string;
  isPublished: boolean;
}

// === Discounts ===
export type DiscountType = "Percentage" | "FixedAmount";

export interface DiscountDto {
  discountId: string;
  name: string;
  description: string;
  discountType: DiscountType;
  value: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  minimumOrderAmount?: number;
  maximumOrderAmount?: number;
  productIds: number[];
  categoryIds: number[];
}

export interface CreateDiscountDto {
  name: string;
  description?: string;
  discountType: DiscountType;
  value: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  minimumOrderAmount?: number;
  maximumOrderAmount?: number;
  productIds: number[];
  categoryIds: number[];
}

export type UpdateDiscountDto = CreateDiscountDto;

export interface PromotionListItemDto {
  discountId: string;
  slug: string;
  name: string;
  description: string;
  discountType: DiscountType;
  value: number;
  startDate: string;
  endDate: string;
  productTargetsCount: number;
  categoryTargetsCount: number;
}

export interface PromotionDetailsDto {
  discountId: string;
  slug: string;
  name: string;
  description: string;
  discountType: DiscountType;
  value: number;
  startDate: string;
  endDate: string;
  productTargetsCount: number;
  categoryTargetsCount: number;
  products: ProductDto[];
}

// === Promo codes ===
export interface PromoCodeDto {
  promoCodeId: string;
  code: string;
  description: string;
  discountType: DiscountType;
  value: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  maxUsageCount?: number;
  currentUsageCount: number;
  maxUsagePerUser?: number;
  minimumOrderAmount?: number;
  maximumDiscountAmount?: number;
  applicableToAllProducts: boolean;
  productIds: number[];
  categoryIds: number[];
}

export interface CreatePromoCodeDto {
  code: string;
  description?: string;
  discountType: DiscountType;
  value: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  maxUsageCount?: number;
  maxUsagePerUser?: number;
  minimumOrderAmount?: number;
  maximumDiscountAmount?: number;
  applicableToAllProducts: boolean;
  productIds: number[];
  categoryIds: number[];
}

export type UpdatePromoCodeDto = CreatePromoCodeDto;

export interface PromoCodeValidationDto {
  code: string;
  userId: string;
  orderAmount: number;
  productIds: number[];
  categoryIds: number[];
}

export interface PromoCodeValidationResultDto {
  isValid: boolean;
  message: string;
  discountAmount: number;
  promoCodeId?: string;
}

// === Bonus ===
export type BonusTransactionType = "Earned" | "Redeemed" | "Refunded" | "AdminAdjustment";

export interface BonusAccountDto {
  id: string;
  userId: string;
  fullName?: string;
  balance: number;
  createdAt: string;
  updatedAt: string;
}

export interface BonusTransactionDto {
  id: string;
  type: BonusTransactionType;
  points: number;
  description: string;
  orderId?: number;
  createdAt: string;
}

export interface BonusSettingsDto {
  earningRate: number;
  minOrderAmountToEarn: number;
  maxRedeemPercent: number;
  isEarningEnabled: boolean;
  isRedemptionEnabled: boolean;
  updatedAt: string;
}

export interface UpdateBonusSettingsDto {
  earningRate: number;
  minOrderAmountToEarn: number;
  maxRedeemPercent: number;
  isEarningEnabled: boolean;
  isRedemptionEnabled: boolean;
}

export interface AdjustBonusDto {
  points: number;
  reason: string;
}

// === Category ===
export interface CategoryDto {
  categoryId: number;
  categoryName: string;
  categoryDescription: string;
}

// === Cart ===
export interface CartDto {
  id: number;
  userId?: string;
  items: CartItemDto[];
  totalPrice: number;
  lastModifiedAt: string;
}

export interface CartItemDto {
  cartId: number;
  productId: number;
  productName: string;
  quantity: number;
  price: number;
  subtotal: number;
  availableStock: number;
}

export interface AddToCartDto {
  productId: number;
  quantity: number;
}

export interface UpdateCartItemDto {
  productId: number;
  quantity: number;
}

// === Wishlist ===
export interface WishlistDto {
  productId: number;
  productName?: string;
}

// === Order ===
export type OrderStatus =
  | "Pending"
  | "Processing"
  | "Shipped"
  | "Delivered"
  | "Cancelled"
  | "Returned";

export interface OrderAddressDto {
  street: string;
  apartmentNumber?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  additionalInfo?: string;
}

export interface OrderItemResponseDto {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
  subtotal: number;
}

export interface OrderDetailsDto {
  id: number;
  buyerFirstName: string;
  buyerLastName: string;
  buyerFullName: string;
  orderDate: string;
  totalAmount: number;
  orderStatus: OrderStatus;
  shippingAddress?: OrderAddressDto;
  orderItems: OrderItemResponseDto[];
  appliedPromoCode?: string;
  promoCodeId?: string;
  promoCodeDiscountAmount: number;
  bonusPointsRedeemed: number;
  bonusPointsEarned: number;
}

export interface OrderSummaryDto {
  id: number;
  orderDate: string;
  totalAmount: number;
  orderStatus: OrderStatus;
  buyerFirstName: string;
  buyerLastName: string;
  buyerFullName: string;
  itemsCount: number;
}

export interface CreateOrderDto {
  savedAddressId?: number;
  newAddress?: OrderAddressDto;
  saveAddress: boolean;
  savedLabel?: string;
  promoCode?: string;
  redeemBonusPoints?: number;
}

// === User ===
export interface UserProfileDto {
  id: string;
  userName: string;
  email: string;
  role?: UserRole | string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  dateOfBirth: string;
  createdAt: string;
}

export interface UserOrderSummaryDto {
  orderId: number;
  orderDate: string;
  totalAmount: number;
  orderStatus: OrderStatus;
  shippingAddress?: OrderAddressDto;
  appliedPromoCode?: string;
  promoCodeDiscountAmount: number;
  bonusPointsRedeemed: number;
  bonusPointsEarned: number;
  itemsCount: number;
  orderItems: OrderItemResponseDto[];
}

export interface UpdateUserDto {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

export interface AddressDetailsDto {
  street: string;
  apartmentNumber?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  additionalInfo?: string;
}

export interface SaveAddressDto extends AddressDetailsDto {
  label: string;
  isDefault: boolean;
}

export interface SavedAddressDto extends AddressDetailsDto {
  id: number;
  label: string;
  isDefault: boolean;
}

// === Auth ===
export interface UserLoginDto {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface UserRegistrationDto {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  phoneNumber?: string;
}

export interface LoginResult {
  succeeded: boolean;
  token?: string;
  refreshToken?: string;
  userId?: string;
  failureReason?: string;
  message?: string;
}

export interface RefreshTokenResult {
  accessToken: string;
  refreshToken: string;
  message: string;
}

export interface ResetPasswordRequestDto {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

// === Pagination ===
export interface QueryParams {
  pageIndex?: number;
  pageSize?: number;
  filterOn?: string;
  filterQuery?: string;
  sortBy?: string;
  isAscending?: boolean;
  categoryName?: string;
  saleOnly?: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResponse<T> {
  data?: T;
  message?: string;
  success: boolean;
  errors?: string[];
}
