using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboard;

    public IndexModel(IDashboardService dashboard) => _dashboard = dashboard;

    public DashboardStatsDto Stats { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Stats = await _dashboard.GetStatsAsync(14, cancellationToken);
}
