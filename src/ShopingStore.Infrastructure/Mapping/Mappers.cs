using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;

namespace ShopingStore.Infrastructure.Mapping;

/// <summary>تبدیل موجودیت‌ها به DTOهای قابل استفاده در رابط کاربری.</summary>
public static class Mappers
{
    public const string PlaceholderImage = "/img/products/placeholder.svg";
    public const string Uncategorized = "دسته‌بندی نشده";

    public static ProductListItemDto ToListItem(this Product product, IReadOnlySet<int>? favoriteIds = null) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Slug = product.Slug,
        ShortDescription = product.ShortDescription,
        ImageUrl = string.IsNullOrWhiteSpace(product.MainImageUrl) ? PlaceholderImage : product.MainImageUrl,
        CategoryName = product.Category?.Name ?? Uncategorized,
        CategorySlug = product.Category?.Slug ?? string.Empty,
        Price = product.Price,
        EffectivePrice = product.EffectivePrice,
        DiscountPercent = product.DiscountPercent,
        HasDiscount = product.HasDiscount,
        Stock = product.Stock,
        Rating = product.Rating,
        ReviewsCount = product.ReviewsCount,
        IsNew = product.IsNew,
        IsFeatured = product.IsFeatured,
        IsFavorite = favoriteIds?.Contains(product.Id) ?? false
    };

    public static IReadOnlyList<ProductListItemDto> ToListItems(this IEnumerable<Product> products, IReadOnlySet<int>? favoriteIds = null)
        => products.Select(p => p.ToListItem(favoriteIds)).ToList();

    public static ProductDetailsDto ToDetails(
        this Product product,
        IEnumerable<Review> reviews,
        IEnumerable<Product> relatedProducts,
        bool isFavorite,
        IReadOnlySet<int>? favoriteIds = null)
    {
        var images = new List<string>();
        if (!string.IsNullOrWhiteSpace(product.MainImageUrl)) images.Add(product.MainImageUrl);
        images.AddRange(product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url));
        if (images.Count == 0) images.Add(PlaceholderImage);

        return new ProductDetailsDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Brand = product.Brand,
            Sku = product.Sku,
            Price = product.Price,
            EffectivePrice = product.EffectivePrice,
            DiscountPercent = product.DiscountPercent,
            HasDiscount = product.HasDiscount,
            Stock = product.Stock,
            Rating = product.Rating,
            ReviewsCount = product.ReviewsCount,
            IsNew = product.IsNew,
            ViewCount = product.ViewCount,
            SoldCount = product.SoldCount,
            CategoryName = product.Category?.Name ?? Uncategorized,
            CategorySlug = product.Category?.Slug ?? string.Empty,
            Images = images.Distinct().ToList(),
            Specifications = product.Specifications
                .Select(s => new ProductSpecificationDto { Name = s.Name, Value = s.Value })
                .ToList(),
            Variants = product.Variants
                .Select(v => new ProductVariantDto { Name = v.Name, Options = v.Options.ToList() })
                .ToList(),
            Reviews = reviews.ToList().ToDtos(),
            RelatedProducts = relatedProducts.ToListItems(favoriteIds),
            IsFavorite = isFavorite
        };
    }

    public static ReviewDto ToDto(this Review review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        ProductName = review.Product?.Name ?? string.Empty,
        ProductSlug = review.Product?.Slug ?? string.Empty,
        AuthorName = review.AuthorName,
        Title = review.Title,
        Comment = review.Comment,
        Rating = review.Rating,
        CreatedAtUtc = review.CreatedAtUtc,
        IsVerifiedPurchase = review.IsVerifiedPurchase,
        Status = review.Status,
        HelpfulCount = review.HelpfulCount
    };

    public static IReadOnlyList<ReviewDto> ToDtos(this IEnumerable<Review> reviews)
        => reviews.Select(r => r.ToDto()).ToList();

    public static CategoryDto ToDto(this Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        ImageUrl = category.ImageUrl,
        Icon = category.Icon,
        ProductsCount = category.Products?.Count ?? 0,
        Children = category.Children?.Select(c => c.ToDto()).ToList() ?? new List<CategoryDto>()
    };

    public static OrderDto ToDto(this Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        UserId = order.UserId,
        CustomerName = order.CustomerName,
        CustomerPhone = order.CustomerPhone,
        CustomerEmail = order.CustomerEmail,
        FullAddress = $"{order.Province} - {order.City} - {order.AddressLine} - کدپستی {order.PostalCode}",
        Subtotal = order.Subtotal,
        DiscountAmount = order.DiscountAmount,
        ShippingCost = order.ShippingCost,
        TaxAmount = order.TaxAmount,
        Total = order.Total,
        DiscountCode = order.DiscountCode,
        Status = order.Status,
        PaymentStatus = order.PaymentStatus,
        PaymentMethod = order.PaymentMethod,
        PaymentReference = order.PaymentReference,
        TrackingCode = order.TrackingCode,
        CustomerNote = order.CustomerNote,
        AdminNote = order.AdminNote,
        CreatedAtUtc = order.CreatedAtUtc,
        PaidAtUtc = order.PaidAtUtc,
        ShippedAtUtc = order.ShippedAtUtc,
        DeliveredAtUtc = order.DeliveredAtUtc,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductSlug = i.Product?.Slug ?? string.Empty,
            ProductImageUrl = string.IsNullOrWhiteSpace(i.ProductImageUrl) ? PlaceholderImage : i.ProductImageUrl,
            VariantSelection = i.VariantSelection,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList(),
        History = order.History.OrderByDescending(h => h.Id).Select(h => new OrderStatusHistoryDto
        {
            Status = h.Status,
            Note = h.Note,
            Actor = h.Actor,
            CreatedAtUtc = h.CreatedAtUtc
        }).ToList()
    };

    public static AddressDto ToDto(this Address address) => new()
    {
        Id = address.Id,
        Title = address.Title,
        ReceiverName = address.ReceiverName,
        PhoneNumber = address.PhoneNumber,
        Province = address.Province,
        City = address.City,
        PostalCode = address.PostalCode,
        Line = address.Line,
        IsDefault = address.IsDefault
    };

    public static UserProfileDto ToProfileDto(this User user, int ordersCount = 0, decimal totalSpent = 0) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        NationalCode = user.NationalCode,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAtUtc = user.CreatedAtUtc,
        LastLoginAtUtc = user.LastLoginAtUtc,
        OrdersCount = ordersCount,
        TotalSpent = totalSpent
    };

    public static DiscountCodeDto ToDto(this DiscountCode code) => new()
    {
        Id = code.Id,
        Code = code.Code,
        Description = code.Description,
        Type = code.Type,
        Amount = code.Amount,
        MinOrderAmount = code.MinOrderAmount,
        MaxDiscountAmount = code.MaxDiscountAmount,
        StartsAtUtc = code.StartsAtUtc,
        ExpiresAtUtc = code.ExpiresAtUtc,
        UsageLimit = code.UsageLimit,
        UsedCount = code.UsedCount,
        IsActive = code.IsActive
    };
}

/// <summary>محاسبه‌ی مشترک مبالغ سبد خرید و سفارش.</summary>
public static class CartPricing
{
    public static (decimal Shipping, decimal Tax, decimal Total) Calculate(StoreSettingsDto settings, decimal subtotal, decimal discount)
    {
        var payable = Math.Max(0, subtotal - discount);

        var shipping = payable == 0
            ? 0
            : (settings.FreeShippingThreshold > 0 && payable >= settings.FreeShippingThreshold ? 0 : settings.ShippingCost);

        var tax = Math.Round(payable * (decimal)settings.TaxPercent / 100m, 0, MidpointRounding.AwayFromZero);

        return (shipping, tax, payable + shipping + tax);
    }
}
