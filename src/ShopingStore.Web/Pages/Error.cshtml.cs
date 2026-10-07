using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopingStore.Web.Pages;

public class ErrorModel : PageModel
{
    public int? Code { get; private set; }

    public void OnGet(int? code = null) => Code = code;
}
