using PharmacyApp.Application.Contracts.Address;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Contracts.Order;

public record OrderAddressDto : AddressDetailsDto
{
    public override string ToString()
    {
        return $"{Street}, {ApartmentNumber}, {City}, {State} {ZipCode}, {Country}";
    }

    public UserAddress ToUserAddress(string userId, string label) =>
        new(userId, Street, ApartmentNumber, City, State, ZipCode, Country, label, AdditionalInfo);
}