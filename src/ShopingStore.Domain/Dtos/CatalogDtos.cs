using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Dtos;

/// <summary>پارامترهای جست‌وجو و فیلتر محصولات (از QueryString صفحه محصولات).</summary>
public class ProductQuery
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool OnlyAvailable { get; set; }
    public bool OnlyDiscounted { get; set; }

    /// <summary>در پنل مدیریت، محصولات غیرفعال هم نمایش داده شوند.</summary>
    public bool IncludeInactive { get; set; }
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Newest;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? ImageUrl { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategorySlug { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal EffectivePrice { get; init; }
    public int DiscountPercent { get; init; }
    public bool HasDiscount { get; init; }
    public int Stock { get; init; }
    public double Rating { get; init; }
    public int ReviewsCount { get; init; }
    public bool IsNew { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsFavorite { get; init; }
}

public class ProductDetailsDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public string? Brand { get; init; }
    public string? Sku { get; init; }
    public decimal Price { get; init; }
    public decimal EffectivePrice { get; init; }
    public int DiscountPercent { get; init; }
    public bool HasDiscount { get; init; }
    public int Stock { get; init; }
    public double Rating { get; init; }
    public int ReviewsCount { get; init; }
    public bool IsNew { get; init; }
    public int ViewCount { get; init; }
    public int SoldCount { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategorySlug { get; init; } = string.Empty;
    public IReadOnlyList<string> Images { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ProductSpecificationDto> Specifications { get; init; } = Array.Empty<ProductSpecificationDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; init; } = Array.Empty<ProductVariantDto>();
    public IReadOnlyList<ReviewDto> Reviews { get; init; } = Array.Empty<ReviewDto>();
    public IReadOnlyList<ProductListItemDto> RelatedProducts { get; init; } = Array.Empty<ProductListItemDto>();
    public bool IsFavorite { get; init; }
}

public class ProductSpecificationDto
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public class ProductVariantDto
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();
}

public class CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? Icon { get; init; }
    public int ProductsCount { get; init; }
    public IReadOnlyList<CategoryDto> Children { get; init; } = Array.Empty<CategoryDto>();
}

/// <summary>داده‌های صفحه اصلی فروشگاه.</summary>
public class HomePageDto
{
    public IReadOnlyList<CategoryDto> Categories { get; init; } = Array.Empty<CategoryDto>();
    public IReadOnlyList<ProductListItemDto> FeaturedProducts { get; init; } = Array.Empty<ProductListItemDto>();
    public IReadOnlyList<ProductListItemDto> NewestProducts { get; init; } = Array.Empty<ProductListItemDto>();
    public IReadOnlyList<ProductListItemDto> BestSellers { get; init; } = Array.Empty<ProductListItemDto>();
    public IReadOnlyList<ProductListItemDto> DiscountedProducts { get; init; } = Array.Empty<ProductListItemDto>();
    public IReadOnlyList<ReviewDto> LatestReviews { get; init; } = Array.Empty<ReviewDto>();
    public int ProductsCount { get; init; }
    public int CustomersCount { get; init; }
    public int OrdersCount { get; init; }
}

public class ReviewDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSlug { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string Comment { get; init; } = string.Empty;
    public int Rating { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public ReviewStatus Status { get; init; }
    public int HelpfulCount { get; init; }
}
