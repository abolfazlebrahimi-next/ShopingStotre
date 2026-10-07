using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class OrdersModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly ICurrentUser _currentUser;

    public OrdersModel(IOrderService orders, ICurrentUser currentUser)
    {
        _orders = orders;
        _currentUser = currentUser;
    }

    public PagedResult<OrderDto> Orders { get; private set; } = PagedResult<OrderDto>.Empty();
    public PaginationModel Pagination { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int page, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login?returnUrl=/account/orders");

        Orders = await _orders.GetUserOrdersAsync(_currentUser.UserId.Value, page <= 0 ? 1 : page, 10, cancellationToken);

        Pagination = new PaginationModel
        {
            Page = Orders.Page,
            TotalPages = Orders.TotalPages,
            TotalCount = Orders.TotalCount,
            BasePath = "/account/orders",
            PageParameter = "page",
            Query = Request.Query
        };

        return Page();
    }
}
