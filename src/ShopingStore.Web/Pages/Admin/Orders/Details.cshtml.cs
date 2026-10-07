using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Orders;

public class DetailsModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly ICurrentUser _currentUser;
    private readonly ToastService _toast;

    public DetailsModel(IOrderService orders, ICurrentUser currentUser, ToastService toast)
    {
        _orders = orders;
        _currentUser = currentUser;
        _toast = toast;
    }

    public OrderDto Order { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetOrderAsync(id, null, cancellationToken);
        if (order is null) return NotFound();

        Order = order;
        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(int orderId, OrderStatus status, string? trackingCode, string? note, CancellationToken cancellationToken)
    {
        try
        {
            await _orders.UpdateStatusAsync(new UpdateOrderStatusRequest
            {
                OrderId = orderId,
                Status = status,
                TrackingCode = trackingCode,
                Note = note
            }, _currentUser.FullName ?? "مدیریت", cancellationToken);

            _toast.Success($"وضعیت سفارش به «{status.ToPersianTitle()}» تغییر کرد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return Redirect($"/admin/orders/{orderId}");
    }
}
