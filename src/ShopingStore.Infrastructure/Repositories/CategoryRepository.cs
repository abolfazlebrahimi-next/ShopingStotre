using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Infrastructure.Data;

namespace ShopingStore.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> GetAllAsync(bool onlyActive = true, CancellationToken ct = default)
        => await _db.Categories
            .Where(c => !onlyActive || c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<IReadOnlyList<Category>> GetWithProductCountsAsync(bool onlyActive = true, CancellationToken ct = default)
        => await _db.Categories
            .Where(c => !onlyActive || c.IsActive)
            .Include(c => c.Children)
            .Include(c => c.Products.Where(p => p.IsActive))
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AddAsync(Category category, CancellationToken ct = default)
        => await _db.Categories.AddAsync(category, ct);

    public void Update(Category category) => _db.Categories.Update(category);

    public void Remove(Category category)
    {
        category.IsDeleted = true;
        category.IsActive = false;
        _db.Categories.Update(category);
    }

    public Task<bool> SlugExistsAsync(string slug, int? exceptId = null, CancellationToken ct = default)
        => _db.Categories.AnyAsync(c => c.Slug == slug && (exceptId == null || c.Id != exceptId), ct);

    public Task<bool> HasProductsAsync(int categoryId, CancellationToken ct = default)
        => _db.Products.AnyAsync(p => p.CategoryId == categoryId, ct);
}
