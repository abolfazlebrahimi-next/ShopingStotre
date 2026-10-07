using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

/// <summary>علاقه‌مندی‌های کاربر (لیست آرزو).</summary>
public class Favorite : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
