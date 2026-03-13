namespace PharmacyApp.Application.DTOs.Address;

public class SaveAddressDto : AddressDetailsDto
{
    public string Label { get; set; }
    public bool IsDefault { get; set; }
}