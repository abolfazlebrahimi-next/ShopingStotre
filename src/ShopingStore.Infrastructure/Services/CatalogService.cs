using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class CatalogService : ICatalogService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly IReviewRepository _reviews;
    private readonly IFavoriteRepository _favorites;

    public CatalogService(
        IProductRepository products,
        ICategoryRepository categories,
        IReviewRepository reviews,
        IFavoriteRepository favorites)
    {
        _products = products;
        _categories = categories;
        _reviews = reviews;
        _favorites = favorites;
    }

    private async Task<IReadOnlySet<int>> GetFavoriteIdsAsync(int? userId, CancellationToken ct)
    {
        if (userId is null or 0) return new HashSet<int>();
        var ids = await _favorites.GetFavoriteProductIdsAsync(userId.Value, ct);
        return ids.ToHashSet();
    }

    public async Task<HomePageDto> GetHomePageAsync(int? userId = null, CancellationToken ct = default)
    {
        var favoriteIds = await GetFavoriteIdsAsync(userId, ct);

        var categories = await _categories.GetWithProductCountsAsync(true, ct);
        var featured = await _products.GetFeaturedAsync(8, ct);
        var newest = await _products.GetNewestAsync(8, ct);
        var bestSellers = await _products.GetBestSellersAsync(8, ct);
        var discounted = await _products.GetDiscountedAsync(8, ct);
        var latestReviews = await _reviews.GetLatestAsync(6, true, ct);

        var roots = categories.Where(c => c.ParentId is null).ToList();
        var rootDtos = roots.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            ImageUrl = c.ImageUrl,
            Icon = c.Icon,
            ProductsCount = c.Products.Count + categories.Where(x => x.ParentId == c.Id).Sum(x => x.Products.Count),
            Children = categories.Where(x => x.ParentId == c.Id)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    ImageUrl = x.ImageUrl,
                    Icon = x.Icon,
                    ProductsCount = x.Products.Count
                }).ToList()
        }).ToList();

        return new HomePageDto
        {
            Categories = rootDtos,
            FeaturedProducts = featured.ToListItems(favoriteIds),
            NewestProducts = newest.ToListItems(favoriteIds),
            BestSellers = bestSellers.ToListItems(favoriteIds),
            DiscountedProducts = discounted.ToListItems(favoriteIds),
            LatestReviews = latestReviews.ToDtos(),
            ProductsCount = await _products.CountAsync(ct),
            CustomersCount = 0,
            OrdersCount = 0
        };
    }

    public async Task<PagedResult<ProductListItemDto>> SearchProductsAsync(ProductQuery query, int? userId = null, CancellationToken ct = default)
    {
        var favoriteIds = await GetFavoriteIdsAsync(userId, ct);
        var page = await _products.SearchAsync(query, ct);
        return page.Map(p => p.ToListItem(favoriteIds));
    }

    public async Task<ProductDetailsDto?> GetProductDetailsAsync(string slug, int? userId = null, bool increaseViewCount = true, CancellationToken ct = default)
    {
        var product = await _products.GetBySlugAsync(slug, true, ct);
        if (product is null) return null;

        var favoriteIds = await GetFavoriteIdsAsync(userId, ct);
        var reviews = product.Reviews.Where(r => r.Status == ReviewStatus.Approved).ToList();
        var related = await _products.GetRelatedAsync(product.Id, product.CategoryId, 4, ct);

        if (increaseViewCount)
            await _products.IncrementViewAsync(product.Id, ct);

        return product.ToDetails(reviews, related, favoriteIds.Contains(product.Id), favoriteIds);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoryTreeAsync(CancellationToken ct = default)
    {
        var categories = await _categories.GetWithProductCountsAsync(true, ct);
        return categories
            .Where(c => c.ParentId is null)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                Icon = c.Icon,
                ProductsCount = c.Products.Count,
                Children = categories.Where(x => x.ParentId == c.Id).Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    ImageUrl = x.ImageUrl,
                    Icon = x.Icon,
                    ProductsCount = x.Products.Count
                }).ToList()
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetSuggestionsAsync(string term, int take = 6, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term)) return Array.Empty<ProductListItemDto>();

        var page = await _products.SearchAsync(new ProductQuery { Search = term, PageSize = take }, ct);
        return page.Items.ToListItems();
    }

    public Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken ct = default)
        => _products.GetBrandsAsync(ct);

    public Task<(decimal Min, decimal Max)> GetPriceRangeAsync(CancellationToken ct = default)
        => _products.GetPriceRangeAsync(ct);
}
