namespace ShopingStore.Web.Infrastructure;

/// <summary>
/// شناسه سبد خرید مهمان که در یک کوکی ذخیره می‌شود؛
/// برای کاربران وارد‌شده از شناسه کاربر استفاده می‌شود و کوکی همچنان برای ادغام سبد مهمان لازم است.
/// </summary>
public class CartIdentity
{
    public const string CookieName = "ShopingStore.CartKey";

    private readonly IHttpContextAccessor _accessor;

    public CartIdentity(IHttpContextAccessor accessor) => _accessor = accessor;

    public string CartKey
    {
        get
        {
            var context = _accessor.HttpContext;
            if (context is null) return Guid.NewGuid().ToString("N");

            if (context.Request.Cookies.TryGetValue(CookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
                return existing;

            var key = Guid.NewGuid().ToString("N");

            if (!context.Response.HasStarted)
            {
                context.Response.Cookies.Append(CookieName, key, new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(60),
                    SameSite = SameSiteMode.Lax
                });
            }

            return key;
        }
    }
}
