namespace PharmacyApp.Application.Contracts.Address;

public record SavedAddressDto : AddressDetailsDto
{
    public int Id { get; set; }
    public string Label { get; set; }
    public bool IsDefault { get; set; }
}