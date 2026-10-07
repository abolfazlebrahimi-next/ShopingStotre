using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopingStore.Web.Pages.Checkout;

public class FailedModel : PageModel
{
    public string? OrderNumber { get; private set; }

    public void OnGet(string? order) => OrderNumber = order;
}
