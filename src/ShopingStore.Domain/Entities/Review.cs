using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

public class Review : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>نام نمایشی نویسنده نظر (برای کاربران مهمان).</summary>
    public string AuthorName { get; set; } = string.Empty;

    public string? Title { get; set; }
    public string Comment { get; set; } = string.Empty;

    /// <summary>امتیاز بین ۱ تا ۵.</summary>
    public int Rating { get; set; } = 5;

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public bool IsVerifiedPurchase { get; set; }

    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
}
