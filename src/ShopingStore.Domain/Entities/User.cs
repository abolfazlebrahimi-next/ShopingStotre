using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    /// <summary>هش رمز عبور (PBKDF2). برای ورود با گوگل خالی است.</summary>
    public string? PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAtUtc { get; set; }

    /// <summary>شناسه کاربر در سرویس خارجی (لاگین با گوگل).</summary>
    public string? ExternalProvider { get; set; }
    public string? ExternalProviderKey { get; set; }

    public string? NationalCode { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public bool IsAdmin => Role == UserRole.Admin;
    public bool CanManageStore => Role is UserRole.Admin or UserRole.Support;

    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "؟",
                1 => parts[0][..1],
                _ => $"{parts[0][..1]}{parts[^1][..1]}"
            };
        }
    }
}
