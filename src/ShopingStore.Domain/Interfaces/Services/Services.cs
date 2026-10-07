using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;

namespace ShopingStore.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<User> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<User> LoginOrCreateExternalAsync(string provider, string key, string email, string fullName, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<User> UpdateProfileAsync(int userId, string fullName, string? phoneNumber, string? nationalCode, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserProfileDto?> GetProfileAsync(int id, CancellationToken ct = default);
}

public interface ICatalogService
{
    Task<HomePageDto> GetHomePageAsync(int? userId = null, CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> SearchProductsAsync(ProductQuery query, int? userId = null, CancellationToken ct = default);
    Task<ProductDetailsDto?> GetProductDetailsAsync(string slug, int? userId = null, bool increaseViewCount = true, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetCategoryTreeAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductListItemDto>> GetSuggestionsAsync(string term, int take = 6, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken ct = default);
    Task<(decimal Min, decimal Max)> GetPriceRangeAsync(CancellationToken ct = default);
}

public interface ICartService
{
    Task<CartDto> GetCartAsync(string cartKey, int? userId, CancellationToken ct = default);
    Task<CartDto> AddItemAsync(string cartKey, int? userId, int productId, int quantity = 1, string? variant = null, CancellationToken ct = default);
    Task<CartDto> UpdateQuantityAsync(string cartKey, int? userId, int cartItemId, int quantity, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(string cartKey, int? userId, int cartItemId, CancellationToken ct = default);
    Task<CartDto> ClearAsync(string cartKey, int? userId, CancellationToken ct = default);
    Task<(bool Success, string? Message, CartDto Cart)> ApplyDiscountAsync(string cartKey, int? userId, string code, CancellationToken ct = default);
    Task<CartDto> RemoveDiscountAsync(string cartKey, int? userId, CancellationToken ct = default);
    Task<int> GetItemsCountAsync(string cartKey, int? userId, CancellationToken ct = default);
    Task MoveGuestCartToUserAsync(string cartKey, int userId, CancellationToken ct = default);
}

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(string cartKey, int userId, CheckoutRequest request, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetUserOrdersAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<OrderDto?> GetOrderAsync(int orderId, int? userId = null, CancellationToken ct = default);
    Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, int? userId = null, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> SearchAsync(AdminOrderFilter filter, CancellationToken ct = default);
    Task UpdateStatusAsync(UpdateOrderStatusRequest request, string actor, CancellationToken ct = default);
    Task<PaymentRequestResult> StartPaymentAsync(int orderId, string callbackUrl, CancellationToken ct = default);
    Task<bool> ConfirmOnlinePaymentAsync(string orderNumber, string authority, CancellationToken ct = default);
    Task CancelAsync(int orderId, int? userId, string? reason = null, CancellationToken ct = default);
    Task<bool> CanUserReviewProductAsync(int userId, int productId, CancellationToken ct = default);
}

public interface IReviewService
{
    Task<ReviewDto> AddAsync(int userId, AddReviewRequest request, CancellationToken ct = default);
    Task<PagedResult<ReviewDto>> SearchAsync(string? search, ReviewStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task ApproveAsync(int reviewId, CancellationToken ct = default);
    Task RejectAsync(int reviewId, CancellationToken ct = default);
    Task DeleteAsync(int reviewId, CancellationToken ct = default);
}

public interface IWishlistService
{
    Task<IReadOnlyList<ProductListItemDto>> GetFavoritesAsync(int userId, CancellationToken ct = default);
    Task<bool> ToggleAsync(int userId, int productId, CancellationToken ct = default);
    Task<IReadOnlySet<int>> GetFavoriteIdsAsync(int? userId, CancellationToken ct = default);
}

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(int userId, CancellationToken ct = default);
    Task<AddressDto> SaveAsync(int userId, SaveAddressRequest request, CancellationToken ct = default);
    Task DeleteAsync(int userId, int addressId, CancellationToken ct = default);
    Task SetDefaultAsync(int userId, int addressId, CancellationToken ct = default);
}

public interface IAdminProductService
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(ProductQuery query, bool includeInactive, CancellationToken ct = default);
    Task<Product?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> SaveAsync(SaveProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task ToggleActiveAsync(int id, CancellationToken ct = default);
}

public interface IAdminCategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> SaveAsync(SaveCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IAdminUserService
{
    Task<PagedResult<UserProfileDto>> SearchAsync(AdminUserFilter filter, CancellationToken ct = default);
    Task<UserProfileDto?> GetAsync(int id, CancellationToken ct = default);
    Task ToggleActiveAsync(int id, CancellationToken ct = default);
    Task ChangeRoleAsync(int id, UserRole role, CancellationToken ct = default);
}

public interface IDiscountCodeService
{
    Task<IReadOnlyList<DiscountCodeDto>> GetAllAsync(CancellationToken ct = default);
    Task<DiscountCode?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> SaveAsync(SaveDiscountCodeRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<(bool Success, string? Message, decimal Amount)> ValidateAsync(string code, decimal cartSubtotal, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int chartDays = 14, CancellationToken ct = default);
}

public interface ISettingsService
{
    Task<StoreSettingsDto> GetAsync(CancellationToken ct = default);
    Task SaveAsync(StoreSettingsDto settings, CancellationToken ct = default);
}

public interface INotificationService
{
    Task SendOrderConfirmationAsync(OrderDto order, CancellationToken ct = default);
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default);
    Task SendEmailAsync(string email, string subject, string body, CancellationToken ct = default);
}
