using PharmacyApp.Application.DTOs.Order;
using PharmacyApp.Application.DTOs.User.UserProfileDto;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static partial class UserMappers
{
    public static UserDto ToUserDto(this UserModel user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        DateOfBirth = user.DateOfBirth,
        PhoneNumber = user.PhoneNumber
    };

    public static UserOrdersDto ToUserOrdersDto(this OrderModel order)
    {
        return new UserOrdersDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            TotalAmount = order.TotalAmount,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                OrderId = oi.OrderId,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };
    }

    public static UserReviewsDto ToUserReviewsDto(this ReviewModel review)
    {
        return new UserReviewsDto
        {
            ReviewId = review.Id,
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            Rating = review.Rating,
            Content = review.Content,
            ReviewDate = review.CreatedAt,
            IsApproved = review.IsApproved
        };
    }
}
