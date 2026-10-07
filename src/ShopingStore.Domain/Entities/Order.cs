using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public User? User { get; set; }

    // اطلاعات گیرنده (عکس لحظه‌ای از آدرس؛ مستقل از تغییرات آینده کاربر)
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    public string? DiscountCode { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;

    public string? PaymentReference { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    public string? TrackingCode { get; set; }
    public string? CustomerNote { get; set; }
    public string? AdminNote { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> History { get; set; } = new List<OrderStatusHistory>();

    public int TotalQuantity => Items.Sum(i => i.Quantity);

    /// <summary>محاسبه جمع‌های سفارش بر اساس اقلام و مقادیر تخفیف/مالیات/ارسال.</summary>
    public void RecalculateTotals(decimal shippingCost = 0, decimal taxPercent = 0)
    {
        Subtotal = Items.Sum(i => i.UnitPrice * i.Quantity);
        ShippingCost = shippingCost;
        TaxAmount = Math.Round(Subtotal * taxPercent / 100m, 0, MidpointRounding.AwayFromZero);
        Total = Math.Max(0, Subtotal - DiscountAmount + ShippingCost + TaxAmount);
    }

    public void MarkAsPaid(string paymentReference, DateTime? whenUtc = null)
    {
        PaymentStatus = PaymentStatus.Paid;
        PaymentReference = paymentReference;
        PaidAtUtc = whenUtc ?? DateTime.UtcNow;
        Status = OrderStatus.Paid;
        Touch();
    }

    public void ApplyDiscount(decimal amount, string? code)
    {
        DiscountAmount = Math.Clamp(amount, 0, Subtotal);
        DiscountCode = code;
    }

    public void Cancel(string? reason = null)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new BusinessException("سفارش ارسال‌شده یا تحویل‌شده قابل لغو نیست.");

        Status = OrderStatus.Canceled;
        PaymentStatus = PaymentStatus == PaymentStatus.Paid ? PaymentStatus.Refunded : PaymentStatus.Unpaid;
        AdminNote = string.IsNullOrWhiteSpace(reason) ? AdminNote : $"{AdminNote} | لغو: {reason}".Trim();
        Touch();
    }

    public void ChangeStatus(OrderStatus status, string? note = null, string? actor = null)
    {
        if (Status == status) return;

        Status = status;
        switch (status)
        {
            case OrderStatus.Shipped:
                ShippedAtUtc = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                DeliveredAtUtc = DateTime.UtcNow;
                if (PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    PaymentStatus = PaymentStatus.Paid;
                    PaidAtUtc = DateTime.UtcNow;
                }
                break;
            case OrderStatus.Canceled:
                PaymentStatus = PaymentStatus == PaymentStatus.Paid ? PaymentStatus.Refunded : PaymentStatus.Unpaid;
                break;
        }

        History.Add(new OrderStatusHistory
        {
            OrderId = Id,
            Status = status,
            Note = note,
            Actor = actor
        });

        Touch();
    }
}

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>نام محصول در لحظه خرید (در صورت تغییر نام محصول در آینده).</summary>
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string? VariantSelection { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal LineTotal => UnitPrice * Quantity;

    public static OrderItem FromProduct(Product product, int quantity, string? variant = null) => new()
    {
        ProductId = product.Id,
        ProductName = product.Name,
        ProductImageUrl = product.MainImageUrl,
        UnitPrice = product.EffectivePrice,
        Quantity = quantity,
        VariantSelection = variant
    };
}

public class OrderStatusHistory : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
    public string? Actor { get; set; }
}
