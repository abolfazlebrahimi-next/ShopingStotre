using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Products;

public class IndexModel : PageModel
{
    private readonly IAdminProductService _products;
    private readonly IAdminCategoryService _categories;
    private readonly ToastService _toast;

    public IndexModel(IAdminProductService products, IAdminCategoryService categories, ToastService toast)
    {
        _products = products;
        _categories = categories;
        _toast = toast;
    }

    public PagedResult<ProductListItemDto> Products { get; private set; } = PagedResult<ProductListItemDto>.Empty();
    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();
    public ProductQuery Query { get; private set; } = new();
    public PaginationModel Pagination { get; private set; } = new();

    public async Task OnGetAsync(
        string? search,
        string? category,
        ProductSortBy sortBy,
        int page,
        CancellationToken cancellationToken)
    {
        Query = new ProductQuery
        {
            Search = search,
            Category = category,
            SortBy = sortBy,
            Page = page <= 0 ? 1 : page,
            PageSize = 15,
            IncludeInactive = true
        };

        Products = await _products.SearchAsync(Query, true, cancellationToken);
        Categories = await _categories.GetAllAsync(cancellationToken);

        Pagination = new PaginationModel
        {
            Page = Products.Page,
            TotalPages = Products.TotalPages,
            TotalCount = Products.TotalCount,
            BasePath = "/admin/products",
            Query = Request.Query
        };
    }

    public async Task<IActionResult> OnPostToggleAsync(int id, CancellationToken cancellationToken)
    {
        await _products.ToggleActiveAsync(id, cancellationToken);
        _toast.Success("وضعیت محصول تغییر کرد.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _products.DeleteAsync(id, cancellationToken);
        _toast.Success("محصول حذف شد.");
        return RedirectToPage();
    }
}
