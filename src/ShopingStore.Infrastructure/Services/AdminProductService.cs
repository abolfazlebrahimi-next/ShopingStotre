using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class AdminProductService : IAdminProductService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public AdminProductService(IProductRepository products, ICategoryRepository categories, IUnitOfWork uow)
    {
        _products = products;
        _categories = categories;
        _uow = uow;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(ProductQuery query, bool includeInactive, CancellationToken ct = default)
    {
        query.IncludeInactive = includeInactive;
        query.PageSize = query.PageSize <= 0 ? 15 : query.PageSize;

        var result = await _products.SearchAsync(query, ct);
        return result.Map(p => p.ToListItem());
    }

    public Task<Product?> GetForEditAsync(int id, CancellationToken ct = default)
        => _products.GetByIdAsync(id, true, ct);

    public async Task<int> SaveAsync(SaveProductRequest request, CancellationToken ct = default)
    {
        var name = Guard.NotEmpty(request.Name, "نام محصول", 250);
        Guard.Positive(request.Price, "قیمت");

        if (request.DiscountPrice is not null && request.DiscountPrice >= request.Price)
            throw new BusinessException("قیمت با تخفیف باید کمتر از قیمت اصلی باشد.");

        Guard.NotNegative(request.Stock, "موجودی");

        var category = await _categories.GetByIdAsync(request.CategoryId, ct)
            ?? throw new BusinessException("دسته‌بندی انتخاب‌شده معتبر نیست.");

        Product product;
        if (request.Id is > 0)
        {
            product = await _products.GetByIdAsync(request.Id.Value, true, ct) ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");
        }
        else
        {
            product = new Product();
            await _products.AddAsync(product, ct);
        }

        var slug = SlugHelper.Generate(string.IsNullOrWhiteSpace(request.Slug) ? name : request.Slug, "product");
        if (await _products.SlugExistsAsync(slug, product.Id == 0 ? null : product.Id, ct))
            slug = $"{slug}-{Random.Shared.Next(100, 999)}";

        product.Name = name;
        product.Slug = slug;
        product.ShortDescription = Guard.Optional(request.ShortDescription, 600);
        product.Description = Guard.Optional(request.Description, 20000);
        product.Brand = Guard.Optional(request.Brand, 120);
        product.Sku = Guard.Optional(request.Sku, 60);
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.Stock = request.Stock;
        product.CategoryId = category.Id;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.IsNew = request.IsNew;
        product.MainImageUrl = Guard.Optional(request.MainImageUrl, 500) ?? Mappers.PlaceholderImage;

        product.Specifications = request.Specifications
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .Select(s => new ProductSpecification { Name = s.Name.Trim(), Value = (s.Value ?? string.Empty).Trim() })
            .ToList();

        product.Variants = request.Variants
            .Where(v => !string.IsNullOrWhiteSpace(v.Name))
            .Select(v => new ProductVariant
            {
                Name = v.Name.Trim(),
                Options = v.Options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList()
            })
            .ToList();

        // گالری تصاویر
        product.Images.Clear();
        var sortOrder = 0;
        foreach (var url in request.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct())
        {
            product.Images.Add(new ProductImage { Url = url.Trim(), Alt = name, SortOrder = sortOrder++ });
        }

        _products.Update(product);
        await _uow.SaveChangesAsync(ct);

        return product.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, false, ct) ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");
        _products.Remove(product);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, false, ct) ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");
        product.IsActive = !product.IsActive;
        _products.Update(product);
        await _uow.SaveChangesAsync(ct);
    }
}
