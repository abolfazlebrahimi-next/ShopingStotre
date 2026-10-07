using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Categories;

public class EditModel : PageModel
{
    private readonly IAdminCategoryService _categories;
    private readonly ToastService _toast;

    public EditModel(IAdminCategoryService categories, ToastService toast)
    {
        _categories = categories;
        _toast = toast;
    }

    public Category? Category { get; private set; }
    public IReadOnlyList<Category> Parents { get; private set; } = Array.Empty<Category>();

    private async Task LoadAsync(int? id, CancellationToken cancellationToken)
    {
        var all = await _categories.GetAllAsync(cancellationToken);
        Parents = all.Where(c => c.ParentId is null && c.Id != id).ToList();

        if (id is > 0) Category = await _categories.GetForEditAsync(id.Value, cancellationToken);
    }

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken cancellationToken)
    {
        await LoadAsync(id, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int? id,
        string name,
        string? slug,
        string? description,
        string? icon,
        int? parentId,
        int sortOrder,
        bool isActive,
        CancellationToken cancellationToken)
    {
        try
        {
            var categoryId = await _categories.SaveAsync(new SaveCategoryRequest
            {
                Id = id,
                Name = name,
                Slug = slug,
                Description = description,
                Icon = icon,
                ParentId = parentId,
                SortOrder = sortOrder,
                IsActive = isActive
            }, cancellationToken);

            _toast.Success("دسته‌بندی ذخیره شد.");
            return Redirect($"/admin/categories/edit/{categoryId}");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
            await LoadAsync(id, cancellationToken);
            return Page();
        }
    }
}
