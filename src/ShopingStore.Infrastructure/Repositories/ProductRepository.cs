using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Infrastructure.Data;

namespace ShopingStore.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db) => _db = db;

    private IQueryable<Product> Query => _db.Products.Include(p => p.Category).AsQueryable();

    public async Task<PagedResult<Product>> SearchAsync(ProductQuery query, CancellationToken ct = default)
    {
        var products = Query.Where(p => p.IsActive || query.IncludeInactive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p =>
                p.Name.Contains(term) ||
                (p.Brand != null && p.Brand.Contains(term)) ||
                (p.ShortDescription != null && p.ShortDescription.Contains(term)) ||
                (p.Sku != null && p.Sku.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var slug = query.Category.Trim();
            products = products.Where(p => p.Category!.Slug == slug || p.Category!.Parent!.Slug == slug);
        }

        if (!string.IsNullOrWhiteSpace(query.Brand))
            products = products.Where(p => p.Brand == query.Brand);

        if (query.MinPrice is > 0)
            products = products.Where(p => (p.DiscountPrice ?? p.Price) >= query.MinPrice);

        if (query.MaxPrice is > 0)
            products = products.Where(p => (p.DiscountPrice ?? p.Price) <= query.MaxPrice);

        if (query.OnlyAvailable)
            products = products.Where(p => p.Stock > 0);

        if (query.OnlyDiscounted)
            products = products.Where(p => p.DiscountPrice != null && p.DiscountPrice > 0 && p.DiscountPrice < p.Price);

        products = query.SortBy switch
        {
            ProductSortBy.Cheapest => products.OrderBy(p => p.DiscountPrice ?? p.Price),
            ProductSortBy.MostExpensive => products.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            ProductSortBy.BestSelling => products.OrderByDescending(p => p.SoldCount).ThenByDescending(p => p.Id),
            ProductSortBy.TopRated => products.OrderByDescending(p => p.Rating).ThenByDescending(p => p.ReviewsCount),
            ProductSortBy.MostDiscounted => products.OrderByDescending(p =>
                p.DiscountPrice == null ? 0 : (p.Price - p.DiscountPrice.Value) / p.Price * 100),
            _ => products.OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id)
        };

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize is <= 0 or > 60 ? 12 : query.PageSize;

        var total = await products.CountAsync(ct);
        var items = await products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<Product> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<Product?> GetByIdAsync(int id, bool includeRelated = false, CancellationToken ct = default)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();
        if (includeRelated)
            query = query.Include(p => p.Images).Include(p => p.Reviews);

        return await query.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product?> GetBySlugAsync(string slug, bool includeRelated = false, CancellationToken ct = default)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();
        if (includeRelated)
            query = query.Include(p => p.Images)
                         .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved));

        return await query.FirstOrDefaultAsync(p => p.Slug == slug, ct);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int take, CancellationToken ct = default)
        => await Query.Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.Rating).ThenByDescending(p => p.SoldCount)
            .Take(take).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetNewestAsync(int take, CancellationToken ct = default)
        => await Query.Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id)
            .Take(take).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetBestSellersAsync(int take, CancellationToken ct = default)
        => await Query.Where(p => p.IsActive)
            .OrderByDescending(p => p.SoldCount).ThenByDescending(p => p.Rating)
            .Take(take).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetDiscountedAsync(int take, CancellationToken ct = default)
        => await Query.Where(p => p.IsActive && p.DiscountPrice != null && p.DiscountPrice > 0 && p.DiscountPrice < p.Price)
            .OrderByDescending(p => (p.Price - p.DiscountPrice!.Value) / p.Price)
            .Take(take).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int categoryId, int take, CancellationToken ct = default)
        => await Query.Where(p => p.IsActive && p.Id != productId && p.CategoryId == categoryId)
            .OrderByDescending(p => p.SoldCount)
            .Take(take).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var list = ids.Distinct().ToList();
        return await Query.Where(p => list.Contains(p.Id)).AsNoTracking().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(int threshold, int take, CancellationToken ct = default)
        => await Query.Where(p => p.IsActive && p.Stock <= threshold)
            .OrderBy(p => p.Stock)
            .Take(take).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken ct = default)
        => await _db.Products.Where(p => p.IsActive && p.Brand != null && p.Brand != "")
            .Select(p => p.Brand!)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(ct);

    public async Task<(decimal Min, decimal Max)> GetPriceRangeAsync(CancellationToken ct = default)
    {
        var prices = await _db.Products.Where(p => p.IsActive).Select(p => p.DiscountPrice ?? p.Price).ToListAsync(ct);
        if (prices.Count == 0) return (0, 0);
        return (prices.Min(), prices.Max());
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
        => await _db.Products.AddAsync(product, ct);

    public void Update(Product product) => _db.Products.Update(product);

    public void Remove(Product product)
    {
        product.IsDeleted = true;
        product.IsActive = false;
        _db.Products.Update(product);
    }

    public Task<bool> SlugExistsAsync(string slug, int? exceptId = null, CancellationToken ct = default)
        => _db.Products.AnyAsync(p => p.Slug == slug && (exceptId == null || p.Id != exceptId), ct);

    public Task<int> CountAsync(CancellationToken ct = default)
        => _db.Products.CountAsync(ct);

    public Task IncrementViewAsync(int productId, CancellationToken ct = default)
        => _db.Products.Where(p => p.Id == productId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(p => p.ViewCount, p => p.ViewCount + 1), ct);
}
