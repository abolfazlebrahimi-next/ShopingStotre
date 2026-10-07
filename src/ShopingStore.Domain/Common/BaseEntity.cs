namespace ShopingStore.Domain.Common;

/// <summary>
/// کلاس پایه تمام موجودیت‌ها (کلید اصلی + تاریخ‌های سیستمی).
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    /// <summary>زمان ایجاد رکورد (UTC).</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>زمان آخرین ویرایش رکورد (UTC).</summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>حذف نرم؛ رکوردهای حذف‌شده در کوئری‌های فروشگاه نمایش داده نمی‌شوند.</summary>
    public bool IsDeleted { get; set; }

    public void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
