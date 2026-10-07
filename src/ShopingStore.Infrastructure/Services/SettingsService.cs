using System.Globalization;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Infrastructure.Services;

/// <summary>خواندن/نوشتن تنظیمات فروشگاه از جدول Settings با مقادیر پیش‌فرض.</summary>
public class SettingsService : ISettingsService
{
    private readonly ISettingRepository _settings;
    private readonly IUnitOfWork _uow;

    public SettingsService(ISettingRepository settings, IUnitOfWork uow)
    {
        _settings = settings;
        _uow = uow;
    }

    public async Task<StoreSettingsDto> GetAsync(CancellationToken ct = default)
    {
        var values = await _settings.GetAllAsync(ct);
        var defaults = new StoreSettingsDto();

        return new StoreSettingsDto
        {
            SiteName = GetString(values, SettingKeys.SiteName, defaults.SiteName),
            SiteSlogan = GetString(values, SettingKeys.SiteSlogan, defaults.SiteSlogan),
            SupportPhone = GetString(values, SettingKeys.SupportPhone, defaults.SupportPhone),
            SupportEmail = GetString(values, SettingKeys.SupportEmail, defaults.SupportEmail),
            Address = GetString(values, SettingKeys.Address, defaults.Address),
            Instagram = GetString(values, SettingKeys.Instagram, defaults.Instagram),
            Telegram = GetString(values, SettingKeys.Telegram, defaults.Telegram),
            ShippingCost = GetDecimal(values, SettingKeys.ShippingCost, defaults.ShippingCost),
            FreeShippingThreshold = GetDecimal(values, SettingKeys.FreeShippingThreshold, defaults.FreeShippingThreshold),
            TaxPercent = GetDouble(values, SettingKeys.TaxPercent, defaults.TaxPercent),
            BankName = GetString(values, SettingKeys.BankName, defaults.BankName),
            CardNumber = GetString(values, SettingKeys.CardNumber, defaults.CardNumber),
            CardOwner = GetString(values, SettingKeys.CardOwner, defaults.CardOwner),
            CashOnDeliveryFee = GetDecimal(values, SettingKeys.CashOnDeliveryFee, defaults.CashOnDeliveryFee)
        };
    }

    public async Task SaveAsync(StoreSettingsDto settings, CancellationToken ct = default)
    {
        await _settings.SetManyAsync(new Dictionary<string, string?>
        {
            [SettingKeys.SiteName] = settings.SiteName,
            [SettingKeys.SiteSlogan] = settings.SiteSlogan,
            [SettingKeys.SupportPhone] = settings.SupportPhone,
            [SettingKeys.SupportEmail] = settings.SupportEmail,
            [SettingKeys.Address] = settings.Address,
            [SettingKeys.Instagram] = settings.Instagram,
            [SettingKeys.Telegram] = settings.Telegram,
            [SettingKeys.ShippingCost] = settings.ShippingCost.ToString(CultureInfo.InvariantCulture),
            [SettingKeys.FreeShippingThreshold] = settings.FreeShippingThreshold.ToString(CultureInfo.InvariantCulture),
            [SettingKeys.TaxPercent] = settings.TaxPercent.ToString(CultureInfo.InvariantCulture),
            [SettingKeys.BankName] = settings.BankName,
            [SettingKeys.CardNumber] = settings.CardNumber,
            [SettingKeys.CardOwner] = settings.CardOwner,
            [SettingKeys.CashOnDeliveryFee] = settings.CashOnDeliveryFee.ToString(CultureInfo.InvariantCulture)
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    private static string GetString(IReadOnlyDictionary<string, string?> values, string key, string fallback)
        => values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;

    private static decimal GetDecimal(IReadOnlyDictionary<string, string?> values, string key, decimal fallback)
        => values.TryGetValue(key, out var value) &&
           decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;

    private static double GetDouble(IReadOnlyDictionary<string, string?> values, string key, double fallback)
        => values.TryGetValue(key, out var value) &&
           double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
}
