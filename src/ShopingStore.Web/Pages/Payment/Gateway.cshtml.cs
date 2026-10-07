using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages.Payment;

/// <summary>صفحه درگاه پرداخت آزمایشی (شبیه‌ساز بانک).</summary>
public class GatewayModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly ISettingsService _settings;

    public GatewayModel(IOrderService orders, ISettingsService settings)
    {
        _orders = orders;
        _settings = settings;
    }

    public string OrderNumber { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string BankName { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string order, CancellationToken cancellationToken)
    {
        var orderDto = await _orders.GetOrderByNumberAsync(order, null, cancellationToken);
        if (orderDto is null) return NotFound();

        if (orderDto.PaymentStatus == PaymentStatus.Paid)
            return Redirect($"/order/success/{orderDto.OrderNumber}");

        var settings = await _settings.GetAsync(cancellationToken);

        OrderNumber = orderDto.OrderNumber;
        Amount = orderDto.Total;
        BankName = settings.BankName;

        return Page();
    }

    /// <summary>پرداخت موفق: دریافت توکن از درگاه و بازگشت به سایت (Callback).</summary>
    public async Task<IActionResult> OnPostPayAsync(string order, CancellationToken cancellationToken)
    {
        var orderDto = await _orders.GetOrderByNumberAsync(order, null, cancellationToken);
        if (orderDto is null) return NotFound();

        var callbackUrl = $"{Request.Scheme}://{Request.Host}/payment/callback?order={orderDto.OrderNumber}";
        var payment = await _orders.StartPaymentAsync(orderDto.Id, callbackUrl, cancellationToken);

        if (!payment.Success)
            return Redirect($"/order/failed?order={orderDto.OrderNumber}");

        return Redirect($"/payment/callback?order={orderDto.OrderNumber}&authority={payment.Authority}");
    }

    public IActionResult OnPostCancel(string order) => Redirect($"/order/failed?order={order}");
}
