using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages;

public class ContactModel : PageModel
{
    private readonly ISettingsService _settings;
    private readonly INotificationService _notifications;
    private readonly ToastService _toast;

    public ContactModel(ISettingsService settings, INotificationService notifications, ToastService toast)
    {
        _settings = settings;
        _notifications = notifications;
        _toast = toast;
    }

    public StoreSettingsDto Settings { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Settings = await _settings.GetAsync(cancellationToken);

    public async Task<IActionResult> OnPostAsync(string name, string phone, string? email, string subject, string message, CancellationToken cancellationToken)
    {
        try
        {
            var sender = Guard.NotEmpty(name, "نام", 150);
            var phoneNumber = Guard.Mobile(phone);
            var title = Guard.NotEmpty(subject, "موضوع", 200);
            var body = Guard.NotEmpty(message, "متن پیام", 2000);

            var settings = await _settings.GetAsync(cancellationToken);

            await _notifications.SendEmailAsync(
                settings.SupportEmail,
                $"پیام جدید از فرم تماس: {title}",
                $"فرستنده: {sender}\nتلفن: {phoneNumber}\nایمیل: {email}\n\n{body}",
                cancellationToken);

            _toast.Success("پیام شما ثبت شد. کارشناسان ما به‌زودی پاسخ می‌دهند.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }
}
