using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Infrastructure.Data;

namespace ShopingStore.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _db;

    public CartRepository(ApplicationDbContext db) => _db = db;

    private IQueryable<Cart> Query => _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Category);

    public async Task<Cart> GetOrCreateAsync(string cartKey, int? userId, CancellationToken ct = default)
    {
        var cart = await FindAsync(cartKey, userId, ct);
        if (cart is not null) return cart;

        cart = new Cart { CartKey = cartKey, UserId = userId, LastActivityUtc = DateTime.UtcNow };
        await _db.Carts.AddAsync(cart, ct);
        await _db.SaveChangesAsync(ct);

        return await GetWithItemsAsync(cart.Id, ct) ?? cart;
    }

    public Task<Cart?> FindAsync(string cartKey, int? userId, CancellationToken ct = default)
        => Query.Where(c => userId != null ? c.UserId == userId : c.CartKey == cartKey)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(ct);

    public Task<Cart?> GetWithItemsAsync(int cartId, CancellationToken ct = default)
        => Query.FirstOrDefaultAsync(c => c.Id == cartId, ct);

    public Task<Cart?> GetByUserAsync(int userId, CancellationToken ct = default)
        => Query.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public void Update(Cart cart)
    {
        cart.LastActivityUtc = DateTime.UtcNow;
        _db.Carts.Update(cart);
    }

    public async Task AddItemAsync(CartItem item, CancellationToken ct = default)
        => await _db.CartItems.AddAsync(item, ct);

    public void UpdateItem(CartItem item) => _db.CartItems.Update(item);

    public void RemoveItem(CartItem item)
    {
        item.IsDeleted = true;
        _db.CartItems.Update(item);
    }

    public void RemoveItems(IEnumerable<CartItem> items)
    {
        foreach (var item in items) RemoveItem(item);
    }

    public async Task<int> CountItemsAsync(string cartKey, int? userId, CancellationToken ct = default)
        => await _db.CartItems
            .Where(i => userId != null
                ? i.Cart!.UserId == userId
                : i.Cart!.CartKey == cartKey)
            .SumAsync(i => (int?)i.Quantity, ct) ?? 0;

    public Task<int> CountItemsByUserAsync(int userId, CancellationToken ct = default)
        => CountItemsAsync(string.Empty, userId, ct);

    public async Task MergeGuestCartAsync(string cartKey, int userId, CancellationToken ct = default)
    {
        var guestCart = await Query.FirstOrDefaultAsync(c => c.CartKey == cartKey && c.UserId == null, ct);
        if (guestCart is null) return;

        var userCart = await Query.FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (userCart is null)
        {
            guestCart.UserId = userId;
            guestCart.LastActivityUtc = DateTime.UtcNow;
            _db.Carts.Update(guestCart);
            await _db.SaveChangesAsync(ct);
            return;
        }

        foreach (var item in guestCart.Items.ToList())
        {
            var existing = userCart.Items.FirstOrDefault(i =>
                i.ProductId == item.ProductId &&
                (i.VariantSelection ?? string.Empty) == (item.VariantSelection ?? string.Empty));

            if (existing is null)
            {
                existing = new CartItem
                {
                    CartId = userCart.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    VariantSelection = item.VariantSelection
                };
                await _db.CartItems.AddAsync(existing, ct);
            }
            else
            {
                existing.Quantity += item.Quantity;
                _db.CartItems.Update(existing);
            }

            RemoveItem(item);
        }

        if (guestCart.DiscountCode is not null && userCart.DiscountCode is null)
            userCart.DiscountCode = guestCart.DiscountCode;

        guestCart.IsDeleted = true;
        _db.Carts.Update(guestCart);
        userCart.LastActivityUtc = DateTime.UtcNow;
        _db.Carts.Update(userCart);

        await _db.SaveChangesAsync(ct);
    }

    public async Task ClearItemsAsync(int cartId, CancellationToken ct = default)
        => await _db.CartItems.Where(i => i.CartId == cartId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsDeleted, true), ct);
}
