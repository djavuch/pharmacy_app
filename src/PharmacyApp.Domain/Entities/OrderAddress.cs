namespace PharmacyApp.Domain.Entities;

public class OrderAddress
{
    public int AddressId { get; set; } 
    public string Street { get; private set; } = string.Empty;
    public string? ApartmentNumber { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string? AdditionalInfo { get; private set; }

    private OrderAddress() { }

    public OrderAddress(string street, string? apartment, string city,
        string state, string zipCode, string country, string? additionalInfo = null)
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
        
        Street = street;
        ApartmentNumber = apartment;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        AdditionalInfo = additionalInfo;
    }
    
    public static OrderAddress FromUserAddress(UserAddress address) =>
        new(address.Street, address.ApartmentNumber, address.City,
            address.State, address.ZipCode, address.Country, address.AdditionalInfo);
}
