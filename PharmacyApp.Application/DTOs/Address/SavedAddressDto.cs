namespace PharmacyApp.Application.DTOs.Address;

public class SavedAddressDto : AddressDetailsDto
{
    public int Id { get; set; }
    public string Label { get; set; }
    public bool IsDefault { get; set; }
}