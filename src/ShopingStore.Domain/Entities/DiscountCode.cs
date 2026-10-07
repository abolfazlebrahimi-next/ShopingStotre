using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

public class DiscountCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DiscountType Type { get; set; } = DiscountType.Percentage;

    /// <summary>برای نوع درصدی: عدد بین ۱ تا ۱۰۰ / برای مبلغ ثابت: مبلغ به تومان.</summary>
    public decimal Amount { get; set; }

    /// <summary>حداقل مبلغ سبد خرید برای اعمال کد.</summary>
    public decimal MinOrderAmount { get; set; }

    /// <summary>سقف تخفیف برای کدهای درصدی (اختیاری).</summary>
    public decimal? MaxDiscountAmount { get; set; }

    public DateTime? StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>حداکثر تعداد استفاده (اختیاری).</summary>
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsUsable(DateTime utcNow, decimal cartSubtotal, out string? error)
    {
        error = null;

        if (!IsActive)
        {
            error = "این کد تخفیف غیرفعال است.";
            return false;
        }

        if (StartsAtUtc is not null && StartsAtUtc > utcNow)
        {
            error = "این کد تخفیف هنوز فعال نشده است.";
            return false;
        }

        if (ExpiresAtUtc is not null && ExpiresAtUtc < utcNow)
        {
            error = "این کد تخفیف منقضی شده است.";
            return false;
        }

        if (UsageLimit is > 0 && UsedCount >= UsageLimit)
        {
            error = "ظرفیت استفاده از این کد تخفیف تکمیل شده است.";
            return false;
        }

        if (cartSubtotal < MinOrderAmount)
        {
            error = $"حداقل مبلغ سفارش برای این کد {PersianDate.ToPrice(MinOrderAmount)} است.";
            return false;
        }

        return true;
    }

    /// <summary>محاسبه مبلغ تخفیف روی جمع سبد خرید.</summary>
    public decimal CalculateDiscount(decimal cartSubtotal)
    {
        if (cartSubtotal <= 0) return 0;

        var discount = Type == DiscountType.Percentage
            ? cartSubtotal * Amount / 100m
            : Amount;

        if (MaxDiscountAmount is > 0)
            discount = Math.Min(discount, MaxDiscountAmount.Value);

        return Math.Min(Math.Round(discount, 0, MidpointRounding.AwayFromZero), cartSubtotal);
    }

    public string AmountLabel => Type == DiscountType.Percentage
        ? $"{PersianDate.ToPersianDigits(Amount.ToString("0.##"))}٪"
        : PersianDate.ToPrice(Amount);
}
