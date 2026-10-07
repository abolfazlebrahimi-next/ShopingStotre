using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Settings;

public class IndexModel : PageModel
{
    private readonly ISettingsService _settings;
    private readonly ToastService _toast;

    public IndexModel(ISettingsService settings, ToastService toast)
    {
        _settings = settings;
        _toast = toast;
    }

    public StoreSettingsDto Settings { get; private set; } = new();

    [BindProperty]
    public StoreSettingsDto Input { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Settings = await _settings.GetAsync(cancellationToken);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Input.SiteName))
                throw new BusinessException("نام فروشگاه را وارد کنید.");

            if (Input.ShippingCost < 0 || Input.FreeShippingThreshold < 0 || Input.TaxPercent < 0)
                throw new BusinessException("مقادیر مالی نمی‌توانند منفی باشند.");

            await _settings.SaveAsync(Input, cancellationToken);
            _toast.Success("تنظیمات فروشگاه ذخیره شد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }
}
