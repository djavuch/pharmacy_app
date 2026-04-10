using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Domain.Enums;
using System.Data;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Application.Contracts.PromoCode;
using PharmacyApp.Application.Contracts.PromoCode.Results;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Common;

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

    public async Task<PaginatedList<OrderSummaryDto>> GetAllOrdersAsync(QueryParams query)
    {
        var ordersQuery = _unitOfWork.Orders.GetAllAsync();

        //Filtering
        if (!string.IsNullOrWhiteSpace(query.FilterOn) && !string.IsNullOrWhiteSpace(query.FilterQuery))
        {
            if (query.FilterOn.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                var fq = query.FilterQuery.ToLower();
                ordersQuery = ordersQuery.Where(o =>
                    o.BuyerFirstName.ToLower().Contains(fq) ||
                    o.BuyerLastName.ToLower().Contains(fq));
            }

            if (query.FilterOn.Equals("OrderStatus", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<OrderStatus>(query.FilterQuery, ignoreCase: true, out var statusFilter))
                {
                    ordersQuery = ordersQuery.Where(o => o.OrderStatus == statusFilter);
                }
            }

            if (query.FilterOn.Equals("OrderDate", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(query.FilterQuery, out var orderDate))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date == orderDate.Date);
            }
        }

        //Sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            if (query.SortBy.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = query.IsAscending
                    ? ordersQuery.OrderBy(o => o.BuyerFirstName).ThenBy(o => o.BuyerLastName)
                    : ordersQuery.OrderByDescending(o => o.BuyerFirstName).ThenByDescending(o => o.BuyerLastName);
            }

            if (query.SortBy.Equals("OrderStatus", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = query.IsAscending ? ordersQuery.OrderBy(o => o.OrderStatus) : ordersQuery.OrderByDescending(o => o.OrderStatus);
            }

            if (query.SortBy.Equals("OrderDate", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = query.IsAscending ? ordersQuery.OrderBy(o => o.OrderDate) : ordersQuery.OrderByDescending(o => o.OrderDate);
            }
        }

        var totalCount = await ordersQuery.CountAsync();

        var orders = await ordersQuery
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        
        return PaginatedList<OrderSummaryDto>.Create(orders, totalCount, query);
    }

    public async Task<Result<OrderDetailsDto>> GetOrderByIdAsync(int id, string userId, bool isStaff)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);

        if (order == null)
            return Result<OrderDetailsDto>.NotFound($"Order with id '{id}' was not found.");

        if (!isStaff && order.UserId != userId)
            return Result<OrderDetailsDto>.Unauthorized("Access denied.");

        return Result<OrderDetailsDto>.Success(order.ToOrderResponseDto());
    }

    public async Task<Result<OrderDetailsDto>> CreateOrderAsync(CreateOrderDto createOrderDto, string userId)
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
                        return Result<OrderDetailsDto>.Failure("Shopping cart is empty.", 400);
                    }

                    decimal subtotalAmount = 0;

                    var productIds = cart.Items.Select(i => i.ProductId).ToList();

                    var categoryIds = cart.Items
                        .Where(i => i.Product is not null)
                        .Select(i => i.Product!.CategoryId)
                        .Distinct()
                        .ToList();

                    // Get shipping address
                    OrderAddress shippingAddress;

                    if (createOrderDto.SavedAddressId.HasValue)
                    {
                        // Option 1: Use saved address
                        var savedAddress = await _unitOfWork.UserAddresses.GetByIdAsync(createOrderDto.SavedAddressId.Value);

                        if (savedAddress == null || savedAddress.UserId != userId)
                        {
                            return Result<OrderDetailsDto>.NotFound($"User address with ID {createOrderDto.SavedAddressId.Value} not found for the current user.");
                        }

                        shippingAddress = OrderAddress.FromUserAddress(savedAddress);
                    }
                    else if (createOrderDto.NewAddress != null)
                    {
                        // Option 2: Use new address provided in the order DTO

                        shippingAddress = createOrderDto.NewAddress.ToOrderAddress();

                        // Optionally save the new address to user's saved addresses
                        if (createOrderDto.SaveAddress)
                        {
                            var newUserAddress = new UserAddress(userId, createOrderDto.NewAddress.Street, createOrderDto.NewAddress.ApartmentNumber,
                                createOrderDto.NewAddress.City, createOrderDto.NewAddress.State, createOrderDto.NewAddress.ZipCode,
                                createOrderDto.NewAddress.Country, createOrderDto.SavedLabel ?? string.Empty, createOrderDto.NewAddress.AdditionalInfo);

                            await _unitOfWork.UserAddresses.AddAsync(newUserAddress);
                        }
                    }
                    else
                    {
                        return Result<OrderDetailsDto>.Failure("Shipping address must be provided.", 400);
                    }

                    var order = new Order(userId, shippingAddress);

                    foreach (var item in cart.Items)
                    {
                        var product = item.Product;

                        if (product is null)
                            return Result<OrderDetailsDto>.NotFound($"Product with ID {item.ProductId} not found.");

                        order.OrderItems.Add(new OrderItem(item.ProductId, product.Name, item.Quantity, product.Price));

                        subtotalAmount += product.Price * item.Quantity;
                        await _productService.UpdateStockAsync(item.ProductId, -item.Quantity);
                    }

                    order.SetAmounts(subtotalAmount);

                    if (!string.IsNullOrWhiteSpace(createOrderDto.PromoCode))
                    {
                        var validateDto = new PromoCodeValidationResults
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
                            var promoCodeDiscount = validationResult.DiscountAmount;
                            order.ApplyPromoCode(createOrderDto.PromoCode, validationResult.PromoCodeId.Value, promoCodeDiscount);
                        }
                        else
                        {
                            return Result<OrderDetailsDto>.BadRequest($"Promo code error: {validationResult.Message}");
                        }
                    }

                    // Bonus points calculation (example: 1 point per $10 spent)
                    if (createOrderDto.RedeemBonusPoints is > 0)
                    {
                        var pointsToRedeem = Math.Min(
                            createOrderDto.RedeemBonusPoints.Value,
                            order.TotalAmount);

                        var bonusResult = await _bonusService.RedeemPointsAsync(userId, 0, pointsToRedeem);

                        if (!bonusResult.IsSuccess)
                            return Result<OrderDetailsDto>.BadRequest(bonusResult.Message);

                        var bonusDiscount = bonusResult.Value; 

                        order.ApplyBonusRedemption(pointsToRedeem, bonusDiscount);
                    }

                    // Save the order 
                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    // Save the promo code usage only after the order is successfully created to avoid recording usage for failed orders
                    if (order.PromoCodeId.HasValue)
                    {
                        await _unitOfWork.PromoCodes.IncrementUsageAsync(order.PromoCodeId.Value);

                        var usage = new PromoCodeUsage(order.PromoCodeId.Value, userId, order.Id, order.PromoCodeDiscountAmount);

                        await _unitOfWork.PromoCodes.RecordUsageAsync(usage);
                    }

                    await _unitOfWork.ShoppingCarts.ClearAsync(cart.Id);

                    // Reward bonus points after order creation
                    var pointsEarned = await _bonusService.EarnPointsAsync(
                        userId, order.Id, order.TotalAmount);

                    order.SetBonusPointsEarned(pointsEarned);

                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    await _orderEmailNotifier.SendOrderConfirmationEmailAsync(order.Id);

                    return Result<OrderDetailsDto>.Success(order.ToOrderResponseDto());
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
                    return Result<OrderDetailsDto>.Failure("Unable to complete the order due to concurrent updates. Please try again.", 409);
                
                // Small delay before retrying to reduce contention
                await Task.Delay(100 * retryCount);

                // Retry
                continue;
            }
        }
        return Result<OrderDetailsDto>.Failure("Order creation failed after multiple retries.", 409);
    }

    public async Task<Result> UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order is null)
            {
                return Result.NotFound($"Order with ID {orderId} not found.");
            }

            if (order.OrderStatus is not OrderStatus.Pending)
            {
                return Result.Conflict($"Only pending orders can be updated.");
            }

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
                    return Result.NotFound($"Product with ID {orderItem.ProductId} not found.");
                }

                order.OrderItems.Add(new OrderItem(orderItem.ProductId, product.Name, orderItem.Quantity, product.Price));

                newSubtotal += orderItem.Quantity * product.Price;
                newProductIds.Add(orderItem.ProductId);
                newCategoryIds.Add(product.CategoryId);
                
                await _productService.UpdateStockAsync(orderItem.ProductId, -orderItem.Quantity);
            }

            order.SetAmounts(newSubtotal);

            // Re-validate promo code against updated order contents
            if (order.PromoCodeId.HasValue && !string.IsNullOrWhiteSpace(order.AppliedPromoCode))
            {
                var validateDto = new PromoCodeValidationResults
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
                    order.ApplyPromoCode(order.AppliedPromoCode, validationResult.PromoCodeId.Value, validationResult.DiscountAmount);
                }
                else
                {
                    // Promo code no longer applicable — remove it from the order
                    await RollbackPromoCodeUsageIfAnyAsync(order);
                    
                    order.RemovePromoCode();
                }
            }

            if (updateOrderDto.ShippingAddress is not null)
            {
                order.UpdateShippingAddress(updateOrderDto.ShippingAddress.ToOrderAddress());
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();
            
            await _orderEmailNotifier.SendOrderCompositionChangeEmailAsync(order.Id);
            
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException) 
        {
            await transaction.RollbackAsync();
            return Result.Conflict("Order was modified by another request. Please retry.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result> CancelOrderAsync(int orderId, string userId, bool isStaff)
    {

        await using var transaction = await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order is null)
                return Result.NotFound($"Order with ID {orderId} not found.");

            if (!isStaff && order.UserId != userId)
                return Result.Unauthorized("Access denied.");

            if (order.OrderStatus != OrderStatus.Pending)
                return Result.Conflict($"Only pending orders can be cancelled.");

            await RestoreStockForOrderItemsAsync(order.OrderItems);
            await RollbackPromoCodeUsageIfAnyAsync(order);

            await _bonusService.ReverseOrderBonusesAsync(order.UserId, order.Id);

            order.ChangeStatus(OrderStatus.Cancelled);
            
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();
            
            await _orderEmailNotifier.SendOrderCancellationEmailAsync(order.Id);
            
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException) 
        {
            await transaction.RollbackAsync();
            return Result.Conflict("Order was modified by another request. Please retry.");
            
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Admin specific methods   
    public async Task<Result> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order is null)
            {
                return Result.NotFound($"Order with ID {orderId} not found.");
            }

            var oldStatus = order.OrderStatus;
            var newStatus = updateOrderStatusDto.Status;
            if (oldStatus == newStatus) 
                return Result.BadRequest($"Order already has status '{newStatus}'.");

            // Restore stock if order is being canceled
            if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                await RestoreStockForOrderItemsAsync(order.OrderItems);
                // Rollback promo code usage
                await RollbackPromoCodeUsageIfAnyAsync(order);
            }
            
            order.ChangeStatus(newStatus);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();

            await _orderEmailNotifier.SendOrderStatusUpdateEmailAsync(order.Id, oldStatus.ToString(), newStatus.ToString());
            
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)  
        {
            await transaction.RollbackAsync();
            return Result.Conflict("Order was modified by another request. Please retry.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    // Specific methods for optimize queries
    private async Task RestoreStockForOrderItemsAsync(IReadOnlyCollection<OrderItem> orderItems)
    {
        if (orderItems.Count == 0)
            return;
        
        foreach (var item in orderItems)
        {
            await _productService.UpdateStockAsync(item.ProductId, item.Quantity);
        }
    }

    private async Task RollbackPromoCodeUsageIfAnyAsync(Order order)
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