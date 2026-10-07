using Microsoft.Extensions.Logging;
using ShopingStore.Domain.Interfaces.Repositories;

namespace ShopingStore.Infrastructure.Payment;

/// <summary>
/// درگاه پرداخت آزمایشی (شبیه‌ساز).
/// برای اتصال درگاه واقعی (زرین‌پال، ملت، سامان و ...) کافی است یک کلاس جدید از
/// <see cref="IPaymentGateway"/> بسازید و در Program.cs ثبت آن را تغییر دهید.
/// </summary>
public class DemoPaymentGateway : IPaymentGateway
{
    private readonly ILogger<DemoPaymentGateway> _logger;

    public DemoPaymentGateway(ILogger<DemoPaymentGateway> logger) => _logger = logger;

    public Task<PaymentRequestResult> RequestPaymentAsync(decimal amount, string orderNumber, string callbackUrl, CancellationToken ct = default)
    {
        var authority = Guid.NewGuid().ToString("N")[..20].ToUpperInvariant();
        _logger.LogInformation("درخواست پرداخت آزمایشی: سفارش {OrderNumber} مبلغ {Amount} توکن {Authority}",
            orderNumber, amount, authority);

        var separator = callbackUrl.Contains('?') ? "&" : "?";
        var redirectUrl = $"{callbackUrl}{separator}authority={authority}&order={orderNumber}&amount={amount:0}&status=OK";

        return Task.FromResult(new PaymentRequestResult
        {
            Success = true,
            Authority = authority,
            RedirectUrl = redirectUrl
        });
    }

    public Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, decimal amount, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(authority))
            return Task.FromResult(new PaymentVerifyResult { Success = false, Error = "شناسه پرداخت نامعتبر است." });

        _logger.LogInformation("تأیید پرداخت آزمایشی با توکن {Authority} و مبلغ {Amount}", authority, amount);

        return Task.FromResult(new PaymentVerifyResult
        {
            Success = true,
            ReferenceId = $"DEMO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}"
        });
    }
}
