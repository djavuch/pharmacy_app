using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.Order;

public record OrderSummaryDto
{
    public int Id { get; init; }
    public DateTime OrderDate { get; init; }
    public decimal TotalAmount { get; init; }
    public OrderStatus OrderStatus { get; init; }
    
    public string BuyerFirstName { get; init; } = string.Empty;
    public string BuyerLastName { get; init; } = string.Empty;
    public string BuyerFullName => $"{BuyerFirstName} {BuyerLastName}";
    
    public int ItemsCount { get; init; }
}