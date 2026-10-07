using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

/// <summary>تنظیمات قابل ویرایش از پنل مدیریت (کلید/مقدار).</summary>
public class Setting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
}

public static class SettingKeys
{
    public const string SiteName = "Site.Name";
    public const string SiteSlogan = "Site.Slogan";
    public const string SupportPhone = "Site.SupportPhone";
    public const string SupportEmail = "Site.SupportEmail";
    public const string Address = "Site.Address";
    public const string Instagram = "Site.Instagram";
    public const string Telegram = "Site.Telegram";
    public const string ShippingCost = "Shop.ShippingCost";
    public const string FreeShippingThreshold = "Shop.FreeShippingThreshold";
    public const string TaxPercent = "Shop.TaxPercent";
    public const string BankName = "Payment.BankName";
    public const string CardNumber = "Payment.CardNumber";
    public const string CardOwner = "Payment.CardOwner";
    public const string CashOnDeliveryFee = "Payment.CashOnDeliveryFee";
}
