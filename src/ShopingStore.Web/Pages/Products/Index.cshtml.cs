using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly ICatalogService _catalog;
    private readonly ICurrentUser _currentUser;

    public IndexModel(ICatalogService catalog, ICurrentUser currentUser)
    {
        _catalog = catalog;
        _currentUser = currentUser;
    }

    public ProductQuery Query { get; private set; } = new();
    public PagedResult<ProductListItemDto> Products { get; private set; } = PagedResult<ProductListItemDto>.Empty();
    public IReadOnlyList<CategoryDto> Categories { get; private set; } = Array.Empty<CategoryDto>();
    public IReadOnlyList<string> Brands { get; private set; } = Array.Empty<string>();
    public (decimal Min, decimal Max) PriceRange { get; private set; }
    public string? SelectedCategoryName { get; private set; }
    public PaginationModel Pagination { get; private set; } = new();

    public async Task OnGetAsync(
        string? category,
        [FromQuery(Name = "search")] string? search,
        [FromQuery(Name = "brand")] string? brand,
        [FromQuery(Name = "minPrice")] decimal? minPrice,
        [FromQuery(Name = "maxPrice")] decimal? maxPrice,
        [FromQuery(Name = "onlyAvailable")] bool onlyAvailable,
        [FromQuery(Name = "onlyDiscounted")] bool onlyDiscounted,
        [FromQuery(Name = "sortBy")] ProductSortBy sortBy,
        [FromQuery(Name = "page")] int page,
        CancellationToken cancellationToken)
    {
        var routeCategory = RouteData.Values["category"]?.ToString();
        var selectedCategory = string.IsNullOrWhiteSpace(category) ? routeCategory : category;

        Query = new ProductQuery
        {
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            Category = string.IsNullOrWhiteSpace(selectedCategory) ? null : selectedCategory,
            Brand = string.IsNullOrWhiteSpace(brand) ? null : brand,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            OnlyAvailable = onlyAvailable,
            OnlyDiscounted = onlyDiscounted,
            SortBy = sortBy,
            Page = page <= 0 ? 1 : page,
            PageSize = 12
        };

        Products = await _catalog.SearchProductsAsync(Query, _currentUser.UserId, cancellationToken);
        Categories = await _catalog.GetCategoryTreeAsync(cancellationToken);
        Brands = await _catalog.GetBrandsAsync(cancellationToken);
        PriceRange = await _catalog.GetPriceRangeAsync(cancellationToken);

        if (Query.Category is not null)
        {
            SelectedCategoryName = Categories
                .SelectMany(c => c.Children.Prepend(c))
                .FirstOrDefault(c => c.Slug == Query.Category)?.Name;
        }

        // اگر دسته‌بندی از مسیر آمده باشد، لینک صفحه‌بندی همان مسیر را نگه می‌دارد
        var basePath = "/products";
        if (RouteData.Values["category"] is not null && string.IsNullOrWhiteSpace(category))
            basePath = $"/category/{RouteData.Values["category"]}";

        Pagination = new PaginationModel
        {
            Page = Products.Page,
            TotalPages = Products.TotalPages,
            TotalCount = Products.TotalCount,
            BasePath = basePath,
            PageParameter = "page",
            Query = Request.Query
        };
    }
}
