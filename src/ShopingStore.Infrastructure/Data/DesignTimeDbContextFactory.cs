using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopingStore.Infrastructure.Data;

/// <summary>
/// این کلاس فقط در زمان طراحی (اجرای دستورات dotnet ef) استفاده می‌شود.
/// دستور ساخت مایگریشن:
///     dotnet ef migrations add InitialCreate --project src/ShopingStore.Infrastructure --startup-project src/ShopingStore.Web
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SHOPINGSTORE_CONNECTION")
            ?? "Server=localhost,1433;Database=ShopingStoreDb;User Id=sa;Password=Shop@Store12345;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        return new ApplicationDbContext(options);
    }
}
