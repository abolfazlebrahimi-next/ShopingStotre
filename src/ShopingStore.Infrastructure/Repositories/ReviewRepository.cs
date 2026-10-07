using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Infrastructure.Data;

namespace ShopingStore.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _db;

    public ReviewRepository(ApplicationDbContext db) => _db = db;

    private IQueryable<Review> Query => _db.Reviews.Include(r => r.Product);

    public async Task<IReadOnlyList<Review>> GetByProductAsync(int productId, bool onlyApproved = true, CancellationToken ct = default)
        => await Query.Where(r => r.ProductId == productId && (!onlyApproved || r.Status == ReviewStatus.Approved))
            .OrderByDescending(r => r.CreatedAtUtc)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Review>> GetLatestAsync(int take, bool onlyApproved = true, CancellationToken ct = default)
        => await Query.Where(r => !onlyApproved || r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<PagedResult<Review>> SearchAsync(string? search, ReviewStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var reviews = Query.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            reviews = reviews.Where(r => r.Comment.Contains(term) || r.AuthorName.Contains(term) ||
                                         (r.Product != null && r.Product.Name.Contains(term)));
        }

        if (status is not null) reviews = reviews.Where(r => r.Status == status);

        page = Math.Max(page, 1);
        pageSize = pageSize is <= 0 or > 100 ? 15 : pageSize;

        var total = await reviews.CountAsync(ct);
        var items = await reviews.OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(ct);

        return new PagedResult<Review> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public Task<Review?> GetByIdAsync(int id, CancellationToken ct = default)
        => Query.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> HasUserReviewedAsync(int productId, int userId, CancellationToken ct = default)
        => _db.Reviews.AnyAsync(r => r.ProductId == productId && r.UserId == userId, ct);

    public async Task AddAsync(Review review, CancellationToken ct = default)
        => await _db.Reviews.AddAsync(review, ct);

    public void Update(Review review) => _db.Reviews.Update(review);

    public void Remove(Review review)
    {
        review.IsDeleted = true;
        _db.Reviews.Update(review);
    }

    public async Task<(double Average, int Count)> GetRatingAsync(int productId, CancellationToken ct = default)
    {
        var ratings = await _db.Reviews
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .Select(r => r.Rating)
            .ToListAsync(ct);

        return ratings.Count == 0 ? (0, 0) : (Math.Round(ratings.Average(), 2), ratings.Count);
    }

    public Task<int> CountAsync(ReviewStatus? status = null, CancellationToken ct = default)
        => _db.Reviews.CountAsync(r => status == null || r.Status == status, ct);
}
