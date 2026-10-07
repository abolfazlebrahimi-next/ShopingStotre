namespace ShopingStore.Domain.Common;

/// <summary>ساخت شماره سفارش خوانا مانند ۱۴۰۵۰۷۱۵-۴۸۲۱.</summary>
public static class OrderNumberGenerator
{
    public static string Generate(DateTime? now = null)
    {
        var utcNow = now ?? DateTime.UtcNow;
        var random = Random.Shared.Next(1000, 9999);
        return $"{utcNow:yyyyMMdd}-{random}";
    }
}
