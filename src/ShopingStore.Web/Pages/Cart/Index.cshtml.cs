using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Cart;

public class IndexModel : PageModel
{
    private readonly ICartService _cartService;
    private readonly ICurrentUser _currentUser;
    private readonly CartIdentity _cartIdentity;
    private readonly ToastService _toast;

    public IndexModel(ICartService cartService, ICurrentUser currentUser, CartIdentity cartIdentity, ToastService toast)
    {
        _cartService = cartService;
        _currentUser = currentUser;
        _cartIdentity = cartIdentity;
        _toast = toast;
    }

    public CartDto Cart { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Cart = await _cartService.GetCartAsync(_cartIdentity.CartKey, _currentUser.UserId, cancellationToken);

    /// <summary>افزودن محصول به سبد (درخواست AJAX از کارت محصول).</summary>
    public async Task<IActionResult> OnPostAddAsync(int productId, int quantity = 1, string? variant = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await _cartService.AddItemAsync(_cartIdentity.CartKey, _currentUser.UserId, productId, quantity, variant, cancellationToken);

            return new JsonResult(new
            {
                success = true,
                message = "محصول به سبد خرید اضافه شد.",
                cartCount = PersianDate.ToPersianDigits(cart.TotalQuantity.ToString()),
                total = PersianDate.ToPrice(cart.Total)
            });
        }
        catch (BusinessException exception)
        {
            return new JsonResult(new { success = false, message = exception.Message });
        }
        catch (NotFoundException exception)
        {
            return new JsonResult(new { success = false, message = exception.Message });
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(int cartItemId, int quantity, CancellationToken cancellationToken)
    {
        await _cartService.UpdateQuantityAsync(_cartIdentity.CartKey, _currentUser.UserId, cartItemId, quantity, cancellationToken);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(int cartItemId, CancellationToken cancellationToken)
    {
        await _cartService.RemoveItemAsync(_cartIdentity.CartKey, _currentUser.UserId, cartItemId, cancellationToken);
        _toast.Success("محصول از سبد خرید حذف شد.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApplyDiscountAsync(string code, CancellationToken cancellationToken)
    {
        var (success, message, _) = await _cartService.ApplyDiscountAsync(_cartIdentity.CartKey, _currentUser.UserId, code, cancellationToken);

        if (success) _toast.Success(message ?? "کد تخفیف اعمال شد.");
        else _toast.Error(message ?? "کد تخفیف معتبر نیست.");

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveDiscountAsync(CancellationToken cancellationToken)
    {
        await _cartService.RemoveDiscountAsync(_cartIdentity.CartKey, _currentUser.UserId, cancellationToken);
        _toast.Info("کد تخفیف حذف شد.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearAsync(CancellationToken cancellationToken)
    {
        await _cartService.ClearAsync(_cartIdentity.CartKey, _currentUser.UserId, cancellationToken);
        _toast.Info("سبد خرید خالی شد.");
        return RedirectToPage();
    }
}
