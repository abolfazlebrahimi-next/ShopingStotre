using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ICatalogService _catalog;
    private readonly ICurrentUser _currentUser;

    public IndexModel(ICatalogService catalog, ICurrentUser currentUser)
    {
        _catalog = catalog;
        _currentUser = currentUser;
    }

    public HomePageDto Home { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Home = await _catalog.GetHomePageAsync(_currentUser.UserId, cancellationToken);
}
