using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Dtos;

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; init; }
    public decimal TodayRevenue { get; init; }
    public decimal MonthRevenue { get; init; }
    public int TotalOrders { get; init; }
    public int TodayOrders { get; init; }
    public int PendingOrders { get; init; }
    public int ProcessingOrders { get; init; }
    public int TotalProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public int TotalCustomers { get; init; }
    public int NewCustomersThisMonth { get; init; }
    public int TotalReviews { get; init; }
    public int PendingReviews { get; init; }
    public double AverageOrderValue { get; init; }
    public IReadOnlyList<SalesChartPointDto> SalesChart { get; init; } = Array.Empty<SalesChartPointDto>();
    public IReadOnlyList<CategorySalesDto> SalesByCategory { get; init; } = Array.Empty<CategorySalesDto>();
    public IReadOnlyList<TopProductDto> TopProducts { get; init; } = Array.Empty<TopProductDto>();
    public IReadOnlyList<OrderDto> LatestOrders { get; init; } = Array.Empty<OrderDto>();
    public IReadOnlyList<LowStockProductDto> LowStockProducts { get; init; } = Array.Empty<LowStockProductDto>();
}

public class SalesChartPointDto
{
    public DateTime DateUtc { get; init; }
    public string Label { get; init; } = string.Empty;
    public int OrdersCount { get; init; }
    public decimal Revenue { get; init; }
}

public class CategorySalesDto
{
    public string CategoryName { get; init; } = string.Empty;
    public int ItemsSold { get; init; }
    public decimal Revenue { get; init; }
}

public class TopProductDto
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int SoldCount { get; init; }
    public decimal Revenue { get; init; }
}

public class LowStockProductDto
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Stock { get; init; }
}

public class SaveProductRequest
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public string? MainImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<ProductSpecificationDto> Specifications { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class SaveCategoryRequest
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DiscountCodeDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DiscountType Type { get; init; }
    public decimal Amount { get; init; }
    public decimal MinOrderAmount { get; init; }
    public decimal? MaxDiscountAmount { get; init; }
    public DateTime? StartsAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public int? UsageLimit { get; init; }
    public int UsedCount { get; init; }
    public bool IsActive { get; init; }
    public string AmountLabel => Type == DiscountType.Percentage
        ? $"{PersianDate.ToPersianDigits(Amount.ToString("0.##"))}٪"
        : PersianDate.ToPrice(Amount);
}

public class SaveDiscountCodeRequest
{
    public int? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType Type { get; set; } = DiscountType.Percentage;
    public decimal Amount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public int? UsageLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class StoreSettingsDto
{
    public string SiteName { get; set; } = "فروشگاه آنلاین شاپینگ‌استور";
    public string SiteSlogan { get; set; } = "خرید آسان، ارسال سریع";
    public string SupportPhone { get; set; } = "۰۲۱-۹۱۰۰۰۰۰۰";
    public string SupportEmail { get; set; } = "support@shopingstore.ir";
    public string Address { get; set; } = "تهران، خیابان ولیعصر، پلاک ۱۲۳";
    public string Instagram { get; set; } = "https://instagram.com/";
    public string Telegram { get; set; } = "https://t.me/";
    public decimal ShippingCost { get; set; } = 49_000;
    public decimal FreeShippingThreshold { get; set; } = 2_000_000;
    public double TaxPercent { get; set; } = 9;
    public string BankName { get; set; } = "بانک ملت";
    public string CardNumber { get; set; } = "6104-3378-1234-5678";
    public string CardOwner { get; set; } = "مدیر فروشگاه";
    public decimal CashOnDeliveryFee { get; set; } = 25_000;
}
