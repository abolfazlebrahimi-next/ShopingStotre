using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Account;

public class OrderDetailsModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly ICurrentUser _currentUser;
    private readonly ToastService _toast;

    public OrderDetailsModel(IOrderService orders, ICurrentUser currentUser, ToastService toast)
    {
        _orders = orders;
        _currentUser = currentUser;
        _toast = toast;
    }

    public OrderDto Order { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login?returnUrl=/account/orders");

        try
        {
            var order = await _orders.GetOrderAsync(id, _currentUser.UserId, cancellationToken);
            if (order is null) return NotFound();

            Order = order;
            return Page();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }

    public async Task<IActionResult> OnPostCancelAsync(int id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login");

        try
        {
            await _orders.CancelAsync(id, _currentUser.UserId, "لغو توسط مشتری", cancellationToken);
            _toast.Success("سفارش شما لغو شد و مبلغ آن در صورت پرداخت، بازگردانده می‌شود.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }

        return RedirectToPage(new { id });
    }
}
