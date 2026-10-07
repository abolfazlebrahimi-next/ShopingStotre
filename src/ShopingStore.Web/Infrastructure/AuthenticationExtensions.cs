using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ShopingStore.Domain.Entities;

namespace ShopingStore.Web.Infrastructure;

public static class AuthenticationExtensions
{
    /// <summary>ورود کاربر و ساخت کوکی احراز هویت.</summary>
    public static async Task SignInAsync(this HttpContext context, User user, bool persistent = true)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.GivenName, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber!));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = persistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(persistent ? 30 : 1)
            });
    }

    public static async Task SignOutAsync(this HttpContext context)
        => await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
