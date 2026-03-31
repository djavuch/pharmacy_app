using PharmacyApp.Application.DTOs.Address;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class OrderAddressMappers
{ 
    public static SavedAddressDto ToSavedAddressDto(this SaveAddressDto address, int id)
    {
        return new SavedAddressDto
        {
            Id = id,
            Street = address.Street,
            ApartmentNumber = address.ApartmentNumber,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode,
            Country = address.Country,
            AdditionalInfo = address.AdditionalInfo,
            Label = address.Label,
            IsDefault = address.IsDefault
        };
    }
    public static SavedAddressDto ToSavedAddressDto(this UserAddressModel address)
    {
        return new SavedAddressDto
        {
            Id = address.Id,
            Street = address.Street,
            ApartmentNumber = address.ApartmentNumber,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode,
            Country = address.Country,
            AdditionalInfo = address.AdditionalInfo,
            Label = address.Label,
            IsDefault = address.IsDefault
        };
    }
}
