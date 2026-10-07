using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly IUserRepository _users;
    private readonly IReviewRepository _reviews;

    public DashboardService(
        IOrderRepository orders,
        IProductRepository products,
        IUserRepository users,
        IReviewRepository reviews)
    {
        _orders = orders;
        _products = products;
        _users = users;
        _reviews = reviews;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(int chartDays = 14, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var chartStart = todayStart.AddDays(-(chartDays - 1));

        var paidOrders = await _orders.GetPaidOrdersSinceAsync(chartStart, ct);
        var recentOrders = await _orders.GetRecentAsync(8, ct);
        var topProducts = await _orders.GetTopProductsAsync(6, ct);
        var salesByCategory = await _orders.GetSalesByCategoryAsync(ct);
        var lowStock = await _products.GetLowStockAsync(5, 8, ct);
        var outOfStock = await _products.GetLowStockAsync(0, 100, ct);

        var dailySales = paidOrders
            .GroupBy(o => o.CreatedAtUtc.Date)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Revenue: g.Sum(o => o.Total)));

        var chart = new List<SalesChartPointDto>();
        for (var day = chartStart; day <= todayStart; day = day.AddDays(1))
        {
            var (_, month, dayOfMonth, _, _) = PersianDate.ToPersian(day);
            dailySales.TryGetValue(day, out var value);

            chart.Add(new SalesChartPointDto
            {
                DateUtc = day,
                Label = $"{PersianDate.ToPersianDigits(dayOfMonth.ToString())} {PersianDate.MonthName(month)}",
                OrdersCount = value.Count,
                Revenue = value.Revenue
            });
        }

        var allPaidOrders = await _orders.GetPaidOrdersSinceAsync(monthStart, ct);
        var todayOrders = await _orders.GetRecentAsync(200, ct);
        var todayCount = todayOrders.Count(o => o.CreatedAtUtc >= todayStart);

        var topProductEntities = await _products.GetByIdsAsync(topProducts.Select(t => t.ProductId), ct);
        var imagesById = topProductEntities.ToDictionary(p => p.Id, p => p.MainImageUrl);

        var topProductsDto = topProducts.Select(t => new TopProductDto
        {
            ProductId = t.ProductId,
            Name = t.Name,
            ImageUrl = imagesById.TryGetValue(t.ProductId, out var image) && !string.IsNullOrWhiteSpace(image)
                ? image
                : Mappers.PlaceholderImage,
            SoldCount = t.Quantity,
            Revenue = t.Revenue
        }).ToList();

        var totalRevenue = await _orders.GetTotalRevenueAsync(ct);

        return new DashboardStatsDto
        {
            TotalRevenue = totalRevenue,
            TodayRevenue = dailySales.TryGetValue(todayStart, out var today) ? today.Revenue : 0,
            MonthRevenue = allPaidOrders.Sum(o => o.Total),
            TotalOrders = await _orders.CountAsync(null, ct),
            TodayOrders = todayCount,
            PendingOrders = await _orders.CountAsync(OrderStatus.Pending, ct),
            ProcessingOrders = await _orders.CountAsync(OrderStatus.Processing, ct) + await _orders.CountAsync(OrderStatus.Paid, ct),
            TotalProducts = await _products.CountAsync(ct),
            OutOfStockProducts = outOfStock.Count,
            TotalCustomers = await _users.CountAsync(true, ct),
            NewCustomersThisMonth = await _users.CountNewSinceAsync(monthStart, ct),
            TotalReviews = await _reviews.CountAsync(null, ct),
            PendingReviews = await _reviews.CountAsync(ReviewStatus.Pending, ct),
            AverageOrderValue = paidOrders.Count == 0 ? 0 : Math.Round(paidOrders.Average(o => o.Total), 0),
            SalesChart = chart,
            SalesByCategory = salesByCategory
                .Select(s => new CategorySalesDto { CategoryName = s.CategoryName, ItemsSold = s.Quantity, Revenue = s.Revenue })
                .ToList(),
            TopProducts = topProductsDto,
            LatestOrders = recentOrders.Select(o => o.ToDto()).ToList(),
            LowStockProducts = lowStock.Select(p => new LowStockProductDto { ProductId = p.Id, Name = p.Name, Stock = p.Stock }).ToList()
        };
    }
}
