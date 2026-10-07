using Microsoft.Extensions.Logging;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;
    private readonly IDiscountCodeRepository _discounts;
    private readonly ISettingsService _settings;
    private readonly IPaymentGateway _paymentGateway;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orders,
        ICartRepository carts,
        IProductRepository products,
        IDiscountCodeRepository discounts,
        ISettingsService settings,
        IPaymentGateway paymentGateway,
        INotificationService notifications,
        IUnitOfWork uow,
        ILogger<OrderService> logger)
    {
        _orders = orders;
        _carts = carts;
        _products = products;
        _discounts = discounts;
        _settings = settings;
        _paymentGateway = paymentGateway;
        _notifications = notifications;
        _uow = uow;
        _logger = logger;
    }

    public async Task<OrderDto> CheckoutAsync(string cartKey, int userId, CheckoutRequest request, CancellationToken ct = default)
    {
        var cart = await _carts.FindAsync(cartKey, userId, ct);

        if (cart is null || cart.Items.Count == 0)
            throw new BusinessException("سبد خرید شما خالی است.");

        var settings = await _settings.GetAsync(ct);

        var order = new Order
        {
            OrderNumber = OrderNumberGenerator.Generate(),
            UserId = userId,
            CustomerName = Guard.NotEmpty(request.CustomerName, "نام گیرنده", 150),
            CustomerPhone = Guard.Mobile(request.CustomerPhone),
            CustomerEmail = Guard.Optional(request.CustomerEmail, 200),
            Province = Guard.NotEmpty(request.Province, "استان", 100),
            City = Guard.NotEmpty(request.City, "شهر", 100),
            PostalCode = Guard.NotEmpty(request.PostalCode, "کد پستی", 20),
            AddressLine = Guard.NotEmpty(request.AddressLine, "نشانی کامل", 600),
            CustomerNote = Guard.Optional(request.CustomerNote, 1000),
            PaymentMethod = request.PaymentMethod
        };

        decimal subtotal = 0;

        foreach (var cartItem in cart.Items)
        {
            var product = await _products.GetByIdAsync(cartItem.ProductId, false, ct);

            if (product is null || !product.IsActive)
                throw new BusinessException("یکی از محصولات سبد خرید شما در دسترس نیست. لطفاً سبد خرید را بازبینی کنید.");

            if (product.Stock < cartItem.Quantity)
                throw new BusinessException($"موجودی «{product.Name}» کافی نیست. موجودی فعلی: {PersianDate.ToPersianDigits(product.Stock.ToString())} عدد");

            order.Items.Add(OrderItem.FromProduct(product, cartItem.Quantity, cartItem.VariantSelection));
            subtotal += product.EffectivePrice * cartItem.Quantity;
        }

        // کد تخفیف
        if (!string.IsNullOrWhiteSpace(cart.DiscountCode))
        {
            var code = await _discounts.GetByCodeAsync(cart.DiscountCode, ct);
            if (code is not null && code.IsUsable(DateTime.UtcNow, subtotal, out _))
            {
                order.ApplyDiscount(code.CalculateDiscount(subtotal), code.Code);
                code.UsedCount++;
                _discounts.Update(code);
            }
        }

        var cashOnDeliveryFee = request.PaymentMethod == PaymentMethod.CashOnDelivery ? settings.CashOnDeliveryFee : 0;
        var (shipping, tax, total) = CartPricing.Calculate(settings, subtotal, order.DiscountAmount);

        shipping += cashOnDeliveryFee;

        order.PaymentStatus = PaymentStatus.Unpaid;
        order.Status = request.PaymentMethod == PaymentMethod.CashOnDelivery ? OrderStatus.Processing : OrderStatus.Pending;
        order.Subtotal = subtotal;
        order.ShippingCost = shipping;
        order.TaxAmount = tax;
        order.Total = total + cashOnDeliveryFee;

        order.History.Add(new OrderStatusHistory
        {
            Status = order.Status,
            Note = "سفارش ثبت شد",
            Actor = order.CustomerName
        });

        await using var transaction = await _uow.BeginTransactionAsync(ct);

        // کسر موجودی و افزایش تعداد فروش
        foreach (var item in order.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId, false, ct);
            if (product is null) continue;

            product.Stock -= item.Quantity;
            product.SoldCount += item.Quantity;
            _products.Update(product);
        }

        await _orders.AddAsync(order, ct);

        // خالی کردن سبد خرید
        await _carts.ClearItemsAsync(cart.Id, ct);
        cart.DiscountCode = null;
        _carts.Update(cart);

        await _uow.SaveChangesAsync(ct);
        await _uow.CommitAsync(ct);

        _logger.LogInformation("سفارش {OrderNumber} به مبلغ {Total} ثبت شد.", order.OrderNumber, order.Total);

        var dto = order.ToDto();
        await _notifications.SendOrderConfirmationAsync(dto, ct);

        return dto;
    }

    public async Task<PaymentRequestResult> StartPaymentAsync(int orderId, string callbackUrl, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, true, ct)
            ?? throw new NotFoundException("سفارش مورد نظر یافت نشد.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new BusinessException("این سفارش قبلاً پرداخت شده است.");

        return await _paymentGateway.RequestPaymentAsync(order.Total, order.OrderNumber, callbackUrl, ct);
    }

    public async Task<bool> ConfirmOnlinePaymentAsync(string orderNumber, string authority, CancellationToken ct = default)
    {
        var order = await _orders.GetByNumberAsync(orderNumber, ct);
        if (order is null) return false;

        var verification = await _paymentGateway.VerifyPaymentAsync(authority, order.Total, ct);
        if (!verification.Success) return false;

        order.MarkAsPaid(verification.ReferenceId ?? authority);
        order.ChangeStatus(OrderStatus.Paid, "پرداخت موفق", "درگاه پرداخت");

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        await _notifications.SendOrderConfirmationAsync(order.ToDto(), ct);

        return true;
    }

    public async Task<PagedResult<OrderDto>> GetUserOrdersAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _orders.GetUserOrdersAsync(userId, page, pageSize, ct);
        return result.Map(o => o.ToDto());
    }

    public async Task<OrderDto?> GetOrderAsync(int orderId, int? userId = null, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, true, ct);
        if (order is null) return null;
        if (userId is not null && order.UserId != userId) throw new ForbiddenException("این سفارش به حساب شما تعلق ندارد.");

        return order.ToDto();
    }

    public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, int? userId = null, CancellationToken ct = default)
    {
        var order = await _orders.GetByNumberAsync(orderNumber, ct);
        if (order is null) return null;
        if (userId is not null && order.UserId != userId) throw new ForbiddenException("این سفارش به حساب شما تعلق ندارد.");

        return order.ToDto();
    }

    public async Task<PagedResult<OrderDto>> SearchAsync(AdminOrderFilter filter, CancellationToken ct = default)
    {
        var result = await _orders.SearchAsync(filter, ct);
        return result.Map(o => o.ToDto());
    }

    public async Task UpdateStatusAsync(UpdateOrderStatusRequest request, string actor, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(request.OrderId, true, ct)
            ?? throw new NotFoundException("سفارش مورد نظر یافت نشد.");

        order.ChangeStatus(request.Status, request.Note, actor);

        if (!string.IsNullOrWhiteSpace(request.TrackingCode))
            order.TrackingCode = request.TrackingCode.Trim();

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        await _notifications.SendSmsAsync(order.CustomerPhone,
            $"وضعیت سفارش {order.OrderNumber} به «{request.Status.ToPersianTitle()}» تغییر کرد.");
    }

    public async Task CancelAsync(int orderId, int? userId, string? reason = null, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, true, ct)
            ?? throw new NotFoundException("سفارش مورد نظر یافت نشد.");

        if (userId is not null && order.UserId != userId)
            throw new ForbiddenException("این سفارش به حساب شما تعلق ندارد.");

        if (order.Status is OrderStatus.Delivered or OrderStatus.Shipped)
            throw new BusinessException("سفارش‌های ارسال‌شده یا تحویل‌شده را باید از طریق پشتیبانی مرجوع کنید.");

        // بازگرداندن موجودی کالاها
        foreach (var item in order.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId, false, ct);
            if (product is null) continue;

            product.Stock += item.Quantity;
            product.SoldCount = Math.Max(0, product.SoldCount - item.Quantity);
            _products.Update(product);
        }

        order.Cancel(reason);
        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> CanUserReviewProductAsync(int userId, int productId, CancellationToken ct = default)
    {
        var page = await _orders.GetUserOrdersAsync(userId, 1, 50, ct);
        return page.Items.Any(o => o.Status is OrderStatus.Delivered or OrderStatus.Shipped or OrderStatus.Paid
                                   && o.Items.Any(i => i.ProductId == productId));
    }
}
