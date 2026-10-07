using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Dtos;

public class CheckoutRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int? AddressId { get; set; }
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;
    public string? DiscountCode { get; set; }
    public string? CustomerNote { get; set; }
    public bool SaveAddress { get; set; }
}

public class OrderDto
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public string? CustomerEmail { get; init; }
    public string FullAddress { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
    public string? DiscountCode { get; init; }
    public OrderStatus Status { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? PaymentReference { get; init; }
    public string? TrackingCode { get; init; }
    public string? CustomerNote { get; init; }
    public string? AdminNote { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? PaidAtUtc { get; init; }
    public DateTime? ShippedAtUtc { get; init; }
    public DateTime? DeliveredAtUtc { get; init; }
    public IReadOnlyList<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();
    public IReadOnlyList<OrderStatusHistoryDto> History { get; init; } = Array.Empty<OrderStatusHistoryDto>();
    public int TotalQuantity => Items.Sum(i => i.Quantity);
}

public class OrderItemDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSlug { get; init; } = string.Empty;
    public string? ProductImageUrl { get; init; }
    public string? VariantSelection { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class OrderStatusHistoryDto
{
    public OrderStatus Status { get; init; }
    public string? Note { get; init; }
    public string? Actor { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public class AdminOrderFilter
{
    public string? Search { get; set; }
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

public class UpdateOrderStatusRequest
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? TrackingCode { get; set; }
    public string? Note { get; set; }
}
