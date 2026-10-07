using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class AddressesModel : PageModel
{
    private readonly IAddressService _addresses;
    private readonly ICurrentUser _currentUser;
    private readonly ToastService _toast;

    public AddressesModel(IAddressService addresses, ICurrentUser currentUser, ToastService toast)
    {
        _addresses = addresses;
        _currentUser = currentUser;
        _toast = toast;
    }

    public IReadOnlyList<AddressDto> Addresses { get; private set; } = Array.Empty<AddressDto>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login?returnUrl=/account/addresses");

        Addresses = await _addresses.GetUserAddressesAsync(_currentUser.UserId.Value, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(
        string title,
        string receiverName,
        string phoneNumber,
        string province,
        string city,
        string postalCode,
        string line,
        bool isDefault,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login");

        try
        {
            await _addresses.SaveAsync(_currentUser.UserId.Value, new SaveAddressRequest
            {
                Title = title,
                ReceiverName = receiverName,
                PhoneNumber = phoneNumber,
                Province = province,
                City = city,
                PostalCode = postalCode,
                Line = line,
                IsDefault = isDefault
            }, cancellationToken);

            _toast.Success("آدرس جدید ذخیره شد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int addressId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login");

        try
        {
            await _addresses.DeleteAsync(_currentUser.UserId.Value, addressId, cancellationToken);
            _toast.Success("آدرس حذف شد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetDefaultAsync(int addressId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login");

        await _addresses.SetDefaultAsync(_currentUser.UserId.Value, addressId, cancellationToken);
        _toast.Success("آدرس پیش‌فرض تغییر کرد.");
        return RedirectToPage();
    }
}
