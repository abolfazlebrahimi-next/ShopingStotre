using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IAdminUserService _users;
    private readonly ToastService _toast;

    public IndexModel(IAdminUserService users, ToastService toast)
    {
        _users = users;
        _toast = toast;
    }

    public PagedResult<UserProfileDto> Users { get; private set; } = PagedResult<UserProfileDto>.Empty();
    public AdminUserFilter Filter { get; private set; } = new();
    public PaginationModel Pagination { get; private set; } = new();

    public async Task OnGetAsync(string? search, UserRole? role, bool? isActive, int page, CancellationToken cancellationToken)
    {
        Filter = new AdminUserFilter
        {
            Search = search,
            Role = role,
            IsActive = isActive,
            Page = page <= 0 ? 1 : page,
            PageSize = 15
        };

        Users = await _users.SearchAsync(Filter, cancellationToken);

        Pagination = new PaginationModel
        {
            Page = Users.Page,
            TotalPages = Users.TotalPages,
            TotalCount = Users.TotalCount,
            BasePath = "/admin/users",
            Query = Request.Query
        };
    }

    public async Task<IActionResult> OnPostToggleAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _users.ToggleActiveAsync(id, cancellationToken);
            _toast.Success("وضعیت کاربر تغییر کرد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRoleAsync(int id, UserRole role, CancellationToken cancellationToken)
    {
        try
        {
            await _users.ChangeRoleAsync(id, role, cancellationToken);
            _toast.Success("نقش کاربر تغییر کرد.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return RedirectToPage();
    }
}
