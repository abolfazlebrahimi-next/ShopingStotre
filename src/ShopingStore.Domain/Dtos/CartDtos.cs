namespace ShopingStore.Domain.Dtos;

public class CartDto
{
    public IReadOnlyList<CartItemDto> Items { get; init; } = Array.Empty<CartItemDto>();
    public string? DiscountCode { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
    public decimal FreeShippingRemaining { get; init; }

    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public bool IsEmpty => Items.Count == 0;
}

public class CartItemDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSlug { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal OriginalPrice { get; init; }
    public int Quantity { get; init; }
    public int Stock { get; init; }
    public string? VariantSelection { get; init; }
    public decimal LineTotal => UnitPrice * Quantity;
    public int MaxQuantity => Math.Max(1, Math.Min(Stock, 10));
}
