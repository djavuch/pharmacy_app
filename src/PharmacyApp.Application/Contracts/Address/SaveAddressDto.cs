namespace PharmacyApp.Application.Contracts.Address;

public record SaveAddressDto : AddressDetailsDto
{
    public string Label { get; set; }
    public bool IsDefault { get; set; }
}