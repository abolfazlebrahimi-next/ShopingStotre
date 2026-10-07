using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _auth;
    private readonly ICartService _cartService;
    private readonly CartIdentity _cartIdentity;
    private readonly ToastService _toast;

    public RegisterModel(IAuthService auth, ICartService cartService, CartIdentity cartIdentity, ToastService toast)
    {
        _auth = auth;
        _cartService = cartService;
        _cartIdentity = cartIdentity;
        _toast = toast;
    }

    public string? FullName { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(
        string fullName,
        string email,
        string phoneNumber,
        string password,
        string confirmPassword,
        bool acceptTerms,
        CancellationToken cancellationToken)
    {
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;

        try
        {
            var user = await _auth.RegisterAsync(new RegisterRequest
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Password = password,
                ConfirmPassword = confirmPassword,
                AcceptTerms = acceptTerms
            }, cancellationToken);

            var cartKey = _cartIdentity.CartKey;
            await HttpContext.SignInAsync(user);
            await _cartService.MoveGuestCartToUserAsync(cartKey, user.Id, cancellationToken);

            _toast.Success("ثبت‌نام شما با موفقیت انجام شد. خوش آمدید!");
            return RedirectToPage("/Account/Index");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
            return Page();
        }
    }
}
