using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Discounts;

public class IndexModel : PageModel
{
    private readonly IDiscountCodeService _discounts;
    private readonly ToastService _toast;

    public IndexModel(IDiscountCodeService discounts, ToastService toast)
    {
        _discounts = discounts;
        _toast = toast;
    }

    public IReadOnlyList<DiscountCodeDto> Codes { get; private set; } = Array.Empty<DiscountCodeDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Codes = await _discounts.GetAllAsync(cancellationToken);

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _discounts.DeleteAsync(id, cancellationToken);
            _toast.Success("کد تخفیف حذف شد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }
}
