using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly IDashboardService _dashboard;

    public IndexModel(IOrderService orders, IDashboardService dashboard)
    {
        _orders = orders;
        _dashboard = dashboard;
    }

    public PagedResult<OrderDto> Orders { get; private set; } = PagedResult<OrderDto>.Empty();
    public AdminOrderFilter Filter { get; private set; } = new();
    public PaginationModel Pagination { get; private set; } = new();
    public DashboardStatsDto Stats { get; private set; } = new();

    public async Task OnGetAsync(
        string? search,
        OrderStatus? status,
        PaymentStatus? paymentStatus,
        int page,
        CancellationToken cancellationToken)
    {
        Filter = new AdminOrderFilter
        {
            Search = search,
            Status = status,
            PaymentStatus = paymentStatus,
            Page = page <= 0 ? 1 : page,
            PageSize = 15
        };

        Orders = await _orders.SearchAsync(Filter, cancellationToken);
        Stats = await _dashboard.GetStatsAsync(7, cancellationToken);

        Pagination = new PaginationModel
        {
            Page = Orders.Page,
            TotalPages = Orders.TotalPages,
            TotalCount = Orders.TotalCount,
            BasePath = "/admin/orders",
            Query = Request.Query
        };
    }
}
