using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Infrastructure.Data;

namespace ShopingStore.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<User?> GetByExternalKeyAsync(string provider, string key, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.ExternalProvider == provider && u.ExternalProviderKey == key, ct);

    public async Task<PagedResult<User>> SearchAsync(AdminUserFilter filter, CancellationToken ct = default)
    {
        var users = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            users = users.Where(u => u.FullName.Contains(term) ||
                                     u.Email.Contains(term) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        if (filter.Role is not null) users = users.Where(u => u.Role == filter.Role);
        if (filter.IsActive is not null) users = users.Where(u => u.IsActive == filter.IsActive);

        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize is <= 0 or > 100 ? 15 : filter.PageSize;

        var total = await users.CountAsync(ct);
        var items = await users.OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(ct);

        return new PagedResult<User> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    public void Update(User user) => _db.Users.Update(user);

    public Task<bool> EmailExistsAsync(string email, int? exceptId = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Users.AnyAsync(u => u.Email == normalized && (exceptId == null || u.Id != exceptId), ct);
    }

    public Task<int> CountAsync(bool onlyCustomers = true, CancellationToken ct = default)
        => _db.Users.CountAsync(u => !onlyCustomers || u.Role == UserRole.Customer, ct);

    public Task<int> CountNewSinceAsync(DateTime fromUtc, CancellationToken ct = default)
        => _db.Users.CountAsync(u => u.CreatedAtUtc >= fromUtc, ct);

    public async Task<(int OrdersCount, decimal TotalSpent)> GetUserStatsAsync(int userId, CancellationToken ct = default)
    {
        var orders = await _db.Orders.Where(o => o.UserId == userId && o.Status != OrderStatus.Canceled).ToListAsync(ct);
        return (orders.Count, orders.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.Total));
    }
}
