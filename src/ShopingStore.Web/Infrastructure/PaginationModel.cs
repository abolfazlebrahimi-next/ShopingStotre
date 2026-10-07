namespace ShopingStore.Web.Infrastructure;

/// <summary>مدل کمکی برای نمایش دکمه‌های صفحه‌بندی.</summary>
public class PaginationModel
{
    public int Page { get; init; } = 1;
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public string BasePath { get; init; } = string.Empty;
    public string PageParameter { get; init; } = "page";

    /// <summary>پارامترهای فعلی آدرس تا با تغییر صفحه حفظ شوند.</summary>
    public Microsoft.AspNetCore.Http.IQueryCollection? Query { get; init; }
}

public static class QueryStringHelper
{
    /// <summary>ساخت آدرس با حفظ پارامترهای فعلی و تغییر یک پارامتر.</summary>
    public static string Build(string path, IQueryCollection query, string key, object? value)
    {
        var parts = new List<string>();

        foreach (var item in query)
        {
            if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrEmpty(item.Value)) continue;
            parts.Add($"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value.ToString())}");
        }

        if (value is not null && !string.IsNullOrEmpty(value.ToString()))
            parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value.ToString()!)}");

        return parts.Count == 0 ? path : $"{path}?{string.Join('&', parts)}";
    }
}
