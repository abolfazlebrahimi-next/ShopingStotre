using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Checkout;

public class IndexModel : PageModel
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orders;
    private readonly IAddressService _addresses;
    private readonly ISettingsService _settings;
    private readonly ICurrentUser _currentUser;
    private readonly CartIdentity _cartIdentity;
    private readonly ToastService _toast;

    public IndexModel(
        ICartService cartService,
        IOrderService orders,
        IAddressService addresses,
        ISettingsService settings,
        ICurrentUser currentUser,
        CartIdentity cartIdentity,
        ToastService toast)
    {
        _cartService = cartService;
        _orders = orders;
        _addresses = addresses;
        _settings = settings;
        _currentUser = currentUser;
        _cartIdentity = cartIdentity;
        _toast = toast;
    }

    public CartDto Cart { get; private set; } = new();
    public IReadOnlyList<AddressDto> Addresses { get; private set; } = Array.Empty<AddressDto>();
    public StoreSettingsDto Settings { get; private set; } = new();
    public CheckoutRequest Request { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Redirect("/account/login?returnUrl=/checkout");

        await LoadAsync(cancellationToken);

        if (Cart.IsEmpty)
        {
            _toast.Error("برای تکمیل خرید، ابتدا محصولی به سبد اضافه کنید.");
            return RedirectToPage("/Cart/Index");
        }

        var defaultAddress = Addresses.FirstOrDefault(a => a.IsDefault) ?? Addresses.FirstOrDefault();
        Request = new CheckoutRequest
        {
            CustomerName = _currentUser.FullName ?? string.Empty,
            CustomerPhone = User.FindFirst(System.Security.Claims.ClaimTypes.MobilePhone)?.Value ?? string.Empty,
            CustomerEmail = _currentUser.Email
        };

        if (defaultAddress is not null)
        {
            Request.AddressId = defaultAddress.Id;
            Request.Province = defaultAddress.Province;
            Request.City = defaultAddress.City;
            Request.PostalCode = defaultAddress.PostalCode;
            Request.AddressLine = defaultAddress.Line;
            Request.CustomerName = defaultAddress.ReceiverName;
            Request.CustomerPhone = defaultAddress.PhoneNumber;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CheckoutRequest request,
        int? AddressId,
        bool UseNewAddress,
        bool SaveAddress,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Redirect("/account/login?returnUrl=/checkout");

        var userId = _currentUser.UserId.Value;

        // اگر آدرس ذخیره‌شده انتخاب شده باشد، اطلاعات از همان خوانده می‌شود
        if (AddressId is > 0 && !UseNewAddress)
        {
            var saved = (await _addresses.GetUserAddressesAsync(userId, cancellationToken)).FirstOrDefault(a => a.Id == AddressId.Value);
            if (saved is not null)
            {
                request.Province = saved.Province;
                request.City = saved.City;
                request.PostalCode = saved.PostalCode;
                request.AddressLine = saved.Line;
                request.CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? saved.ReceiverName : request.CustomerName;
                request.CustomerPhone = string.IsNullOrWhiteSpace(request.CustomerPhone) ? saved.PhoneNumber : request.CustomerPhone;
            }
        }
        else if (SaveAddress && !string.IsNullOrWhiteSpace(request.AddressLine))
        {
            try
            {
                await _addresses.SaveAsync(userId, new SaveAddressRequest
                {
                    Title = "آدرس سفارش",
                    ReceiverName = request.CustomerName,
                    PhoneNumber = request.CustomerPhone,
                    Province = request.Province,
                    City = request.City,
                    PostalCode = request.PostalCode,
                    Line = request.AddressLine
                }, cancellationToken);
            }
            catch (BusinessException)
            {
                // ذخیره آدرس اختیاری است و مانع ثبت سفارش نمی‌شود
            }
        }

        OrderDto order;
        try
        {
            order = await _orders.CheckoutAsync(_cartIdentity.CartKey, userId, request, cancellationToken);
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }

        if (request.PaymentMethod == PaymentMethod.CashOnDelivery)
        {
            _toast.Success("سفارش شما با موفقیت ثبت شد. پرداخت در محل انجام می‌شود.");
            return Redirect($"/order/success/{order.OrderNumber}");
        }

        return Redirect($"/payment/gateway?order={order.OrderNumber}");
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Cart = await _cartService.GetCartAsync(_cartIdentity.CartKey, _currentUser.UserId, cancellationToken);
        Settings = await _settings.GetAsync(cancellationToken);

        if (_currentUser.UserId is not null)
            Addresses = await _addresses.GetUserAddressesAsync(_currentUser.UserId.Value, cancellationToken);
    }
}
