using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Discounts;

public class EditModel : PageModel
{
    private readonly IDiscountCodeService _discounts;
    private readonly ToastService _toast;

    public EditModel(IDiscountCodeService discounts, ToastService toast)
    {
        _discounts = discounts;
        _toast = toast;
    }

    public DiscountCode? Code { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is > 0)
        {
            Code = await _discounts.GetForEditAsync(id.Value, cancellationToken);
            if (Code is null) return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int? id,
        string code,
        DiscountType type,
        decimal amount,
        decimal minOrderAmount,
        decimal? maxDiscountAmount,
        int? usageLimit,
        string? description,
        DateTime? startsAt,
        DateTime? expiresAt,
        bool isActive,
        CancellationToken cancellationToken)
    {
        try
        {
            var codeId = await _discounts.SaveAsync(new SaveDiscountCodeRequest
            {
                Id = id,
                Code = code,
                Type = type,
                Amount = amount,
                MinOrderAmount = minOrderAmount,
                MaxDiscountAmount = maxDiscountAmount is > 0 ? maxDiscountAmount : null,
                UsageLimit = usageLimit is > 0 ? usageLimit : null,
                Description = description,
                StartsAtUtc = startsAt,
                ExpiresAtUtc = expiresAt,
                IsActive = isActive
            }, cancellationToken);

            _toast.Success("کد تخفیف ذخیره شد.");
            return Redirect($"/admin/discounts/edit/{codeId}");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
            Code = id is > 0 ? await _discounts.GetForEditAsync(id.Value, cancellationToken) : null;
            return Page();
        }
    }
}
