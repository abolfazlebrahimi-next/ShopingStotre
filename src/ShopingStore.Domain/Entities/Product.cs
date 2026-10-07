using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    public string? Brand { get; set; }
    public string? Sku { get; set; }

    public decimal Price { get; set; }

    /// <summary>قیمت قبل از تخفیف؛ اگر خالی باشد یعنی محصول تخفیف ندارد.</summary>
    public decimal? DiscountPrice { get; set; }

    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }

    public string? MainImageUrl { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int ViewCount { get; set; }
    public int SoldCount { get; set; }

    /// <summary>میانگین امتیاز (با هر تأیید نظر به‌روزرسانی می‌شود).</summary>
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }

    public List<ProductSpecification> Specifications { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    // ---------------------------------------------------------------- محاسبات
    public decimal EffectivePrice => DiscountPrice is > 0 and var discount && discount < Price ? discount.Value : Price;

    public decimal DiscountAmount => Price - EffectivePrice;

    public int DiscountPercent => Price <= 0 || DiscountAmount <= 0
        ? 0
        : (int)Math.Round(DiscountAmount / Price * 100, MidpointRounding.AwayFromZero);

    public bool HasDiscount => DiscountAmount > 0;

    public bool IsInStock => Stock > 0;

    public string StockLabel => Stock switch
    {
        <= 0 => "ناموجود",
        <= 5 => $"تنها {PersianDate.ToPersianDigits(Stock.ToString())} عدد در انبار",
        _ => "موجود در انبار"
    };
}

/// <summary>ویژگی فنی محصول (نام/مقدار) که به‌صورت ستون JSON ذخیره می‌شود.</summary>
public class ProductSpecification
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>تنوع محصول (مثلاً رنگ: مشکی، سفید).</summary>
public class ProductVariant
{
    public string Name { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public int SortOrder { get; set; }
}
