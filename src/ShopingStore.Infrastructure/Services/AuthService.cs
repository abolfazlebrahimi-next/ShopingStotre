using Microsoft.Extensions.Logging;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;
using ShopingStore.Infrastructure.Security;

namespace ShopingStore.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, IUnitOfWork uow, ILogger<AuthService> logger)
    {
        _users = users;
        _uow = uow;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var fullName = Guard.NotEmpty(request.FullName, "نام و نام خانوادگی", 150);
        var email = Guard.Email(request.Email);
        var phone = Guard.Mobile(request.PhoneNumber);

        if (request.Password != request.ConfirmPassword)
            throw new BusinessException("تکرار رمز عبور با رمز عبور یکسان نیست.");

        ValidatePassword(request.Password);

        if (!request.AcceptTerms)
            throw new BusinessException("برای ثبت‌نام باید قوانین و شرایط استفاده را بپذیرید.");

        if (await _users.EmailExistsAsync(email, null, ct))
            throw new BusinessException("کاربری با این ایمیل قبلاً ثبت‌نام کرده است.");

        var user = new User
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phone,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = UserRole.Customer,
            IsActive = true
        };

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("کاربر جدید ثبت‌نام کرد: {Email}", email);

        return user;
    }

    public async Task<User> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = Guard.Email(request.Email);
        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null)
            throw new BusinessException("ایمیل یا رمز عبور نادرست است.");

        if (!user.IsActive)
            throw new BusinessException("حساب کاربری شما غیرفعال شده است. با پشتیبانی تماس بگیرید.");

        if (string.IsNullOrEmpty(user.PasswordHash) || !PasswordHasher.Verify(request.Password, user.PasswordHash, out var needsRehash))
            throw new BusinessException("ایمیل یا رمز عبور نادرست است.");

        if (needsRehash) user.PasswordHash = PasswordHasher.Hash(request.Password);

        user.LastLoginAtUtc = DateTime.UtcNow;
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return user;
    }

    public async Task<User> LoginOrCreateExternalAsync(string provider, string key, string email, string fullName, CancellationToken ct = default)
    {
        var user = await _users.GetByExternalKeyAsync(provider, key, ct);
        if (user is not null) return user;

        var normalizedEmail = Guard.Email(email);
        user = await _users.GetByEmailAsync(normalizedEmail, ct);

        if (user is null)
        {
            user = new User
            {
                FullName = string.IsNullOrWhiteSpace(fullName) ? normalizedEmail : fullName.Trim(),
                Email = normalizedEmail,
                Role = UserRole.Customer,
                IsActive = true
            };
            await _users.AddAsync(user, ct);
        }

        user.ExternalProvider = provider;
        user.ExternalProviderKey = key;
        user.LastLoginAtUtc = DateTime.UtcNow;
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return user;
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("کاربر یافت نشد.");

        if (!string.IsNullOrEmpty(user.PasswordHash) &&
            !PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new BusinessException("رمز عبور فعلی نادرست است.");
        }

        if (request.NewPassword != request.ConfirmPassword)
            throw new BusinessException("تکرار رمز عبور جدید نادرست است.");

        ValidatePassword(request.NewPassword);

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<User> UpdateProfileAsync(int userId, string fullName, string? phoneNumber, string? nationalCode, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("کاربر یافت نشد.");

        user.FullName = Guard.NotEmpty(fullName, "نام و نام خانوادگی", 150);
        user.PhoneNumber = Guard.Mobile(phoneNumber);
        user.NationalCode = Guard.Optional(nationalCode, 20);

        _users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return user;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) => _users.GetByIdAsync(id, ct);

    public async Task<UserProfileDto?> GetProfileAsync(int id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null) return null;

        var (ordersCount, totalSpent) = await _users.GetUserStatsAsync(id, ct);
        return user.ToProfileDto(ordersCount, totalSpent);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new BusinessException("رمز عبور باید حداقل ۶ کاراکتر باشد.");

        if (password.Length > 100)
            throw new BusinessException("رمز عبور نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");
    }
}
