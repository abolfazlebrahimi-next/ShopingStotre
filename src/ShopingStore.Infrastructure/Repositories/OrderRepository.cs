using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Infrastructure.Data;

namespace ShopingStore.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;

    public OrderRepository(ApplicationDbContext db) => _db = db;

    private IQueryable<Order> Query => _db.Orders
        .Include(o => o.Items).ThenInclude(i => i.Product)
        .Include(o => o.History)
        .Include(o => o.User);

    public async Task<PagedResult<Order>> GetUserOrdersAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        var orders = Query.Where(o => o.UserId == userId);
        var total = await orders.CountAsync(ct);

        var items = await orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<Order> { Items = items, TotalCount = total, Page = Math.Max(page, 1), PageSize = pageSize };
    }

    public async Task<PagedResult<Order>> SearchAsync(AdminOrderFilter filter, CancellationToken ct = default)
    {
        var orders = Query.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            orders = orders.Where(o => o.OrderNumber.Contains(term) ||
                                       o.CustomerName.Contains(term) ||
                                       o.CustomerPhone.Contains(term) ||
                                       (o.CustomerEmail != null && o.CustomerEmail.Contains(term)));
        }

        if (filter.Status is not null)
            orders = orders.Where(o => o.Status == filter.Status);

        if (filter.PaymentStatus is not null)
            orders = orders.Where(o => o.PaymentStatus == filter.PaymentStatus);

        if (filter.FromUtc is not null)
            orders = orders.Where(o => o.CreatedAtUtc >= filter.FromUtc);

        if (filter.ToUtc is not null)
            orders = orders.Where(o => o.CreatedAtUtc <= filter.ToUtc);

        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize is <= 0 or > 100 ? 15 : filter.PageSize;

        var total = await orders.CountAsync(ct);
        var items = await orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<Order> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<Order?> GetByIdAsync(int id, bool includeItems = true, CancellationToken ct = default)
    {
        var query = _db.Orders.AsQueryable();
        if (includeItems)
            query = query.Include(o => o.Items).ThenInclude(i => i.Product)
                         .Include(o => o.History)
                         .Include(o => o.User);
        return await query.FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken ct = default)
        => Query.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await _db.Orders.AddAsync(order, ct);

    public void Update(Order order) => _db.Orders.Update(order);

    public async Task<IReadOnlyList<Order>> GetRecentAsync(int take, CancellationToken ct = default)
        => await Query.OrderByDescending(o => o.CreatedAtUtc).Take(take).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetPaidOrdersSinceAsync(DateTime fromUtc, CancellationToken ct = default)
        => await _db.Orders
            .Where(o => o.CreatedAtUtc >= fromUtc && o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Pending)
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default)
        => await _db.Orders
            .Where(o => o.Status != OrderStatus.Canceled && o.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(o => (decimal?)o.Total, ct) ?? 0;

    public async Task<int> CountAsync(OrderStatus? status = null, CancellationToken ct = default)
        => await _db.Orders.CountAsync(o => status == null || o.Status == status, ct);

    public async Task<IReadOnlyList<(int ProductId, string Name, int Quantity, decimal Revenue)>> GetTopProductsAsync(int take, CancellationToken ct = default)
    {
        var raw = await _db.OrderItems
            .Where(i => i.Order!.Status != OrderStatus.Canceled)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(take)
            .ToListAsync(ct);

        return raw.Select(x => (x.ProductId, x.ProductName, x.Quantity, x.Revenue)).ToList();
    }

    public async Task<IReadOnlyList<(string CategoryName, int Quantity, decimal Revenue)>> GetSalesByCategoryAsync(CancellationToken ct = default)
    {
        var raw = await _db.OrderItems
            .Where(i => i.Order!.Status != OrderStatus.Canceled)
            .GroupBy(i => i.Product!.Category!.Name)
            .Select(g => new
            {
                CategoryName = g.Key,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync(ct);

        return raw.Select(x => (x.CategoryName, x.Quantity, x.Revenue)).ToList();
    }
}
