using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Order;
using PharmacyApp.Application.DTOs.PromoCode;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Domain.Enums;
using System.Data;
using PharmacyApp.Application.Interfaces.Email;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly IOrderEmailNotifier _orderEmailNotifier;
    private readonly IPromoCodeService _promoCodeService;
    private readonly IProductService _productService;
    private readonly IBonusService _bonusService;

    public OrderService(IUnitOfWorkRepository unitOfWork,
        IOrderEmailNotifier orderEmailNotifier, IPromoCodeService
        promoCodeService, IProductService productService, IBonusService bonusService)
    {
        _unitOfWork = unitOfWork;
        _orderEmailNotifier = orderEmailNotifier;
        _promoCodeService = promoCodeService;
        _productService = productService;
        _bonusService = bonusService;
    }

    public async Task<PaginatedList<OrderListDto>> GetAllOrdersAsync(int pageIndex = 1, int pageSize = 10,
        string? filterOn = null,
        string? filterQuery = null, string? sortBy = null, bool isAscending = true)
    {
        var ordersQuery = _unitOfWork.Orders.GetAllAsync();

        //Filtering
        if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
        {
            if (filterOn.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                var fq = filterQuery.ToLower();
                ordersQuery = ordersQuery.Where(o =>
                    o.BuyerFirstName.ToLower().Contains(fq) ||
                    o.BuyerLastName.ToLower().Contains(fq));
            }

            if (filterOn.Equals("OrderStatus", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<OrderStatus>(filterQuery, ignoreCase: true, out var statusFilter))
                {
                    ordersQuery = ordersQuery.Where(o => o.OrderStatus == statusFilter);
                }
            }

            if (filterOn.Equals("OrderDate", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(filterQuery, out var orderDate))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date == orderDate.Date);
            }
        }

        var totalCount = await ordersQuery.CountAsync();

        //Sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            if (sortBy.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = isAscending
                    ? ordersQuery.OrderBy(o => o.BuyerFirstName).ThenBy(o => o.BuyerLastName)
                    : ordersQuery.OrderByDescending(o => o.BuyerFirstName).ThenByDescending(o => o.BuyerLastName);
            }

            if (sortBy.Equals("OrderStatus", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = isAscending ? ordersQuery.OrderBy(o => o.OrderStatus) : ordersQuery.OrderByDescending(o => o.OrderStatus);
            }

            if (sortBy.Equals("OrderDate", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = isAscending ? ordersQuery.OrderBy(o => o.OrderDate) : ordersQuery.OrderByDescending(o => o.OrderDate);
            }
        }

        //Pagination
        var skipResults = (pageIndex - 1) * pageSize;

        var orders = await ordersQuery.Skip(skipResults).Take(pageSize).ToListAsync();
        

        return new PaginatedList<OrderListDto>
        {
            Items = orders,
            PageIndex = pageIndex,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            PageSize = pageSize
        };
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id, string userId, bool isStaff)
    {
        var orderById = await _unitOfWork.Orders.GetByIdAsync(id);

        if (orderById == null)
            return null;

        if (!isStaff && orderById.UserId != userId)
            throw new UnauthorizedException("Access denied.");

        return orderById.ToOrderResponseDto();
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userId)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            // Start a transaction with appropriate isolation level to handle concurrency
            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                // Re-fetch the cart within the transaction to ensure we have the latest data
                try
                {
                    var cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(userId);

                    if (cart is null || !cart.Items.Any())
                    {
                        throw new NotFoundException("Cannot create order from empty cart.");
                    }

                    decimal subtotalAmount = 0;

                    var productIds = cart.Items.Select(i => i.ProductId).ToList();

                    var categoryIds = cart.Items
                        .Where(i => i.Product is not null)
                        .Select(i => i.Product!.CategoryId)
                        .Distinct()
                        .ToList();

                    // Get shipping address
                    OrderAddressModel shippingAddress;

                    if (createOrderDto.SavedAddressId.HasValue)
                    {
                        // Option 1: Use saved address
                        var savedAddress = await _unitOfWork.UserAddresses.GetByIdAsync(createOrderDto.SavedAddressId.Value);

                        if (savedAddress == null || savedAddress.UserId != userId)
                        {
                            throw new ConflictException($"User address with ID {createOrderDto.SavedAddressId.Value} not found for the current user.");
                        }

                        shippingAddress = new OrderAddressModel
                        {
                            Street = savedAddress.Street,
                            ApartmentNumber = savedAddress.ApartmentNumber,
                            City = savedAddress.City,
                            State = savedAddress.State,
                            ZipCode = savedAddress.ZipCode,
                            Country = savedAddress.Country,
                            AdditionalInfo = savedAddress.AdditionalInfo
                        };
                    }
                    else if (createOrderDto.NewAddress != null)
                    {
                        // Option 2: Use new address provided in the order DTO
                        shippingAddress = new OrderAddressModel
                        {
                            Street = createOrderDto.NewAddress.Street,
                            ApartmentNumber = createOrderDto.NewAddress.ApartmentNumber,
                            City = createOrderDto.NewAddress.City,
                            State = createOrderDto.NewAddress.State,
                            ZipCode = createOrderDto.NewAddress.ZipCode,
                            Country = createOrderDto.NewAddress.Country,
                            AdditionalInfo = createOrderDto.NewAddress.AdditionalInfo
                        };

                        // Optionally save the new address to user's saved addresses
                        if (createOrderDto.SaveAddress)
                        {
                            var newUserAddress = new UserAddressModel
                            {
                                UserId = userId,
                                Street = createOrderDto.NewAddress.Street,
                                ApartmentNumber = createOrderDto.NewAddress.ApartmentNumber,
                                City = createOrderDto.NewAddress.City,
                                State = createOrderDto.NewAddress.State,
                                ZipCode = createOrderDto.NewAddress.ZipCode,
                                Country = createOrderDto.NewAddress.Country,
                                AdditionalInfo = createOrderDto.NewAddress.AdditionalInfo,
                                Label = createOrderDto.SavedLabel ?? string.Empty,
                                IsDefault = false,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.UserAddresses.AddAsync(newUserAddress);
                        }
                    }
                    else
                    {
                        throw new BadRequestException("Shipping address must be provided.");
                    }

                    var order = new OrderModel
                    {
                        UserId = userId,
                        OrderDate = DateTime.UtcNow,
                        OrderStatus = OrderStatus.Pending,
                        OrderItems = new List<OrderItemModel>(),
                        ShippingAddress = shippingAddress
                    };

                    foreach (var item in cart.Items)
                    {
                        var product = item.Product;

                        if (product == null)
                            throw new ConflictException($"Product with ID {item.ProductId} not found.");

                        order.OrderItems.Add(new OrderItemModel
                        {
                            ProductId = item.ProductId,
                            ProductName = product.Name,
                            Quantity = item.Quantity,
                            Price = product.Price,
                            Order = order
                        });

                        subtotalAmount += product.Price * item.Quantity;
                        await _productService.UpdateStockAsync(item.ProductId, -item.Quantity);
                    }

                    order.SubtotalAmount = subtotalAmount;
                    order.TotalAmount = subtotalAmount;

                    order.DiscountAmount = 0;

                    if (!string.IsNullOrWhiteSpace(createOrderDto.PromoCode))
                    {
                        try
                        {
                            // Validate the promo code
                            var validateDto = new ValidatePromoCodeDto
                            {
                                Code = createOrderDto.PromoCode,
                                UserId = userId,
                                OrderAmount = subtotalAmount,
                                ProductIds = productIds,
                                CategoryIds = categoryIds
                            };

                            var validationResult = await _promoCodeService.ValidatePromoCodeAsync(validateDto);

                            if (validationResult.IsValid && validationResult.PromoCodeId.HasValue)
                            {
                                // Apply the discount to the order
                                var promoCodeDiscount = validationResult.DiscountAmount;

                                order.AppliedPromoCode = createOrderDto.PromoCode.ToUpper();
                                order.PromoCodeId = validationResult.PromoCodeId.Value;
                                order.PromoCodeDiscountAmount = promoCodeDiscount;
                                order.DiscountAmount += promoCodeDiscount;
                                order.TotalAmount -= promoCodeDiscount;

                                // Check if sum is not negative after applying discount
                                if (order.TotalAmount < 0)
                                    order.TotalAmount = 0;
                            }
                            else
                            {
                                // Promo code is invalid or not applicable, we can choose to either ignore it or throw an error
                                throw new BadRequestException($"Promo code error: {validationResult.Message}");
                            }
                        }
                        catch (BadRequestException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new BadRequestException($"Error applying promo code: {ex.Message}");
                        }
                    }

                    // Bonus points calculation (example: 1 point per $10 spent)
                    if (createOrderDto.RedeemBonusPoints is > 0)
                    {
                        var pointsToRedeem = Math.Min(
                            createOrderDto.RedeemBonusPoints.Value,
                            order.TotalAmount);

                        var bonusDiscount = await _bonusService.RedeemPointsAsync(
                            userId, 0, pointsToRedeem);

                        order.BonusPointsRedeemed = pointsToRedeem;
                        order.DiscountAmount += bonusDiscount;
                        order.TotalAmount -= bonusDiscount;
                        order.TotalAmount = Math.Max(0, order.TotalAmount);
                    }

                    // Save the order 
                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    // Remember: save the promo code usage only after the order is successfully created to avoid recording usage for failed orders
                    if (order.PromoCodeId.HasValue)
                    {
                        await _unitOfWork.PromoCodes.IncrementUsageAsync(order.PromoCodeId.Value);

                        var usage = new PromoCodeUsageModel
                        {
                            UsageId = Guid.NewGuid(),
                            PromoCodeId = order.PromoCodeId.Value,
                            UserId = userId,
                            OrderId = order.Id,
                            DiscountApplied = order.PromoCodeDiscountAmount,
                            UsedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.PromoCodes.RecordUsageAsync(usage);
                    }

                    await _unitOfWork.ShoppingCarts.ClearAsync(cart.Id);

                    // Reward bonus points after order creation
                    var pointsEarned = await _bonusService.EarnPointsAsync(
                        userId, order.Id, order.TotalAmount);

                    order.BonusPointsEarned = pointsEarned;

                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    await _orderEmailNotifier.SendOrderConfirmationEmailAsync(order.Id);

                    return order.ToOrderResponseDto();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            catch (DbUpdateConcurrencyException)
            {
                retryCount++;

                if (retryCount >= maxRetries)
                    throw new BadRequestException("Unable to complete the order due to concurrent updates. Please try again.");


                // Small delay before retrying to reduce contention
                await Task.Delay(100 * retryCount);

                // Retry
                continue;
            }
        }
        throw new BadRequestException("Order creation failed after multiple retries.");
    }

    public async Task UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order is null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            if (order.OrderStatus is not OrderStatus.Pending)
            {
                throw new ConflictException("Only pending orders can be updated.");
            }

            var existingProductIds = order.OrderItems.Select(i => i.ProductId).ToList();
            var existingProducts = await _unitOfWork.Products.GetByIdsAsync(existingProductIds); 
            var existingProductMap = existingProducts.ToDictionary(p => p.Id);

            foreach (var existingItem in order.OrderItems)
            {
                await _productService.UpdateStockAsync(existingItem.ProductId, existingItem.Quantity);
            }

            order.OrderItems.Clear();
            decimal newSubtotal = 0;
            var newProductIds = new List<int>();
            var newCategoryIds = new List<int>();

            var requestedProductIds = updateOrderDto.OrderItems
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var requestedProducts = await _unitOfWork.Products.GetByIdsAsync(requestedProductIds);
            var requestedProductMap = requestedProducts.ToDictionary(p => p.Id);

            foreach (var orderItem in updateOrderDto.OrderItems)
            {
                if (!requestedProductMap.TryGetValue(orderItem.ProductId, out var product))
                {
                    throw new NotFoundException($"Product with ID {orderItem.ProductId} not found.");
                }

                order.OrderItems.Add(new OrderItemModel
                {
                    ProductId = orderItem.ProductId,
                    Quantity = orderItem.Quantity,
                    Price = product.Price
                });

                newSubtotal += orderItem.Quantity * product.Price;
                newProductIds.Add(orderItem.ProductId);
                newCategoryIds.Add(product.CategoryId);
                
                await _productService.UpdateStockAsync(orderItem.ProductId, -orderItem.Quantity);
            }

            order.SubtotalAmount = newSubtotal;
            order.TotalAmount = newSubtotal;
            order.DiscountAmount = 0;
            order.PromoCodeDiscountAmount = 0;

            // Re-validate promo code against updated order contents
            if (order.PromoCodeId.HasValue && !string.IsNullOrWhiteSpace(order.AppliedPromoCode))
            {
                var validateDto = new ValidatePromoCodeDto
                {
                    Code = order.AppliedPromoCode,
                    UserId = order.UserId,
                    OrderAmount = newSubtotal,
                    ProductIds = newProductIds,
                    CategoryIds = newCategoryIds
                };

                var validationResult = await _promoCodeService.ValidatePromoCodeAsync(validateDto);

                if (validationResult.IsValid)
                {
                    order.PromoCodeDiscountAmount = validationResult.DiscountAmount;
                    order.DiscountAmount = validationResult.DiscountAmount;
                    order.TotalAmount = Math.Max(0, newSubtotal - validationResult.DiscountAmount);
                }
                else
                {
                    // Promo code no longer applicable — remove it from the order
                    await RollbackPromoCodeUsageIfAnyAsync(order);
                    
                    order.AppliedPromoCode = null;
                    order.PromoCodeId = null;
                    order.PromoCodeDiscountAmount = 0;
                }
            }

            if (updateOrderDto.ShippingAddress is not null)
            {
                order.ShippingAddress.Street = updateOrderDto.ShippingAddress.Street;
                order.ShippingAddress.ApartmentNumber = updateOrderDto.ShippingAddress.ApartmentNumber ?? string.Empty;
                order.ShippingAddress.City = updateOrderDto.ShippingAddress.City;
                order.ShippingAddress.State = updateOrderDto.ShippingAddress.State;
                order.ShippingAddress.ZipCode = updateOrderDto.ShippingAddress.ZipCode;
                order.ShippingAddress.Country = updateOrderDto.ShippingAddress.Country;
                order.ShippingAddress.AdditionalInfo = updateOrderDto.ShippingAddress.AdditionalInfo ?? string.Empty;
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();
            
            await _orderEmailNotifier.SendOrderCompositionChangeEmailAsync(order.Id);
        }
        catch (DbUpdateConcurrencyException) 
        {
            await transaction.RollbackAsync();
            throw new ConflictException("Order was modified by another request. Please retry.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CancelOrderAsync(int orderId, string userId, bool isStaff)
    {

        await using var transaction = await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order is null)
                throw new NotFoundException($"Order with ID {orderId} not found.");

            if (!isStaff && order.UserId != userId)
                throw new UnauthorizedException("Access denied.");

            if (order.OrderStatus != OrderStatus.Pending)
                throw new ConflictException("Only pending orders can be cancelled.");

            await RestoreStockForOrderItemsAsync(order.OrderItems);
            await RollbackPromoCodeUsageIfAnyAsync(order);

            await _bonusService.ReverseOrderBonusesAsync(order.UserId, order.Id);

            order.OrderStatus = OrderStatus.Cancelled;
            
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();
            
            await _orderEmailNotifier.SendOrderCancellationEmailAsync(order.Id);
        }
        catch (DbUpdateConcurrencyException) 
        {
            await transaction.RollbackAsync();
            throw new ConflictException("Order was modified by another request. Please retry.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Admin specific methods   
    public async Task UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order is null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            var oldStatus = order.OrderStatus;
            var newStatus = updateOrderStatusDto.Status;
            if (oldStatus == newStatus) return;

            // Restore stock if order is being canceled
            if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                await RestoreStockForOrderItemsAsync(order.OrderItems);
                // Rollback promo code usage
                await RollbackPromoCodeUsageIfAnyAsync(order);
            }

            order.OrderStatus = newStatus;
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();

            await _orderEmailNotifier.SendOrderStatusUpdateEmailAsync(order.Id, oldStatus.ToString(), newStatus.ToString());
        }
        catch (DbUpdateConcurrencyException)  
        {
            await transaction.RollbackAsync();
            throw new ConflictException("Order was modified by another request. Please retry.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    // Specific methods for optimize queries
    private async Task RestoreStockForOrderItemsAsync(IReadOnlyCollection<OrderItemModel> orderItems)
    {
        if (orderItems.Count == 0)
            return;
        
        foreach (var item in orderItems)
        {
            await _productService.UpdateStockAsync(item.ProductId, item.Quantity);
        }
    }

    private async Task RollbackPromoCodeUsageIfAnyAsync(OrderModel order)
    {
        if (!order.PromoCodeId.HasValue)
            return;

        var removed = await _unitOfWork.PromoCodes.RemoveUsageByOrderIdAsync(order.Id);

        if (removed > 0)
        {
            await _unitOfWork.PromoCodes.DecrementUsageAsync(order.PromoCodeId.Value);
        }
    }
}