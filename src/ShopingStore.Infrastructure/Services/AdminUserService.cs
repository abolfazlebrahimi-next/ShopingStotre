using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public AdminUserService(IUserRepository users, IUnitOfWork uow)
    {
        _users = users;
        _uow = uow;
    }

    public async Task<PagedResult<UserProfileDto>> SearchAsync(AdminUserFilter filter, CancellationToken ct = default)
    {
        var result = await _users.SearchAsync(filter, ct);
        return result.Map(u => u.ToProfileDto());
    }

    public async Task<UserProfileDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null) return null;

        var (ordersCount, totalSpent) = await _users.GetUserStatsAsync(id, ct);
        return user.ToProfileDto(ordersCount, totalSpent);
    }

    public async Task ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct) ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");

        if (user.Role == UserRole.Admin && user.IsActive && await _users.CountAsync(false, ct) <= 1)
            throw new BusinessException("امکان غیرفعال‌سازی تنها مدیر سیستم وجود ندارد.");

        user.IsActive = !user.IsActive;
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ChangeRoleAsync(int id, UserRole role, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct) ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");

        if (user.Role == UserRole.Admin && role != UserRole.Admin)
        {
            var admins = await _users.SearchAsync(new AdminUserFilter { Role = UserRole.Admin, PageSize = 100 }, ct);
            if (admins.TotalCount <= 1)
                throw new BusinessException("حداقل یک مدیر سیستم باید باقی بماند.");
        }

        user.Role = role;
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }
}
