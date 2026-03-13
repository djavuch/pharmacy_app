using PharmacyApp.Application.DTOs.Address;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class UserAddressService : IUserAddressService
{
    private readonly IUnitOfWorkRepository _unitOfWork;

    public UserAddressService(IUnitOfWorkRepository unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SavedAddressDto>> GetUserAddressesAsync(string userId)
    {
        var addresses = await _unitOfWork.UserAddresses.GetByUserIdAsync(userId);

        return addresses.Select(a => a.ToSavedAddressDto()).ToList();
    }

    public async Task<SavedAddressDto?> GetAddressByIdAsync(int id, string userId)
    {
        var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);

        if (address is null || address.UserId != userId)
        {
            return null;
        }

        return address.ToSavedAddressDto(); 
    }

    public async Task<SavedAddressDto> CreateAddressAsync(SaveAddressDto saveAddressDto, string userId)
    {
        var address = new UserAddressModel
        {
            UserId = userId,
            Street = saveAddressDto.Street,
            ApartmentNumber = saveAddressDto.ApartmentNumber,
            City = saveAddressDto.City,
            State = saveAddressDto.State,
            ZipCode = saveAddressDto.ZipCode,
            Country = saveAddressDto.Country,
            AdditionalInfo = saveAddressDto.AdditionalInfo,
            Label = saveAddressDto.Label,
            IsDefault = saveAddressDto.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.UserAddresses.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return address.ToSavedAddressDto();
    }

    public async Task<SavedAddressDto> UpdateAddressAsync(int id, SaveAddressDto saveAddressDto, string userId)
    {
        var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);

        if (address is null)
            throw new KeyNotFoundException($"Address not found");

        if (address.UserId != userId)
            throw new UnauthorizedAccessException();

        address.Street = saveAddressDto.Street;
        address.ApartmentNumber = saveAddressDto.ApartmentNumber;
        address.City = saveAddressDto.City;
        address.State = saveAddressDto.State;
        address.ZipCode = saveAddressDto.ZipCode;
        address.Country = saveAddressDto.Country;
        address.AdditionalInfo = saveAddressDto.AdditionalInfo;
        address.Label = saveAddressDto.Label;
        address.IsDefault = saveAddressDto.IsDefault;

        if (saveAddressDto.IsDefault)
        {
            var others = await _unitOfWork.UserAddresses.GetByUserIdAsync(userId);
            foreach (var other in others.Where(a => a.Id != id && a.IsDefault))
            {
                other.IsDefault = false;
                await _unitOfWork.UserAddresses.UpdateAsync(other);
            }
        }

        await _unitOfWork.UserAddresses.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return saveAddressDto.ToSavedAddressDto(id);
    }

    public async Task DeleteAddressAsync(int id, string userId)
    {
        var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);

        if (address is null)
        {
            throw new KeyNotFoundException($"Address not found");
        }

        if (address.UserId != userId)
        {
            throw new UnauthorizedException("You do not have permission to delete this address.");
        }

        await _unitOfWork.UserAddresses.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetDefaultAddressAsync(int id, string userId)
    {
        var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);

        if (address is null)
        {
            throw new NotFoundException($"Address not found");
        }

        if (address.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        var allAddresses = await _unitOfWork.UserAddresses.GetByUserIdAsync(userId);

        foreach (var addr in allAddresses)
        {
            addr.IsDefault = addr.Id == id;
            await _unitOfWork.UserAddresses.UpdateAsync(addr);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
