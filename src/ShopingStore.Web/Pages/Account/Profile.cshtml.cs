using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly IAuthService _auth;
    private readonly ICurrentUser _currentUser;
    private readonly ToastService _toast;

    public ProfileModel(IAuthService auth, ICurrentUser currentUser, ToastService toast)
    {
        _auth = auth;
        _currentUser = currentUser;
        _toast = toast;
    }

    public UserProfileDto? Profile { get; private set; }

    private async Task<IActionResult?> EnsureUserAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login?returnUrl=/account/profile");

        Profile = await _auth.GetProfileAsync(_currentUser.UserId.Value, cancellationToken);
        return null;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var redirect = await EnsureUserAsync(cancellationToken);
        return redirect ?? Page();
    }

    public async Task<IActionResult> OnPostAsync(string fullName, string? phoneNumber, string? nationalCode, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login");

        try
        {
            await _auth.UpdateProfileAsync(_currentUser.UserId.Value, fullName, phoneNumber, nationalCode, cancellationToken);
            _toast.Success("اطلاعات حساب شما به‌روزرسانی شد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }
}
