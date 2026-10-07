using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages;

public class FaqModel : PageModel
{
    private readonly ISettingsService _settings;

    public FaqModel(ISettingsService settings) => _settings = settings;

    public StoreSettingsDto Settings { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Settings = await _settings.GetAsync(cancellationToken);
}
