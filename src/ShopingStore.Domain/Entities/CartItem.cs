using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; } = 1;

    /// <summary>انتخاب تنوع محصول (مثلاً «رنگ: مشکی»).</summary>
    public string? VariantSelection { get; set; }

    public decimal LineTotal => (Product?.EffectivePrice ?? 0) * Quantity;
}
