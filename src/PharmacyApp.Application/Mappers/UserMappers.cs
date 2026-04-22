using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Application.Contracts.User.Admin;
using PharmacyApp.Application.Contracts.User.Profile;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Mappers;

public static partial class UserMappers
{
    public static UserProfileDto ToUserDto(this User user, string? role = null) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        Role = role,
        FirstName = user.FirstName ?? string.Empty,
        LastName = user.LastName ?? string.Empty,
        DateOfBirth = user.DateOfBirth,
        PhoneNumber = user.PhoneNumber,
        CreatedAt = user.CreatedAt
    };
    
    public static AdminUserDto ToAdminUserDto(this User user, string? role) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName ?? string.Empty,
        LastName = user.LastName ?? string.Empty,
        DateOfBirth = user.DateOfBirth,
        PhoneNumber = user.PhoneNumber,
        CreatedAt = user.CreatedAt,
        Role = role,
        IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
        LockoutEnd = user.LockoutEnd,
        AccessFailedCount = user.AccessFailedCount
    };

    public static UserOrderSummaryDto ToUserOrdersDto(this Order order)
    {
        return new UserOrderSummaryDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress?.ToOrderAddressDto(),
            AppliedPromoCode = order.AppliedPromoCode,
            PromoCodeDiscountAmount = order.PromoCodeDiscountAmount,
            BonusPointsRedeemed = order.BonusPointsRedeemed,
            BonusPointsEarned = order.BonusPointsEarned,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName ?? string.Empty,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Subtotal = oi.Subtotal
            }).ToList()
        };
    }

    public static UserReviewSummaryDto ToUserReviewsDto(this Review review)
    {
        return new UserReviewSummaryDto
        {
            ReviewId = review.Id,
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            Rating = review.Rating,
            Content = review.Content,
            ReviewDate = review.CreatedAt,
            IsApproved = review.Status == ReviewStatus.Approved
        };
    }
}
