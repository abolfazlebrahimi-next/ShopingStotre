using System.Text;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Endpoints;

/// <summary>
/// نقطه‌های پایانی سبک (JSON API) که در کنار صفحات Razor استفاده می‌شوند:
/// پیشنهاد جست‌وجو، شمارش سبد خرید، علاقه‌مندی‌ها، نقشه سایت و سلامت سرویس.
/// </summary>
public static class EndpointMappings
{
    public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { status = "ok", timeUtc = DateTime.UtcNow }))
            .WithName("HealthCheck");

        // نقشه سایت ساده برای موتورهای جست‌وجو
        endpoints.MapGet("/sitemap.xml", async (ICatalogService catalog, CancellationToken ct) =>
        {
            var categories = await catalog.GetCategoryTreeAsync(ct);
            var products = await catalog.SearchProductsAsync(new ProductQuery { PageSize = 60 }, null, ct);

            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            builder.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            builder.AppendLine("  <url><loc>/</loc></url>");
            builder.AppendLine("  <url><loc>/products</loc></url>");

            foreach (var category in categories.SelectMany(c => c.Children.Prepend(c)))
                builder.AppendLine($"  <url><loc>/category/{category.Slug}</loc></url>");

            foreach (var product in products.Items)
                builder.AppendLine($"  <url><loc>/product/{product.Slug}</loc></url>");

            builder.AppendLine("</urlset>");

            return Results.Content(builder.ToString(), "application/xml", Encoding.UTF8);
        }).WithName("Sitemap");

        return endpoints;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // پیشنهاد زنده جست‌وجو
        endpoints.MapGet("/api/products/suggest", async (string? term, ICatalogService catalog, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
                return Results.Ok(Array.Empty<object>());

            var suggestions = await catalog.GetSuggestionsAsync(term.Trim(), 6, ct);

            return Results.Ok(suggestions.Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                imageUrl = p.ImageUrl,
                price = PersianDate.ToPrice(p.EffectivePrice),
                category = p.CategoryName
            }));
        }).WithName("ProductSuggestions");

        endpoints.MapGet("/api/products/{slug}", async (string slug, ICatalogService catalog, ICurrentUser currentUser, CancellationToken ct) =>
        {
            var product = await catalog.GetProductDetailsAsync(slug, currentUser.UserId, false, ct);
            return product is null ? Results.NotFound(new { message = "محصول یافت نشد." }) : Results.Ok(product);
        }).WithName("ProductApi");

        endpoints.MapGet("/api/categories", async (ICatalogService catalog, CancellationToken ct) =>
            Results.Ok(await catalog.GetCategoryTreeAsync(ct))).WithName("CategoriesApi");

        return endpoints;
    }

    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/cart/count", async (ICartService carts, ICurrentUser user, CartIdentity identity, CancellationToken ct) =>
        {
            var count = await carts.GetItemsCountAsync(identity.CartKey, user.UserId, ct);
            var cart = await carts.GetCartAsync(identity.CartKey, user.UserId, ct);

            return Results.Ok(new
            {
                count,
                countLabel = PersianDate.ToPersianDigits(count.ToString()),
                total = cart.Total,
                totalLabel = PersianDate.ToPrice(cart.Total)
            });
        }).WithName("CartCount");

        return endpoints;
    }

    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/account/summary", async (IAuthService auth, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is null) return Results.Unauthorized();

            var profile = await auth.GetProfileAsync(user.UserId.Value, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization("CustomerOnly").WithName("AccountSummary");

        return endpoints;
    }

    public static IEndpointRouteBuilder MapCheckoutEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/orders/{id:int}/status", async (int id, IOrderService orders, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is null) return Results.Unauthorized();

            try
            {
                var order = await orders.GetOrderAsync(id, user.UserId, ct);
                if (order is null) return Results.NotFound();

                return Results.Ok(new
                {
                    order.OrderNumber,
                    status = order.Status.ToString(),
                    statusTitle = order.Status.ToPersianTitle(),
                    paymentStatus = order.PaymentStatus.ToString(),
                    trackingCode = order.TrackingCode
                });
            }
            catch (ForbiddenException)
            {
                return Results.Forbid();
            }
        }).RequireAuthorization("CustomerOnly").WithName("OrderStatus");

        return endpoints;
    }

    /// <summary>APIهای عمومی استفاده‌شده توسط جاوااسکریپت فروشگاه.</summary>
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // تغییر وضعیت علاقه‌مندی (افزودن/حذف)
        endpoints.MapPost("/api/wishlist/toggle", async (
            HttpContext context,
            [FromForm] int productId,
            IAntiforgery antiforgery,
            IWishlistService wishlist,
            ICurrentUser user,
            CancellationToken ct) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest(new { success = false, message = "درخواست نامعتبر است." });
            }

            if (user.UserId is null)
                return Results.Ok(new { success = false, message = "برای افزودن به علاقه‌مندی‌ها وارد حساب کاربری خود شوید." });

            var isFavorite = await wishlist.ToggleAsync(user.UserId.Value, productId, ct);

            return Results.Ok(new
            {
                success = true,
                isFavorite,
                message = isFavorite ? "به علاقه‌مندی‌ها اضافه شد." : "از علاقه‌مندی‌ها حذف شد."
            });
        }).WithName("ToggleWishlist");

        endpoints.MapGet("/api/wishlist/ids", async (IWishlistService wishlist, ICurrentUser user, CancellationToken ct) =>
        {
            var ids = await wishlist.GetFavoriteIdsAsync(user.UserId, ct);
            return Results.Ok(ids);
        }).WithName("WishlistIds");

        return endpoints;
    }

    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/admin/stats", async (IDashboardService dashboard, int? days, CancellationToken ct) =>
        {
            var stats = await dashboard.GetStatsAsync(days is > 0 and <= 90 ? days.Value : 14, ct);
            return Results.Ok(stats);
        }).RequireAuthorization("StaffOnly").WithName("AdminStats");

        return endpoints;
    }
}
