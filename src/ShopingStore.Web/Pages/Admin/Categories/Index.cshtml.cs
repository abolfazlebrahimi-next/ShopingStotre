using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Categories;

public class IndexModel : PageModel
{
    private readonly IAdminCategoryService _categories;
    private readonly ToastService _toast;

    public IndexModel(IAdminCategoryService categories, ToastService toast)
    {
        _categories = categories;
        _toast = toast;
    }

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Categories = await _categories.GetAllAsync(cancellationToken);

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _categories.DeleteAsync(id, cancellationToken);
            _toast.Success("دسته‌بندی حذف شد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }
}
