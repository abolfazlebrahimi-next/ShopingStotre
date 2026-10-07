using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;

namespace ShopingStore.Domain.Interfaces.Repositories;

/// <summary>واحد کار: ذخیره تغییرات و مدیریت تراکنش.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public interface IProductRepository
{
    Task<PagedResult<Product>> SearchAsync(ProductQuery query, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, bool includeRelated = false, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, bool includeRelated = false, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetNewestAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetBestSellersAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetDiscountedAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int categoryId, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(int threshold, int take, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken ct = default);
    Task<(decimal Min, decimal Max)> GetPriceRangeAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Remove(Product product);
    Task<bool> SlugExistsAsync(string slug, int? exceptId = null, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task IncrementViewAsync(int productId, CancellationToken ct = default);
}

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(bool onlyActive = true, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetWithProductCountsAsync(bool onlyActive = true, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Update(Category category);
    void Remove(Category category);
    Task<bool> SlugExistsAsync(string slug, int? exceptId = null, CancellationToken ct = default);
    Task<bool> HasProductsAsync(int categoryId, CancellationToken ct = default);
}

public interface ICartRepository
{
    /// <summary>سبد خرید کاربر/مهمان را پیدا کرده و در صورت نبود، ایجاد می‌کند.</summary>
    Task<Cart> GetOrCreateAsync(string cartKey, int? userId, CancellationToken ct = default);
    Task<Cart?> FindAsync(string cartKey, int? userId, CancellationToken ct = default);
    Task<Cart?> GetWithItemsAsync(int cartId, CancellationToken ct = default);
    Task<Cart?> GetByUserAsync(int userId, CancellationToken ct = default);
    void Update(Cart cart);
    Task AddItemAsync(CartItem item, CancellationToken ct = default);
    void UpdateItem(CartItem item);
    void RemoveItem(CartItem item);
    void RemoveItems(IEnumerable<CartItem> items);
    Task<int> CountItemsAsync(string cartKey, int? userId, CancellationToken ct = default);
    Task<int> CountItemsByUserAsync(int userId, CancellationToken ct = default);
    Task MergeGuestCartAsync(string cartKey, int userId, CancellationToken ct = default);
    Task ClearItemsAsync(int cartId, CancellationToken ct = default);
}

public interface IOrderRepository
{
    Task<PagedResult<Order>> GetUserOrdersAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Order>> SearchAsync(AdminOrderFilter filter, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(int id, bool includeItems = true, CancellationToken ct = default);
    Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    Task<IReadOnlyList<Order>> GetRecentAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetPaidOrdersSinceAsync(DateTime fromUtc, CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<int> CountAsync(OrderStatus? status = null, CancellationToken ct = default);
    Task<IReadOnlyList<(int ProductId, string Name, int Quantity, decimal Revenue)>> GetTopProductsAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<(string CategoryName, int Quantity, decimal Revenue)>> GetSalesByCategoryAsync(CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByExternalKeyAsync(string provider, string key, CancellationToken ct = default);
    Task<PagedResult<User>> SearchAsync(AdminUserFilter filter, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    Task<bool> EmailExistsAsync(string email, int? exceptId = null, CancellationToken ct = default);
    Task<int> CountAsync(bool onlyCustomers = true, CancellationToken ct = default);
    Task<int> CountNewSinceAsync(DateTime fromUtc, CancellationToken ct = default);
    Task<(int OrdersCount, decimal TotalSpent)> GetUserStatsAsync(int userId, CancellationToken ct = default);
}

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetByProductAsync(int productId, bool onlyApproved = true, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetLatestAsync(int take, bool onlyApproved = true, CancellationToken ct = default);
    Task<PagedResult<Review>> SearchAsync(string? search, ReviewStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<Review?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> HasUserReviewedAsync(int productId, int userId, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    void Update(Review review);
    void Remove(Review review);
    Task<(double Average, int Count)> GetRatingAsync(int productId, CancellationToken ct = default);
    Task<int> CountAsync(ReviewStatus? status = null, CancellationToken ct = default);
}

public interface IAddressRepository
{
    Task<IReadOnlyList<Address>> GetUserAddressesAsync(int userId, CancellationToken ct = default);
    Task<Address?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Address address, CancellationToken ct = default);
    void Update(Address address);
    void Remove(Address address);
    Task ClearDefaultAsync(int userId, CancellationToken ct = default);
}

public interface IDiscountCodeRepository
{
    Task<DiscountCode?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<DiscountCode?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DiscountCode>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(DiscountCode code, CancellationToken ct = default);
    void Update(DiscountCode code);
    void Remove(DiscountCode code);
    Task<bool> CodeExistsAsync(string code, int? exceptId = null, CancellationToken ct = default);
}

public interface IFavoriteRepository
{
    Task<IReadOnlyList<Favorite>> GetUserFavoritesAsync(int userId, CancellationToken ct = default);
    Task<bool> IsFavoriteAsync(int userId, int productId, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetFavoriteProductIdsAsync(int userId, CancellationToken ct = default);
    Task<Favorite?> GetAsync(int userId, int productId, CancellationToken ct = default);
    Task AddAsync(Favorite favorite, CancellationToken ct = default);
    void Remove(Favorite favorite);
}

public interface ISettingRepository
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, string?>> GetAllAsync(CancellationToken ct = default);
    Task SetAsync(string key, string? value, CancellationToken ct = default);
    Task SetManyAsync(IReadOnlyDictionary<string, string?> values, CancellationToken ct = default);
}

public interface IPaymentGateway
{
    /// <summary>شروع فرآیند پرداخت و دریافت توکن/شناسه تراکنش.</summary>
    Task<PaymentRequestResult> RequestPaymentAsync(decimal amount, string orderNumber, string callbackUrl, CancellationToken ct = default);

    /// <summary>تأیید پرداخت پس از بازگشت کاربر از درگاه.</summary>
    Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, decimal amount, CancellationToken ct = default);
}

public class PaymentRequestResult
{
    public bool Success { get; init; }
    public string? Authority { get; init; }
    public string? RedirectUrl { get; init; }
    public string? Error { get; init; }
}

public class PaymentVerifyResult
{
    public bool Success { get; init; }
    public string? ReferenceId { get; init; }
    public string? Error { get; init; }
}

public interface IFileStorage
{
    /// <summary>ذخیره تصویر آپلودشده و بازگشت آدرس نسبی آن.</summary>
    Task<string> SaveImageAsync(Stream content, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string relativeUrl, CancellationToken ct = default);
}
