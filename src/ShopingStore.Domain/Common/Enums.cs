namespace ShopingStore.Domain.Common;

public enum UserRole
{
    Customer = 0,
    Support = 1,
    Admin = 2
}

public enum OrderStatus
{
    Pending = 0,        // در انتظار پرداخت
    Paid = 1,           // پرداخت شده
    Processing = 2,     // در حال پردازش
    Shipped = 3,        // ارسال شده
    Delivered = 4,      // تحویل داده شده
    Canceled = 5,       // لغو شده
    Returned = 6        // مرجوع شده
}

public enum PaymentStatus
{
    Unpaid = 0,
    Paid = 1,
    Refunded = 2,
    Failed = 3
}

public enum PaymentMethod
{
    Online = 0,      // پرداخت اینترنتی (درگاه)
    CashOnDelivery = 1,
    Wallet = 2
}

public enum DiscountType
{
    Percentage = 0,  // درصدی
    FixedAmount = 1  // مبلغ ثابت
}

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum ProductSortBy
{
    Newest = 0,
    Cheapest = 1,
    MostExpensive = 2,
    BestSelling = 3,
    TopRated = 4,
    MostDiscounted = 5
}

public static class OrderStatusExtensions
{
    public static string ToPersianTitle(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "در انتظار پرداخت",
        OrderStatus.Paid => "پرداخت شده",
        OrderStatus.Processing => "در حال پردازش",
        OrderStatus.Shipped => "ارسال شده",
        OrderStatus.Delivered => "تحویل داده شده",
        OrderStatus.Canceled => "لغو شده",
        OrderStatus.Returned => "مرجوع شده",
        _ => "نامشخص"
    };

    public static string ToCssClass(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "warning",
        OrderStatus.Paid => "info",
        OrderStatus.Processing => "primary",
        OrderStatus.Shipped => "info",
        OrderStatus.Delivered => "success",
        OrderStatus.Canceled => "danger",
        OrderStatus.Returned => "secondary",
        _ => "secondary"
    };

    public static string ToPersianTitle(this PaymentStatus status) => status switch
    {
        PaymentStatus.Unpaid => "پرداخت نشده",
        PaymentStatus.Paid => "پرداخت شده",
        PaymentStatus.Refunded => "بازگشت وجه",
        PaymentStatus.Failed => "ناموفق",
        _ => "نامشخص"
    };

    public static string ToPersianTitle(this PaymentMethod method) => method switch
    {
        PaymentMethod.Online => "پرداخت اینترنتی",
        PaymentMethod.CashOnDelivery => "پرداخت در محل",
        PaymentMethod.Wallet => "کیف پول",
        _ => "نامشخص"
    };

    public static string ToPersianTitle(this DiscountType type) =>
        type == DiscountType.Percentage ? "درصدی" : "مبلغ ثابت";

    public static string ToPersianTitle(this UserRole role) => role switch
    {
        UserRole.Admin => "مدیر سیستم",
        UserRole.Support => "پشتیبان",
        _ => "مشتری"
    };
}
