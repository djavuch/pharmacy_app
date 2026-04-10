using PharmacyApp.Application.Contracts.Address;
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
    public static SavedAddressDto ToSavedAddressDto(this UserAddress address)
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
    
    public static OrderAddress ToOrderAddress(this AddressDetailsDto dto) =>
        new(dto.Street, dto.ApartmentNumber, dto.City,
            dto.State, dto.ZipCode, dto.Country, dto.AdditionalInfo);
    
    public static OrderAddress ToOrderAddress(this UserAddress address) =>
        new(address.Street, address.ApartmentNumber, address.City,
            address.State, address.ZipCode, address.Country, address.AdditionalInfo);
}
