namespace PharmacyApp.Domain.Entities;

public class UserAddressModel
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public UserModel User { get; set; }

    public string Street { get; set; }
    public string? ApartmentNumber { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string? AdditionalInfo { get; set; }

    public string Label { get; set; }
    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
