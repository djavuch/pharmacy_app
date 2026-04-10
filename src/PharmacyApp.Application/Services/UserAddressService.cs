using PharmacyApp.Application.Contracts.Address;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities;

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
        var address = new UserAddress(userId, saveAddressDto.Street, saveAddressDto.ApartmentNumber, 
            saveAddressDto.City, saveAddressDto.State,  saveAddressDto.ZipCode, saveAddressDto.Country, 
            saveAddressDto.AdditionalInfo, saveAddressDto.Label, saveAddressDto.IsDefault);

        await _unitOfWork.UserAddresses.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return address.ToSavedAddressDto();
    }

    public async Task<Result<SavedAddressDto>> UpdateAddressAsync(int id, SaveAddressDto saveAddressDto, string userId)
    {
        var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);

        if (address is null)
            return Result<SavedAddressDto>.NotFound("Address not found");

        if (address.UserId != userId)
            return Result<SavedAddressDto>.Forbidden("You do not have permission to modify this address.");

        address.Update(
            saveAddressDto.Street, saveAddressDto.ApartmentNumber,
            saveAddressDto.City, saveAddressDto.State, saveAddressDto.ZipCode,
            saveAddressDto.Country,  saveAddressDto.AdditionalInfo, 
            saveAddressDto.Label, saveAddressDto.IsDefault);

        if (saveAddressDto.IsDefault)
        {
            var others = await _unitOfWork.UserAddresses.GetByUserIdAsync(userId);
            foreach (var other in others.Where(a => a.Id != id && a.IsDefault))
            {
                other.UnsetDefault();
                await _unitOfWork.UserAddresses.UpdateAsync(other);
            }
        }

        await _unitOfWork.UserAddresses.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return Result<SavedAddressDto>.Success(saveAddressDto.ToSavedAddressDto(id));
    }

    public async Task<Result> DeleteAddressAsync(int id, string userId)
    {
        var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);

        if (address is null)
            return Result.NotFound($"Address not found");

        if (address.UserId != userId)
            return Result.Forbidden("You do not have permission to delete this address.");
        
        await _unitOfWork.UserAddresses.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        
        return  Result.Success();
    }

    public async Task<Result> SetDefaultAddressAsync(int id, string userId)
    {
        var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);

        if (address is null)
            return Result.NotFound("Address not found");

        if (address.UserId != userId)
            return Result.Forbidden("You do not have permission to modify this address.");

        await _unitOfWork.UserAddresses.SetDefaultAsync(id, userId); 
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
