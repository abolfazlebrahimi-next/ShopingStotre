using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages.Account;

public class FavoritesModel : PageModel
{
    private readonly IWishlistService _wishlist;
    private readonly ICurrentUser _currentUser;

    public FavoritesModel(IWishlistService wishlist, ICurrentUser currentUser)
    {
        _wishlist = wishlist;
        _currentUser = currentUser;
    }

    public IReadOnlyList<ProductListItemDto> Products { get; private set; } = Array.Empty<ProductListItemDto>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login?returnUrl=/account/favorites");

        Products = await _wishlist.GetFavoritesAsync(_currentUser.UserId.Value, cancellationToken);
        return Page();
    }
}
