using PharmacyApp.Application.DTOs.Address;

namespace PharmacyApp.Application.DTOs.Order;

public class OrderAddressDto : AddressDetailsDto
{
    public override string ToString()
    {
        return $"{Street}, {ApartmentNumber}, {City}, {State} {ZipCode}, {Country}";
    }
}