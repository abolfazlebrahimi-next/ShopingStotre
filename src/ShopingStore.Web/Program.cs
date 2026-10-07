using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Infrastructure;
using ShopingStore.Infrastructure.Data;
using ShopingStore.Web.Endpoints;
using ShopingStore.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// امکان بازنویسی تنظیمات محلی بدون دست‌زدن به appsettings.json
// (این فایل در .gitignore قرار دارد؛ نمونه آن: appsettings.Local.json.example)
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json"),
    optional: true,
    reloadOnChange: true);

// ------------------------------------------------------------------ سرویس‌ها
builder.Services.AddShopingStoreInfrastructure(builder.Configuration, builder.Environment.WebRootPath);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<CartIdentity>();
builder.Services.AddScoped<ToastService>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "StaffOnly");
    options.Conventions.AuthorizeFolder("/Account", "CustomerOnly");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "ShopingStore.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Support"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "ShopingStore.Antiforgery";
});

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(180);
    options.IncludeSubDomains = true;
});

// فرهنگ فارسی برای کل برنامه (تاریخ شمسی، اعداد و فرمت‌ها)
var culture = new CultureInfo("fa-IR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture);
    options.SupportedCultures = new[] { culture };
    options.SupportedUICultures = new[] { culture };
});

var app = builder.Build();

// ---------------------------------------------------------------- میان‌افزار
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseResponseCompression();
app.UseRequestLocalization();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorPages();
app.MapHomeEndpoints();
app.MapCatalogEndpoints();
app.MapCartEndpoints();
app.MapCheckoutEndpoints();
app.MapAccountEndpoints();
app.MapApiEndpoints();
app.MapAdminEndpoints();

// ------------------------------------------------- آماده‌سازی دیتابیس و داده‌ها
await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // اگر دیتابیس در دسترس نباشد، اجرای برنامه را بیش از ۳۰ ثانیه منتظر نگه نمی‌داریم.
    using var startupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    try
    {
        await initializer.InitializeAsync(startupTimeout.Token);
    }
    catch (OperationCanceledException)
    {
        logger.LogWarning("زمان آماده‌سازی دیتابیس به پایان رسید؛ برنامه بدون داده اولیه ادامه می‌دهد. " +
                          "برای رفع مشکل: docker compose up -d یا تنظیم ConnectionStrings:Default در appsettings.Local.json");
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "خطا در آماده‌سازی دیتابیس. بررسی کنید SQL Server در دسترس باشد.");
    }
}

app.Run();
