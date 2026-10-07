using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class WishlistService : IWishlistService
{
    private readonly IFavoriteRepository _favorites;
    private readonly IUnitOfWork _uow;

    public WishlistService(IFavoriteRepository favorites, IUnitOfWork uow)
    {
        _favorites = favorites;
        _uow = uow;
    }

    public async Task<IReadOnlyList<Domain.Dtos.ProductListItemDto>> GetFavoritesAsync(int userId, CancellationToken ct = default)
    {
        var favorites = await _favorites.GetUserFavoritesAsync(userId, ct);
        var ids = favorites.Select(f => f.ProductId).ToHashSet();

        return favorites
            .Where(f => f.Product is not null && f.Product.IsActive)
            .Select(f => f.Product!.ToListItem(ids))
            .ToList();
    }

    public async Task<bool> ToggleAsync(int userId, int productId, CancellationToken ct = default)
    {
        var existing = await _favorites.GetAsync(userId, productId, ct);

        if (existing is null)
        {
            await _favorites.AddAsync(new Domain.Entities.Favorite { UserId = userId, ProductId = productId }, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        _favorites.Remove(existing);
        await _uow.SaveChangesAsync(ct);
        return false;
    }

    public async Task<IReadOnlySet<int>> GetFavoriteIdsAsync(int? userId, CancellationToken ct = default)
    {
        if (userId is null or 0) return new HashSet<int>();
        var ids = await _favorites.GetFavoriteProductIdsAsync(userId.Value, ct);
        return ids.ToHashSet();
    }
}
