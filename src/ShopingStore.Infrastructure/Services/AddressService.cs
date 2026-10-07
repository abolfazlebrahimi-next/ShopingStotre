using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addresses;
    private readonly IUnitOfWork _uow;

    public AddressService(IAddressRepository addresses, IUnitOfWork uow)
    {
        _addresses = addresses;
        _uow = uow;
    }

    public async Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(int userId, CancellationToken ct = default)
    {
        var list = await _addresses.GetUserAddressesAsync(userId, ct);
        return list.Select(a => a.ToDto()).ToList();
    }

    public async Task<AddressDto> SaveAsync(int userId, SaveAddressRequest request, CancellationToken ct = default)
    {
        Address address;

        if (request.Id is > 0)
        {
            address = await _addresses.GetByIdAsync(request.Id.Value, ct) ?? throw new NotFoundException("آدرس مورد نظر یافت نشد.");
            if (address.UserId != userId) throw new ForbiddenException("این آدرس به حساب شما تعلق ندارد.");
        }
        else
        {
            address = new Address { UserId = userId };
            await _addresses.AddAsync(address, ct);
        }

        address.Title = Guard.NotEmpty(request.Title, "عنوان آدرس", 100);
        address.ReceiverName = Guard.NotEmpty(request.ReceiverName, "نام گیرنده", 150);
        address.PhoneNumber = Guard.Mobile(request.PhoneNumber);
        address.Province = Guard.NotEmpty(request.Province, "استان", 100);
        address.City = Guard.NotEmpty(request.City, "شهر", 100);
        address.PostalCode = Guard.NotEmpty(request.PostalCode, "کد پستی", 20);
        address.Line = Guard.NotEmpty(request.Line, "نشانی", 500);

        if (request.IsDefault)
        {
            await _addresses.ClearDefaultAsync(userId, ct);
            address.IsDefault = true;
        }
        else
        {
            var existing = await _addresses.GetUserAddressesAsync(userId, ct);
            if (existing.All(a => a.Id == address.Id || !a.IsDefault) && existing.Count <= 1)
                address.IsDefault = true;
        }

        _addresses.Update(address);
        await _uow.SaveChangesAsync(ct);

        return address.ToDto();
    }

    public async Task DeleteAsync(int userId, int addressId, CancellationToken ct = default)
    {
        var address = await _addresses.GetByIdAsync(addressId, ct) ?? throw new NotFoundException("آدرس مورد نظر یافت نشد.");
        if (address.UserId != userId) throw new ForbiddenException("این آدرس به حساب شما تعلق ندارد.");

        _addresses.Remove(address);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SetDefaultAsync(int userId, int addressId, CancellationToken ct = default)
    {
        var address = await _addresses.GetByIdAsync(addressId, ct) ?? throw new NotFoundException("آدرس مورد نظر یافت نشد.");
        if (address.UserId != userId) throw new ForbiddenException("این آدرس به حساب شما تعلق ندارد.");

        await _addresses.ClearDefaultAsync(userId, ct);
        address.IsDefault = true;
        _addresses.Update(address);
        await _uow.SaveChangesAsync(ct);
    }
}
