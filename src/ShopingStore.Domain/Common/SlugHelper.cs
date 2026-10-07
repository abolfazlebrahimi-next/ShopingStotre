using System.Globalization;
using System.Text;

namespace ShopingStore.Domain.Common;

/// <summary>ساخت «نامک» (slug) سازگار با آدرس صفحات فارسی.</summary>
public static class SlugHelper
{
    public static string Generate(string? title, string fallbackPrefix = "item")
    {
        if (string.IsNullOrWhiteSpace(title))
            return $"{fallbackPrefix}-{DateTime.UtcNow.Ticks.ToString()[^6..]}";

        var normalized = title.Trim().ToLowerInvariant()
            .Replace('ي', 'ی')
            .Replace('ك', 'ک')
            .Replace('ۀ', 'ه')
            .Replace('ة', 'ه')
            .Replace('\u200c', '-'); // نیم‌فاصله

        var builder = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch)) builder.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_' or '.' or '/' or '\\' or ':' or '،' or '؟' or '!' or '(' or ')' or '«' or '»') builder.Append('-');
        }

        var slug = builder.ToString();
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrWhiteSpace(slug)) slug = fallbackPrefix;
        if (slug.Length > 120) slug = slug[..120].Trim('-');
        return slug;
    }

    public static string NormalizePersianDigits(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var builder = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            builder.Append(ch switch
            {
                >= '\u06F0' and <= '\u06F9' => (char)(ch - '\u06F0' + '0'), // ۰-۹
                >= '\u0660' and <= '\u0669' => (char)(ch - '\u0660' + '0'), // ٠-٩
                _ => ch
            });
        }
        return builder.ToString();
    }
}
