using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Application.Contracts.PromoCode;
using PharmacyApp.Application.Contracts.PromoCode.Results;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Domain.Enums;
using System.Data;

namespace PharmacyApp.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly IOrderEmailNotifier _orderEmailNotifier;
    private readonly IPromoCodeService _promoCodeService;
    private readonly IProductService _productService;
    private readonly IBonusService _bonusService;
    private readonly IDiscountService _discountService;

    public OrderService(
        IUnitOfWorkRepository unitOfWork,
        IOrderEmailNotifier orderEmailNotifier,
        IPromoCodeService promoCodeService,
        IProductService productService,
        IBonusService bonusService,
        IDiscountService discountService)
    {
        _unitOfWork = unitOfWork;
        _orderEmailNotifier = orderEmailNotifier;
        _promoCodeService = promoCodeService;
        _productService = productService;
        _bonusService = bonusService;
        _discountService = discountService;
    }

    public async Task<PaginatedList<OrderSummaryDto>> GetAllOrdersAsync(QueryParams query)
    {
        var ordersQuery = _unitOfWork.Orders.GetAllAsync();

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

            if (query.FilterOn.Equals("OrderDate", StringComparison.OrdinalIgnoreCase) &&
                DateTime.TryParse(query.FilterQuery, out var orderDate))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date == orderDate.Date);
            }
        }

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
                ordersQuery = query.IsAscending
                    ? ordersQuery.OrderBy(o => o.OrderStatus)
                    : ordersQuery.OrderByDescending(o => o.OrderStatus);
            }

            if (query.SortBy.Equals("OrderDate", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = query.IsAscending
                    ? ordersQuery.OrderBy(o => o.OrderDate)
                    : ordersQuery.OrderByDescending(o => o.OrderDate);
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
            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                try
                {
                    var cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(userId);

                    if (cart is null || !cart.Items.Any())
                    {
                        return Result<OrderDetailsDto>.BadRequest("Shopping cart is empty.");
                    }

                    decimal subtotalAmount = 0;

                    var productIds = cart.Items.Select(i => i.ProductId).ToList();

                    var categoryIds = cart.Items
                        .Where(i => i.Product is not null)
                        .Select(i => i.Product!.CategoryId)
                        .Distinct()
                        .ToList();

                    OrderAddress shippingAddress;

                    if (createOrderDto.SavedAddressId.HasValue)
                    {
                        var savedAddress = await _unitOfWork.UserAddresses.GetByIdAsync(createOrderDto.SavedAddressId.Value);

                        if (savedAddress == null || savedAddress.UserId != userId)
                        {
                            return Result<OrderDetailsDto>.NotFound(
                                $"User address with ID {createOrderDto.SavedAddressId.Value} not found for the current user.");
                        }

                        shippingAddress = OrderAddress.FromUserAddress(savedAddress);
                    }
                    else if (createOrderDto.NewAddress != null)
                    {
                        shippingAddress = createOrderDto.NewAddress.ToOrderAddress();

                        if (createOrderDto.SaveAddress)
                        {
                            var newUserAddress = new UserAddress(
                                userId,
                                createOrderDto.NewAddress.Street,
                                createOrderDto.NewAddress.ApartmentNumber,
                                createOrderDto.NewAddress.City,
                                createOrderDto.NewAddress.State,
                                createOrderDto.NewAddress.ZipCode,
                                createOrderDto.NewAddress.Country,
                                createOrderDto.SavedLabel ?? string.Empty,
                                createOrderDto.NewAddress.AdditionalInfo);

                            await _unitOfWork.UserAddresses.AddAsync(newUserAddress);
                        }
                    }
                    else
                    {
                        return Result<OrderDetailsDto>.BadRequest("Shipping address must be provided.");
                    }

                    var order = new Order(userId, shippingAddress);

                    var discountedPrices = await BuildDiscountedPriceMapAsync(
                        cart.Items
                            .Where(item => item.Product is not null)
                            .Select(item => item.Product!)
                            .DistinctBy(product => product.Id));

                    foreach (var item in cart.Items)
                    {
                        var product = item.Product;

                        if (product is null)
                            return Result<OrderDetailsDto>.NotFound($"Product with ID {item.ProductId} not found.");

                        var effectivePrice = discountedPrices.GetValueOrDefault(product.Id, product.Price);

                        order.OrderItems.Add(new OrderItem(item.ProductId, product.Name, item.Quantity, effectivePrice));

                        subtotalAmount += effectivePrice * item.Quantity;
                        var decreaseStockResult = await _productService.UpdateStockAsync(item.ProductId, -item.Quantity);

                        if (!decreaseStockResult.IsSuccess)
                        {
                            return Result<OrderDetailsDto>.Failure(
                                $"Failed to reserve stock for product with ID {item.ProductId}: {decreaseStockResult.Message}",
                                decreaseStockResult.ErrorType);
                        }
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

                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    if (createOrderDto.RedeemBonusPoints is > 0)
                    {
                        var pointsToRedeem = Math.Min(
                            createOrderDto.RedeemBonusPoints.Value,
                            order.TotalAmount);

                        var bonusResult = await _bonusService.RedeemPointsAsync(userId, order.Id, pointsToRedeem);

                        if (!bonusResult.IsSuccess)
                            return Result<OrderDetailsDto>.BadRequest(bonusResult.Message);

                        var bonusDiscount = bonusResult.Value;
                        order.ApplyBonusRedemption(pointsToRedeem, bonusDiscount);
                    }

                    if (order.PromoCodeId.HasValue)
                    {
                        await _unitOfWork.PromoCodes.IncrementUsageAsync(order.PromoCodeId.Value);

                        var usage = new PromoCodeUsage(order.PromoCodeId.Value, userId, order.Id, order.PromoCodeDiscountAmount);

                        await _unitOfWork.PromoCodes.RecordUsageAsync(usage);
                    }

                    await _unitOfWork.ShoppingCarts.ClearAsync(cart.Id);

                    var pointsEarned = await _bonusService.EarnPointsAsync(userId, order.Id, order.TotalAmount);

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
                    return Result<OrderDetailsDto>.Conflict("Unable to complete the order due to concurrent updates. Please try again.");

                await Task.Delay(100 * retryCount);
                continue;
            }
        }

        return Result<OrderDetailsDto>.Conflict("Order creation failed after multiple retries.");
    }

    public async Task<Result> UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

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
                var restoreStockResult = await _productService.UpdateStockAsync(existingItem.ProductId, existingItem.Quantity);

                if (!restoreStockResult.IsSuccess)
                {
                    return Result.Failure(
                        $"Failed to restore stock for product with ID {existingItem.ProductId}: {restoreStockResult.Message}",
                        restoreStockResult.ErrorType);
                }
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
            var discountedPrices = await BuildDiscountedPriceMapAsync(requestedProducts);

            foreach (var orderItem in updateOrderDto.OrderItems)
            {
                if (!requestedProductMap.TryGetValue(orderItem.ProductId, out var product))
                {
                    return Result.NotFound($"Product with ID {orderItem.ProductId} not found.");
                }

                var effectivePrice = discountedPrices.GetValueOrDefault(product.Id, product.Price);

                order.OrderItems.Add(new OrderItem(orderItem.ProductId, product.Name, orderItem.Quantity, effectivePrice));

                newSubtotal += orderItem.Quantity * effectivePrice;
                newProductIds.Add(orderItem.ProductId);
                newCategoryIds.Add(product.CategoryId);

                var decreaseStockResult = await _productService.UpdateStockAsync(orderItem.ProductId, -orderItem.Quantity);

                if (!decreaseStockResult.IsSuccess)
                {
                    return Result.Failure(
                        $"Failed to reserve stock for product with ID {orderItem.ProductId}: {decreaseStockResult.Message}",
                        decreaseStockResult.ErrorType);
                }
            }

            order.SetAmounts(newSubtotal);

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
        await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order is null)
                return Result.NotFound($"Order with ID {orderId} not found.");

            if (!isStaff && order.UserId != userId)
                return Result.Unauthorized("Access denied.");

            if (order.OrderStatus != OrderStatus.Pending)
                return Result.Conflict($"Only pending orders can be cancelled.");

            var restoreStockResult = await RestoreStockForOrderItemsAsync(order.OrderItems);

            if (!restoreStockResult.IsSuccess)
            {
                return restoreStockResult;
            }

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

            if (oldStatus == OrderStatus.Cancelled)
                return Result.Conflict("Cancelled orders are final and their status cannot be changed.");

            if (oldStatus == newStatus)
                return Result.BadRequest($"Order already has status '{newStatus}'.");

            if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                var restoreStockResult = await RestoreStockForOrderItemsAsync(order.OrderItems);

                if (!restoreStockResult.IsSuccess)
                {
                    return restoreStockResult;
                }

                await RollbackPromoCodeUsageIfAnyAsync(order);
                await _bonusService.ReverseOrderBonusesAsync(order.UserId, order.Id);
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

    private async Task<Result> RestoreStockForOrderItemsAsync(IReadOnlyCollection<OrderItem> orderItems)
    {
        if (orderItems.Count == 0)
            return Result.Success();

        foreach (var item in orderItems)
        {
            var restoreStockResult = await _productService.UpdateStockAsync(item.ProductId, item.Quantity);

            if (!restoreStockResult.IsSuccess)
            {
                return Result.Failure(
                    $"Failed to restore stock for product with ID {item.ProductId}: {restoreStockResult.Message}",
                    restoreStockResult.ErrorType);
            }
        }

        return Result.Success();
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

    private async Task<Dictionary<int, decimal>> BuildDiscountedPriceMapAsync(IEnumerable<Product> products)
    {
        var priceContexts = products
            .Select(product => new ProductPriceContext(product.Id, product.CategoryId, product.Price))
            .DistinctBy(product => product.ProductId)
            .ToList();

        if (priceContexts.Count == 0)
            return [];

        return await _discountService.CalculateDiscountedPricesAsync(priceContexts);
    }
}
