namespace ShopingStore.Domain.Interfaces;

/// <summary>انتزاع زمان برای تست‌پذیری.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

public interface ICurrentUser
{
    int? UserId { get; }
    string? FullName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool CanManageStore { get; }
    string? RemoteIp { get; }
}
