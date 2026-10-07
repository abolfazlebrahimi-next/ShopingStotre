using Microsoft.Extensions.Logging;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Infrastructure.Services;

/// <summary>
/// سرویس اطلاع‌رسانی: پیام‌ها در لاگ ثبت می‌شوند.
/// برای ارسال واقعی پیامک/ایمیل کافی است همین کلاس را با سرویس‌دهنده دلخواه (کاوه‌نگار، ملی‌پیامک، SMTP و ...) جایگزین کنید.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger) => _logger = logger;

    public Task SendOrderConfirmationAsync(OrderDto order, CancellationToken ct = default)
    {
        var summary = string.Join("، ", order.Items.Select(i => $"{i.ProductName} × {PersianDate.ToPersianDigits(i.Quantity.ToString())}"));

        _logger.LogInformation(
            "اطلاع‌رسانی سفارش {OrderNumber} | مشتری: {Customer} | تلفن: {Phone} | مبلغ: {Total} تومان | اقلام: {Items}",
            order.OrderNumber, order.CustomerName, order.CustomerPhone, order.Total, summary);

        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("پیامک به {Phone}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string email, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("ایمیل به {Email} | موضوع: {Subject} | متن: {Body}", email, subject, body);
        return Task.CompletedTask;
    }
}
