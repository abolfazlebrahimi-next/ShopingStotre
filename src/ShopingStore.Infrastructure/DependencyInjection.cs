using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Data;
using ShopingStore.Infrastructure.Payment;
using ShopingStore.Infrastructure.Repositories;
using ShopingStore.Infrastructure.Security;
using ShopingStore.Infrastructure.Services;
using ShopingStore.Infrastructure.Storage;

namespace ShopingStore.Infrastructure;

/// <summary>ثبت تمام سرویس‌ها، مخازن و تنظیمات لایه زیرساخت در DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddShopingStoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string webRootPath)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Server=localhost,1433;Database=ShopingStoreDb;User Id=sa;Password=Shop@Store12345;TrustServerCertificate=True;MultipleActiveResultSets=true";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3);
                sql.CommandTimeout(60);
            }));

        // ------------------------------------------------------------ زیرساخت
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPaymentGateway, DemoPaymentGateway>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        services.Configure<StorageOptions>(options =>
        {
            options.RootPath = Path.Combine(string.IsNullOrWhiteSpace(webRootPath) ? "wwwroot" : webRootPath, "uploads");
            options.PublicBasePath = "/uploads";
        });

        // ------------------------------------------------------------ مخازن
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();

        // ------------------------------------------------------------ سرویس‌ها
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IAdminProductService, AdminProductService>();
        services.AddScoped<IAdminCategoryService, AdminCategoryService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IDiscountCodeService, DiscountCodeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddScoped<DbInitializer>();

        return services;
    }
}
