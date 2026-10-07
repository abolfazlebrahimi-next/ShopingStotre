using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class ChangePasswordModel : PageModel
{
    private readonly IAuthService _auth;
    private readonly ICurrentUser _currentUser;
    private readonly ToastService _toast;

    public ChangePasswordModel(IAuthService auth, ICurrentUser currentUser, ToastService toast)
    {
        _auth = auth;
        _currentUser = currentUser;
        _toast = toast;
    }

    public IActionResult OnGet()
        => _currentUser.UserId is null ? Redirect("/account/login?returnUrl=/account/change-password") : Page();

    public async Task<IActionResult> OnPostAsync(string currentPassword, string newPassword, string confirmPassword, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login");

        try
        {
            await _auth.ChangePasswordAsync(_currentUser.UserId.Value, new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            }, cancellationToken);

            _toast.Success("رمز عبور شما با موفقیت تغییر کرد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }
}
