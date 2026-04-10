using PharmacyApp.Application.Contracts.Address;

namespace PharmacyApp.Application.Contracts.Order;

public record OrderAddressDto : AddressDetailsDto
{
    public override string ToString()
    {
        return $"{Street}, {ApartmentNumber}, {City}, {State} {ZipCode}, {Country}";
    }
}