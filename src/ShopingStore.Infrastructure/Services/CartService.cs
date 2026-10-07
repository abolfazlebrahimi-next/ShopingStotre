using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;
    private readonly IDiscountCodeRepository _discounts;
    private readonly ISettingsService _settings;
    private readonly IUnitOfWork _uow;

    public CartService(
        ICartRepository carts,
        IProductRepository products,
        IDiscountCodeRepository discounts,
        ISettingsService settings,
        IUnitOfWork uow)
    {
        _carts = carts;
        _products = products;
        _discounts = discounts;
        _settings = settings;
        _uow = uow;
    }

    public async Task<CartDto> GetCartAsync(string cartKey, int? userId, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(cartKey, userId, ct);
        return await BuildAsync(cart, ct);
    }

    public async Task<CartDto> AddItemAsync(string cartKey, int? userId, int productId, int quantity = 1, string? variant = null, CancellationToken ct = default)
    {
        Guard.InRange(quantity, 1, 10, "تعداد");

        var cart = await _carts.GetOrCreateAsync(cartKey, userId, ct);
        var product = await _products.GetByIdAsync(productId, false, ct)
            ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");

        if (!product.IsActive) throw new BusinessException("این محصول در حال حاضر قابل خریداری نیست.");
        if (product.Stock <= 0) throw new BusinessException("متأسفانه این محصول موجود نیست.");

        var normalizedVariant = string.IsNullOrWhiteSpace(variant) ? null : variant.Trim();
        var existing = cart.Items.FirstOrDefault(i =>
            i.ProductId == productId &&
            (i.VariantSelection ?? string.Empty) == (normalizedVariant ?? string.Empty));

        if (existing is null)
        {
            await _carts.AddItemAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = Math.Min(quantity, product.Stock),
                VariantSelection = normalizedVariant
            }, ct);
        }
        else
        {
            existing.Quantity = Math.Min(existing.Quantity + quantity, Math.Min(product.Stock, 10));
            _carts.UpdateItem(existing);
        }

        cart.LastActivityUtc = DateTime.UtcNow;
        _carts.Update(cart);
        await _uow.SaveChangesAsync(ct);

        var refreshed = await _carts.GetWithItemsAsync(cart.Id, ct) ?? cart;
        return await BuildAsync(refreshed, ct);
    }

    public async Task<CartDto> UpdateQuantityAsync(string cartKey, int? userId, int cartItemId, int quantity, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(cartKey, userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new NotFoundException("این قلم در سبد خرید شما وجود ندارد.");

        if (quantity <= 0)
        {
            _carts.RemoveItem(item);
        }
        else
        {
            var stock = item.Product?.Stock ?? 0;
            item.Quantity = Math.Clamp(quantity, 1, Math.Max(1, Math.Min(stock, 10)));
            _carts.UpdateItem(item);
        }

        _carts.Update(cart);
        await _uow.SaveChangesAsync(ct);

        var refreshed = await _carts.GetWithItemsAsync(cart.Id, ct) ?? cart;
        return await BuildAsync(refreshed, ct);
    }

    public async Task<CartDto> RemoveItemAsync(string cartKey, int? userId, int cartItemId, CancellationToken ct = default)
        => await UpdateQuantityAsync(cartKey, userId, cartItemId, 0, ct);

    public async Task<CartDto> ClearAsync(string cartKey, int? userId, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(cartKey, userId, ct);
        await _carts.ClearItemsAsync(cart.Id, ct);
        cart.DiscountCode = null;
        _carts.Update(cart);
        await _uow.SaveChangesAsync(ct);

        var refreshed = await _carts.GetWithItemsAsync(cart.Id, ct) ?? cart;
        return await BuildAsync(refreshed, ct);
    }

    public async Task<(bool Success, string? Message, CartDto Cart)> ApplyDiscountAsync(string cartKey, int? userId, string code, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(cartKey, userId, ct);
        var subtotal = cart.Items.Sum(i => (i.Product?.EffectivePrice ?? 0) * i.Quantity);

        var discountCode = await _discounts.GetByCodeAsync(Guard.NotEmpty(code, "کد تخفیف", 60), ct);
        if (discountCode is null || !discountCode.IsUsable(DateTime.UtcNow, subtotal, out var error))
        {
            return (false, error ?? "کد تخفیف وارد‌شده معتبر نیست.", await BuildAsync(cart, ct));
        }

        cart.DiscountCode = discountCode.Code;
        _carts.Update(cart);
        await _uow.SaveChangesAsync(ct);

        var refreshed = await _carts.GetWithItemsAsync(cart.Id, ct) ?? cart;
        var dto = await BuildAsync(refreshed, ct);

        return (true, $"کد تخفیف «{discountCode.Code}» اعمال شد.", dto);
    }

    public async Task<CartDto> RemoveDiscountAsync(string cartKey, int? userId, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(cartKey, userId, ct);
        cart.DiscountCode = null;
        _carts.Update(cart);
        await _uow.SaveChangesAsync(ct);

        var refreshed = await _carts.GetWithItemsAsync(cart.Id, ct) ?? cart;
        return await BuildAsync(refreshed, ct);
    }

    public Task<int> GetItemsCountAsync(string cartKey, int? userId, CancellationToken ct = default)
        => _carts.CountItemsAsync(cartKey, userId, ct);

    public async Task MoveGuestCartToUserAsync(string cartKey, int userId, CancellationToken ct = default)
        => await _carts.MergeGuestCartAsync(cartKey, userId, ct);

    /// <summary>ساخت DTO سبد خرید همراه با محاسبه مبالغ.</summary>
    private async Task<CartDto> BuildAsync(Cart cart, CancellationToken ct)
    {
        var settings = await _settings.GetAsync(ct);
        var items = cart.Items.Where(i => i.Product is not null && i.Product.IsActive).ToList();
        var subtotal = items.Sum(i => i.Product!.EffectivePrice * i.Quantity);

        decimal discount = 0;
        string? appliedCode = null;

        if (!string.IsNullOrWhiteSpace(cart.DiscountCode))
        {
            var code = await _discounts.GetByCodeAsync(cart.DiscountCode, ct);
            if (code is not null && code.IsUsable(DateTime.UtcNow, subtotal, out _))
            {
                discount = code.CalculateDiscount(subtotal);
                appliedCode = code.Code;
            }
        }

        var (shipping, tax, total) = CartPricing.Calculate(settings, subtotal, discount);

        var freeShippingRemaining = settings.FreeShippingThreshold > 0
            ? Math.Max(0, settings.FreeShippingThreshold - (subtotal - discount))
            : 0;

        return new CartDto
        {
            Items = items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product!.Name,
                ProductSlug = i.Product.Slug,
                ImageUrl = string.IsNullOrWhiteSpace(i.Product.MainImageUrl) ? Mappers.PlaceholderImage : i.Product.MainImageUrl,
                UnitPrice = i.Product.EffectivePrice,
                OriginalPrice = i.Product.Price,
                Quantity = i.Quantity,
                Stock = i.Product.Stock,
                VariantSelection = i.VariantSelection
            }).ToList(),
            DiscountCode = appliedCode,
            DiscountAmount = discount,
            Subtotal = subtotal,
            ShippingCost = shipping,
            TaxAmount = tax,
            Total = total,
            FreeShippingRemaining = freeShippingRemaining
        };
    }
}
