namespace PharmacyApp.Application.DTOs.Address;

public record AddressDetailsDto
{
    public string Street { get; set; }
    public string? ApartmentNumber { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string? AdditionalInfo { get; set; }
}