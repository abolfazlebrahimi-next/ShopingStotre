using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

/// <summary>
/// سبد خرید. برای کاربران مهمان با <see cref="CartKey"/> (کوکی مرورگر) و برای کاربران
/// وارد‌شده با <see cref="UserId"/> شناسایی می‌شود.
/// </summary>
public class Cart : BaseEntity
{
    public string CartKey { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>کد تخفیف اعمال‌شده روی سبد (اختیاری).</summary>
    public string? DiscountCode { get; set; }

    public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    public int TotalQuantity => Items.Sum(i => i.Quantity);

    public decimal Subtotal => Items.Sum(i => (i.Product?.EffectivePrice ?? 0) * i.Quantity);
}
