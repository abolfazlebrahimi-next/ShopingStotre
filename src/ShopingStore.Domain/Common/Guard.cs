using System.Text.RegularExpressions;

namespace ShopingStore.Domain.Common;

/// <summary>اعتبارسنجی‌های پرکاربرد.</summary>
public static class Guard
{
    public static string NotEmpty(string? value, string fieldTitle, int maxLength = 250)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException($"{fieldTitle} را وارد کنید.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new BusinessException($"{fieldTitle} نمی‌تواند بیشتر از {maxLength} کاراکتر باشد.");

        return trimmed;
    }

    public static string? Optional(string? value, int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }

    public static string Email(string? value)
    {
        var email = NotEmpty(value, "ایمیل", 150);
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$"))
            throw new BusinessException("ایمیل وارد شده معتبر نیست.");
        return email.ToLowerInvariant();
    }

    public static string Mobile(string? value)
    {
        var phone = NotEmpty(value, "شماره موبایل", 20)
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);

        phone = phone.Replace("+98", "0").Replace("0098", "0");
        if (!Regex.IsMatch(phone, @"^09\d{9}$"))
            throw new BusinessException("شماره موبایل باید با ۰۹ شروع شده و ۱۱ رقم باشد.");

        return phone;
    }

    public static decimal Positive(decimal value, string fieldTitle)
    {
        if (value <= 0) throw new BusinessException($"{fieldTitle} باید بزرگ‌تر از صفر باشد.");
        return value;
    }

    public static decimal NotNegative(decimal value, string fieldTitle)
    {
        if (value < 0) throw new BusinessException($"{fieldTitle} نمی‌تواند منفی باشد.");
        return value;
    }

    public static int InRange(int value, int min, int max, string fieldTitle)
    {
        if (value < min || value > max)
            throw new BusinessException($"{fieldTitle} باید بین {min} و {max} باشد.");
        return value;
    }
}
