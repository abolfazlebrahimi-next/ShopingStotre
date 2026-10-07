using System.Security.Claims;
using ShopingStore.Domain.Interfaces;

namespace ShopingStore.Web.Infrastructure;

/// <summary>دسترسی به اطلاعات کاربر جاری از طریق HttpContext.</summary>
public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? FullName => Principal?.FindFirstValue(ClaimTypes.GivenName) ?? Principal?.Identity?.Name;
    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => Principal?.IsInRole("Admin") ?? false;
    public bool CanManageStore => IsAdmin || (Principal?.IsInRole("Support") ?? false);
    public string? RemoteIp => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
