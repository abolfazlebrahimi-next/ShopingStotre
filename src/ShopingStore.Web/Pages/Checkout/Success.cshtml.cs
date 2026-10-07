using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages.Checkout;

public class SuccessModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly ICurrentUser _currentUser;

    public SuccessModel(IOrderService orders, ICurrentUser currentUser)
    {
        _orders = orders;
        _currentUser = currentUser;
    }

    public OrderDto Order { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await _orders.GetOrderByNumberAsync(orderNumber, null, cancellationToken);
        if (order is null) return NotFound();

        Order = order;
        return Page();
    }
}
