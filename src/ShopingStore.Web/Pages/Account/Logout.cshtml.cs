using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly ToastService _toast;

    public LogoutModel(ToastService toast) => _toast = toast;

    public IActionResult OnGet() => Redirect("/");

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync();
        _toast.Info("از حساب کاربری خود خارج شدید.");
        return Redirect("/");
    }
}
