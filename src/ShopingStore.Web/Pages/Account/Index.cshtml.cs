using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;

namespace ShopingStore.Web.Pages.Account;

public class IndexModel : PageModel
{
    private readonly IAuthService _auth;
    private readonly IOrderService _orders;
    private readonly IWishlistService _wishlist;
    private readonly IAddressService _addresses;
    private readonly ICurrentUser _currentUser;

    public IndexModel(
        IAuthService auth,
        IOrderService orders,
        IWishlistService wishlist,
        IAddressService addresses,
        ICurrentUser currentUser)
    {
        _auth = auth;
        _orders = orders;
        _wishlist = wishlist;
        _addresses = addresses;
        _currentUser = currentUser;
    }

    public UserProfileDto? Profile { get; private set; }
    public IReadOnlyList<OrderDto> RecentOrders { get; private set; } = Array.Empty<OrderDto>();
    public int FavoriteCount { get; private set; }
    public int AddressCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Redirect("/account/login?returnUrl=/account");

        var userId = _currentUser.UserId.Value;

        Profile = await _auth.GetProfileAsync(userId, cancellationToken);
        var page = await _orders.GetUserOrdersAsync(userId, 1, 5, cancellationToken);
        RecentOrders = page.Items;

        FavoriteCount = (await _wishlist.GetFavoritesAsync(userId, cancellationToken)).Count;
        AddressCount = (await _addresses.GetUserAddressesAsync(userId, cancellationToken)).Count;

        return Page();
    }
}
