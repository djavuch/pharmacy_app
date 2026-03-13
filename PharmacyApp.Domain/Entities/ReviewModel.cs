namespace PharmacyApp.Domain.Entities;

public class ReviewModel
{
    public int Id { get; set; } 
    public string? UserId { get; set; }
    public int ProductId { get; set; }
    public string? Content { get; set; }
    public int Rating { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    public ProductModel? Product { get; set; } 
    public UserModel? User { get; set; }
    public bool IsApproved { get; set; } = false;
}
