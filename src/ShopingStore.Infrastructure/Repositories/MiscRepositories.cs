using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Infrastructure.Data;

namespace ShopingStore.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly ApplicationDbContext _db;

    public AddressRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Address>> GetUserAddressesAsync(int userId, CancellationToken ct = default)
        => await _db.Addresses.Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.Id)
            .AsNoTracking().ToListAsync(ct);

    public Task<Address?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(Address address, CancellationToken ct = default)
        => await _db.Addresses.AddAsync(address, ct);

    public void Update(Address address) => _db.Addresses.Update(address);

    public void Remove(Address address)
    {
        address.IsDeleted = true;
        _db.Addresses.Update(address);
    }

    public async Task ClearDefaultAsync(int userId, CancellationToken ct = default)
        => await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
}

public class DiscountCodeRepository : IDiscountCodeRepository
{
    private readonly ApplicationDbContext _db;

    public DiscountCodeRepository(ApplicationDbContext db) => _db = db;

    public Task<DiscountCode?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return _db.DiscountCodes.FirstOrDefaultAsync(d => d.Code == normalized, ct);
    }

    public Task<DiscountCode?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.DiscountCodes.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<DiscountCode>> GetAllAsync(CancellationToken ct = default)
        => await _db.DiscountCodes.OrderByDescending(d => d.Id).AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(DiscountCode code, CancellationToken ct = default)
        => await _db.DiscountCodes.AddAsync(code, ct);

    public void Update(DiscountCode code) => _db.DiscountCodes.Update(code);

    public void Remove(DiscountCode code)
    {
        code.IsDeleted = true;
        code.IsActive = false;
        _db.DiscountCodes.Update(code);
    }

    public Task<bool> CodeExistsAsync(string code, int? exceptId = null, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return _db.DiscountCodes.AnyAsync(d => d.Code == normalized && (exceptId == null || d.Id != exceptId), ct);
    }
}

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _db;

    public FavoriteRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Favorite>> GetUserFavoritesAsync(int userId, CancellationToken ct = default)
        => await _db.Favorites.Where(f => f.UserId == userId)
            .Include(f => f.Product).ThenInclude(p => p!.Category)
            .OrderByDescending(f => f.Id)
            .AsNoTracking().ToListAsync(ct);

    public Task<bool> IsFavoriteAsync(int userId, int productId, CancellationToken ct = default)
        => _db.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId, ct);

    public async Task<IReadOnlyList<int>> GetFavoriteProductIdsAsync(int userId, CancellationToken ct = default)
        => await _db.Favorites.Where(f => f.UserId == userId).Select(f => f.ProductId).ToListAsync(ct);

    public Task<Favorite?> GetAsync(int userId, int productId, CancellationToken ct = default)
        => _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, ct);

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default)
        => await _db.Favorites.AddAsync(favorite, ct);

    public void Remove(Favorite favorite)
    {
        favorite.IsDeleted = true;
        _db.Favorites.Update(favorite);
    }
}

public class SettingRepository : ISettingRepository
{
    private readonly ApplicationDbContext _db;

    public SettingRepository(ApplicationDbContext db) => _db = db;

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
        => await _db.Settings.Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<string, string?>> GetAllAsync(CancellationToken ct = default)
    {
        var settings = await _db.Settings.AsNoTracking().ToListAsync(ct);
        return settings.ToDictionary(s => s.Key, s => s.Value);
    }

    public async Task SetAsync(string key, string? value, CancellationToken ct = default)
    {
        var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            await _db.Settings.AddAsync(new Setting { Key = key, Value = value }, ct);
        }
        else
        {
            setting.Value = value;
            _db.Settings.Update(setting);
        }
    }

    public async Task SetManyAsync(IReadOnlyDictionary<string, string?> values, CancellationToken ct = default)
    {
        foreach (var (key, value) in values)
            await SetAsync(key, value, ct);
    }
}
