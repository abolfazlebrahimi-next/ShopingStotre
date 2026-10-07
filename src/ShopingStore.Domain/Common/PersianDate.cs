using System.Globalization;
using System.Text;

namespace ShopingStore.Domain.Common;

/// <summary>
/// کمک‌کار تاریخ شمسی (جلالی) بر پایه <see cref="PersianCalendar"/> خود دات‌نت.
/// </summary>
public static class PersianDate
{
    private static readonly PersianCalendar Calendar = new();

    private static readonly string[] MonthNames =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    private static readonly string[] DayNames =
    {
        "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"
    };

    /// <summary>تبدیل تاریخ میلادی به (سال، ماه، روز) شمسی.</summary>
    public static (int Year, int Month, int Day, int Hour, int Minute) ToPersian(DateTime dateTime)
    {
        var local = dateTime.Kind == DateTimeKind.Utc ? dateTime.ToLocalTime() : dateTime;
        return (
            Calendar.GetYear(local),
            Calendar.GetMonth(local),
            Calendar.GetDayOfMonth(local),
            local.Hour,
            local.Minute);
    }

    /// <summary>تبدیل تاریخ شمسی به میلادی.</summary>
    public static DateTime ToGregorian(int year, int month, int day)
        => Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);

    public static string MonthName(int month) => MonthNames[Math.Clamp(month, 1, 12) - 1];

    /// <summary>مثال: ۱۵ مهر ۱۴۰۵</summary>
    public static string ToPersianDate(DateTime? dateTime)
    {
        if (dateTime is null) return "-";
        var (y, m, d, _, _) = ToPersian(dateTime.Value);
        return $"{ToPersianDigits(d.ToString())} {MonthName(m)} {ToPersianDigits(y.ToString())}";
    }

    /// <summary>مثال: ۱۵ مهر ۱۴۰۵ - ۱۴:۳۰</summary>
    public static string ToPersianDateTime(DateTime? dateTime)
    {
        if (dateTime is null) return "-";
        var (y, m, d, h, mi) = ToPersian(dateTime.Value);
        return $"{ToPersianDigits(d.ToString())} {MonthName(m)} {ToPersianDigits(y.ToString())} - {ToPersianDigits($"{h:00}:{mi:00}")}";
    }

    /// <summary>مثال: پنجشنبه ۱۵ مهر ۱۴۰۵</summary>
    public static string ToPersianFullDate(DateTime? dateTime)
    {
        if (dateTime is null) return "-";
        var local = dateTime.Value.Kind == DateTimeKind.Utc ? dateTime.Value.ToLocalTime() : dateTime.Value;
        return $"{DayNames[(int)local.DayOfWeek]} {ToPersianDate(dateTime)}";
    }

    /// <summary>مثال: ۱۴۰۵/۰۷/۱۵</summary>
    public static string ToShortPersian(DateTime? dateTime)
    {
        if (dateTime is null) return "-";
        var (y, m, d, _, _) = ToPersian(dateTime.Value);
        return ToPersianDigits($"{y:0000}/{m:00}/{d:00}");
    }

    /// <summary>مثال: ۳ روز پیش / ۵ ساعت پیش / همین حالا</summary>
    public static string ToRelative(DateTime? dateTimeUtc)
    {
        if (dateTimeUtc is null) return "-";
        var diff = DateTime.UtcNow - (dateTimeUtc.Value.Kind == DateTimeKind.Utc ? dateTimeUtc.Value : dateTimeUtc.Value.ToUniversalTime());

        if (diff.TotalSeconds < 60) return "همین حالا";
        if (diff.TotalMinutes < 60) return $"{ToPersianDigits((int)diff.TotalMinutes)} دقیقه پیش";
        if (diff.TotalHours < 24) return $"{ToPersianDigits((int)diff.TotalHours)} ساعت پیش";
        if (diff.TotalDays < 31) return $"{ToPersianDigits((int)diff.TotalDays)} روز پیش";
        if (diff.TotalDays < 365) return $"{ToPersianDigits((int)(diff.TotalDays / 30))} ماه پیش";
        return $"{ToPersianDigits((int)(diff.TotalDays / 365))} سال پیش";
    }

    /// <summary>تبدیل ارقام لاتین به ارقام فارسی.</summary>
    public static string ToPersianDigits(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var builder = new StringBuilder(input.Length);
        foreach (var ch in input)
            builder.Append(ch is >= '0' and <= '9' ? (char)(ch - '0' + '\u06F0') : ch);
        return builder.ToString();
    }

    /// <summary>نمایش قیمت به‌صورت «۱٬۲۵۰٬۰۰۰ تومان».</summary>
    public static string ToPrice(decimal amount, string? currency = "تومان")
    {
        var formatted = amount.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", "٬");
        return string.IsNullOrWhiteSpace(currency) ? ToPersianDigits(formatted) : $"{ToPersianDigits(formatted)} {currency}";
    }

    /// <summary>نام روز هفته به فارسی.</summary>
    public static string DayName(DateTime date) => DayNames[(int)date.DayOfWeek];
}
