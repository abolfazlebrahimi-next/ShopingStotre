using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Infrastructure.Services;

public class AdminCategoryService : IAdminCategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public AdminCategoryService(ICategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
        => _categories.GetAllAsync(false, ct);

    public Task<Category?> GetForEditAsync(int id, CancellationToken ct = default)
        => _categories.GetByIdAsync(id, ct);

    public async Task<int> SaveAsync(SaveCategoryRequest request, CancellationToken ct = default)
    {
        var name = Guard.NotEmpty(request.Name, "نام دسته‌بندی", 150);

        Category category;
        if (request.Id is > 0)
        {
            category = await _categories.GetByIdAsync(request.Id.Value, ct) ?? throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");
        }
        else
        {
            category = new Category();
            await _categories.AddAsync(category, ct);
        }

        if (request.ParentId is > 0 && request.ParentId == category.Id)
            throw new BusinessException("دسته‌بندی نمی‌تواند والد خودش باشد.");

        var slug = SlugHelper.Generate(string.IsNullOrWhiteSpace(request.Slug) ? name : request.Slug, "category");
        if (await _categories.SlugExistsAsync(slug, category.Id == 0 ? null : category.Id, ct))
            slug = $"{slug}-{Random.Shared.Next(10, 99)}";

        category.Name = name;
        category.Slug = slug;
        category.Description = Guard.Optional(request.Description, 1000);
        category.ImageUrl = Guard.Optional(request.ImageUrl, 500);
        category.Icon = Guard.Optional(request.Icon, 500);
        category.ParentId = request.ParentId is > 0 ? request.ParentId : null;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;

        _categories.Update(category);
        await _uow.SaveChangesAsync(ct);

        return category.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _categories.GetByIdAsync(id, ct) ?? throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

        if (await _categories.HasProductsAsync(id, ct))
            throw new BusinessException("این دسته‌بندی دارای محصول است؛ ابتدا محصولات آن را منتقل یا حذف کنید.");

        _categories.Remove(category);
        await _uow.SaveChangesAsync(ct);
    }
}
