namespace PharmacyApp.Domain.Entities;

public class UserAddress
{
    public int Id { get; private set; }
    public string UserId { get; private set; } 
    public User User { get; init; }

    public string Street { get; private set; } = string.Empty;
    public string? ApartmentNumber { get; private set; }
    public string City { get; private set; }  = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string? AdditionalInfo { get; private set; }

    public string Label { get; private set; }
    public bool IsDefault { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    
    private UserAddress() { }

    public UserAddress(string userId, string street, string? apartmentNumber,
        string city, string state, string zipCode, string country,
        string label, string? additionalInfo = null, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.");
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.");
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.");
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code cannot be empty.");
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.");
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Address label cannot be empty.");

        UserId = userId;
        Street = street;
        ApartmentNumber = apartmentNumber;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        Label = label;
        AdditionalInfo = additionalInfo;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void Update(string street, string? apartmentNumber,
        string city, string state, string zipCode, string country,
        string label, string? additionalInfo = null, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.");
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.");
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code cannot be empty.");
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.");
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Address label cannot be empty.");

        Street = street;
        ApartmentNumber = apartmentNumber;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        Label = label;
        AdditionalInfo = additionalInfo;
        IsDefault = isDefault;
    }
    
    public void SetAsDefault() => IsDefault = true;
    public void UnsetDefault() => IsDefault = false;
    public void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
}
