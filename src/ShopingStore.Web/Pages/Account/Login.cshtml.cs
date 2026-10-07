using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _auth;
    private readonly ICartService _cartService;
    private readonly CartIdentity _cartIdentity;
    private readonly ToastService _toast;

    public LoginModel(IAuthService auth, ICartService cartService, CartIdentity cartIdentity, ToastService toast)
    {
        _auth = auth;
        _cartService = cartService;
        _cartIdentity = cartIdentity;
        _toast = toast;
    }

    public string? Email { get; private set; }
    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl = null)
    {
        Email = string.Empty;
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string email, string password, bool rememberMe, string? returnUrl, CancellationToken cancellationToken)
    {
        Email = email;
        ReturnUrl = returnUrl;

        try
        {
            var user = await _auth.LoginAsync(new LoginRequest { Email = email, Password = password, RememberMe = rememberMe }, cancellationToken);

            var cartKey = _cartIdentity.CartKey;
            await HttpContext.SignInAsync(user, rememberMe);
            await _cartService.MoveGuestCartToUserAsync(cartKey, user.Id, cancellationToken);

            _toast.Success($"{user.FullName} عزیز، خوش آمدید!");

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return user.CanManageStore ? Redirect("/admin") : RedirectToPage("/Account/Index");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
            return Page();
        }
    }
}
