using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages.Payment;

/// <summary>بازگشت از درگاه پرداخت و تأیید تراکنش.</summary>
public class CallbackModel : PageModel
{
    private readonly IOrderService _orders;

    public CallbackModel(IOrderService orders) => _orders = orders;

    public async Task<IActionResult> OnGetAsync(string order, string authority, CancellationToken cancellationToken)
    {
        var success = await _orders.ConfirmOnlinePaymentAsync(order, authority, cancellationToken);
        return Redirect(success ? $"/order/success/{order}" : $"/order/failed?order={order}");
    }
}
