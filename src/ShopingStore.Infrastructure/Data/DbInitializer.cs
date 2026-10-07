using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Infrastructure.Security;

namespace ShopingStore.Infrastructure.Data;

/// <summary>
/// ساخت خودکار دیتابیس، اعمال مایگریشن‌ها و درج داده‌های اولیه (دسته‌بندی، محصول، کاربر، کد تخفیف و تنظیمات).
/// این کلاس در شروع برنامه یک بار اجرا می‌شود و اجرای مجدد آن بی‌خطر است.
/// </summary>
public class DbInitializer
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(ApplicationDbContext db, ILogger<DbInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (await _db.Database.CanConnectAsync(ct))
        {
            _logger.LogInformation("اتصال به دیتابیس برقرار است.");
        }
        else
        {
            _logger.LogWarning("امکان اتصال به دیتابیس وجود ندارد؛ لطفاً رشته اتصال (ConnectionStrings:Default) را بررسی کنید.");
            return;
        }

        var migrations = (await _db.Database.GetMigrationsAsync(ct)).ToList();

        if (migrations.Count == 0)
        {
            // اگر هنوز مایگریشنی ساخته نشده باشد، دیتابیس از روی مدل ساخته می‌شود.
            await _db.Database.EnsureCreatedAsync(ct);
            _logger.LogInformation("دیتابیس از روی مدل ساخته شد (بدون مایگریشن).");
        }
        else if ((await _db.Database.GetPendingMigrationsAsync(ct)).Any())
        {
            await _db.Database.MigrateAsync(ct);
            _logger.LogInformation("مایگریشن‌ها با موفقیت اعمال شدند.");
        }

        await SeedSettingsAsync(ct);
        await SeedUsersAsync(ct);
        await SeedCategoriesAsync(ct);
        await SeedProductsAsync(ct);
        await SeedDiscountCodesAsync(ct);
        await SeedReviewsAsync(ct);
        await SeedOrdersAsync(ct);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("داده‌های اولیه آماده است.");
    }

    // -------------------------------------------------------------- تنظیمات
    private async Task SeedSettingsAsync(CancellationToken ct)
    {
        if (await _db.Settings.AnyAsync(ct)) return;

        var defaults = new StoreSettingsDto();

        await _db.Settings.AddRangeAsync(new[]
        {
            new Setting { Key = SettingKeys.SiteName, Value = defaults.SiteName, Description = "نام فروشگاه" },
            new Setting { Key = SettingKeys.SiteSlogan, Value = defaults.SiteSlogan, Description = "شعار فروشگاه" },
            new Setting { Key = SettingKeys.SupportPhone, Value = defaults.SupportPhone, Description = "تلفن پشتیبانی" },
            new Setting { Key = SettingKeys.SupportEmail, Value = defaults.SupportEmail, Description = "ایمیل پشتیبانی" },
            new Setting { Key = SettingKeys.Address, Value = defaults.Address, Description = "نشانی فروشگاه" },
            new Setting { Key = SettingKeys.Instagram, Value = defaults.Instagram, Description = "اینستاگرام" },
            new Setting { Key = SettingKeys.Telegram, Value = defaults.Telegram, Description = "تلگرام" },
            new Setting { Key = SettingKeys.ShippingCost, Value = defaults.ShippingCost.ToString(), Description = "هزینه ارسال (تومان)" },
            new Setting { Key = SettingKeys.FreeShippingThreshold, Value = defaults.FreeShippingThreshold.ToString(), Description = "سقف ارسال رایگان (تومان)" },
            new Setting { Key = SettingKeys.TaxPercent, Value = defaults.TaxPercent.ToString(), Description = "درصد مالیات بر ارزش افزوده" },
            new Setting { Key = SettingKeys.BankName, Value = defaults.BankName, Description = "نام بانک" },
            new Setting { Key = SettingKeys.CardNumber, Value = defaults.CardNumber, Description = "شماره کارت" },
            new Setting { Key = SettingKeys.CardOwner, Value = defaults.CardOwner, Description = "صاحب کارت" },
            new Setting { Key = SettingKeys.CashOnDeliveryFee, Value = defaults.CashOnDeliveryFee.ToString(), Description = "کارمزد پرداخت در محل (تومان)" }
        }, ct);
    }

    // ----------------------------------------------------------------- کاربر
    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(ct)) return;

        _db.Users.AddRange(
            new User
            {
                FullName = "ابوالفضل ابراهیمی",
                Email = "admin@shopingstore.ir",
                PhoneNumber = "09120000001",
                PasswordHash = PasswordHasher.Hash("Admin@12345"),
                Role = UserRole.Admin,
                IsActive = true
            },
            new User
            {
                FullName = "کارشناس پشتیبانی",
                Email = "support@shopingstore.ir",
                PhoneNumber = "09120000002",
                PasswordHash = PasswordHasher.Hash("Support@12345"),
                Role = UserRole.Support,
                IsActive = true
            },
            new User
            {
                FullName = "زهرا محمدی",
                Email = "customer@shopingstore.ir",
                PhoneNumber = "09120000003",
                PasswordHash = PasswordHasher.Hash("Customer@12345"),
                Role = UserRole.Customer,
                IsActive = true,
                Addresses = new List<Address>
                {
                    new()
                    {
                        Title = "خانه", ReceiverName = "زهرا محمدی", PhoneNumber = "09120000003",
                        Province = "تهران", City = "تهران", PostalCode = "1234567890",
                        Line = "خیابان ولیعصر، کوچه بهار، پلاک ۱۲، واحد ۳", IsDefault = true
                    },
                    new()
                    {
                        Title = "محل کار", ReceiverName = "زهرا محمدی", PhoneNumber = "09120000003",
                        Province = "تهران", City = "تهران", PostalCode = "1111111111",
                        Line = "میدان آرژانتین، برج نگین، طبقه ۵"
                    }
                }
            },
            new User
            {
                FullName = "محمد رضایی",
                Email = "mohammad@example.com",
                PhoneNumber = "09120000004",
                PasswordHash = PasswordHasher.Hash("Customer@12345"),
                Role = UserRole.Customer,
                IsActive = true
            },
            new User
            {
                FullName = "سارا کریمی",
                Email = "sara@example.com",
                PhoneNumber = "09120000005",
                PasswordHash = PasswordHasher.Hash("Customer@12345"),
                Role = UserRole.Customer,
                IsActive = true
            });
    }

    // ------------------------------------------------------------ دسته‌بندی
    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        if (await _db.Categories.AnyAsync(ct)) return;

        var categories = new[]
        {
            new Category { Name = "موبایل و تبلت", Slug = "mobile-tablet", Icon = "mobile", SortOrder = 1, Description = "گوشی موبایل، تبلت و لوازم جانبی" },
            new Category { Name = "لپ‌تاپ و کامپیوتر", Slug = "laptop-computer", Icon = "laptop", SortOrder = 2, Description = "لپ‌تاپ، کامپیوتر، مانیتور و قطعات" },
            new Category { Name = "هدفون و صوتی", Slug = "audio-headphone", Icon = "headphone", SortOrder = 3, Description = "هدفون، هندزفری، اسپیکر و سیستم صوتی" },
            new Category { Name = "ساعت و مچ‌بند هوشمند", Slug = "smart-watch", Icon = "watch", SortOrder = 4, Description = "ساعت هوشمند و مچ‌بند سلامتی" },
            new Category { Name = "لوازم خانگی", Slug = "home-appliance", Icon = "home", SortOrder = 5, Description = "لوازم برقی و آشپزخانه" },
            new Category { Name = "مد و پوشاک", Slug = "fashion", Icon = "shirt", SortOrder = 6, Description = "پوشاک مردانه، زنانه و بچگانه" },
            new Category { Name = "زیبایی و سلامت", Slug = "beauty-health", Icon = "sparkle", SortOrder = 7, Description = "لوازم آرایشی، بهداشتی و مراقبت پوست" },
            new Category { Name = "ورزش و سفر", Slug = "sport-travel", Icon = "ball", SortOrder = 8, Description = "لوازم ورزشی، کوهنوردی و سفر" }
        };

        await _db.Categories.AddRangeAsync(categories, ct);
        await _db.SaveChangesAsync(ct);

        var subCategories = new List<Category>();
        void AddSub(string parentSlug, params string[] names)
        {
            var parent = categories.First(c => c.Slug == parentSlug);
            foreach (var name in names)
                subCategories.Add(new Category
                {
                    Name = name,
                    Slug = SlugHelper.Generate($"{parent.Slug}-{name}"),
                    ParentId = parent.Id,
                    SortOrder = 1
                });
        }

        AddSub("mobile-tablet", "گوشی موبایل", "تبلت", "لوازم جانبی موبایل");
        AddSub("laptop-computer", "لپ‌تاپ", "مانیتور", "کیبورد و ماوس");
        AddSub("audio-headphone", "هدفون", "اسپیکر");
        AddSub("home-appliance", "چای‌ساز و کتری برقی", "جاروبرقی", "مخلوط‌کن");

        await _db.Categories.AddRangeAsync(subCategories, ct);
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------- محصولات
    private async Task SeedProductsAsync(CancellationToken ct)
    {
        if (await _db.Products.AnyAsync(ct)) return;

        var categories = await _db.Categories.ToDictionaryAsync(c => c.Slug, ct);

        var products = new List<Product>
        {
            // ---------------------------------------------------- موبایل
            Product1(categories["mobile-tablet"], "گوشی موبایل سامسونگ مدل Galaxy S24 Ultra ظرفیت ۲۵۶ گیگابایت", "samsung-galaxy-s24-ultra", "سامسونگ", 74_900_000, 69_900_000, 12, 4.8),
            Product1(categories["mobile-tablet"], "گوشی موبایل شیائومی مدل Redmi Note 13 Pro ظرفیت ۱۲۸ گیگابایت", "xiaomi-redmi-note-13-pro", "شیائومی", 18_900_000, 17_200_000, 35, 4.6),
            Product1(categories["mobile-tablet"], "گوشی موبایل اپل مدل iPhone 15 Pro Max ظرفیت ۲۵۶ گیگابایت", "apple-iphone-15-pro-max", "اپل", 96_500_000, null, 8, 4.9),
            Product1(categories["mobile-tablet"], "گوشی موبایل سامسونگ مدل Galaxy A55 ظرفیت ۲۵۶ گیگابایت", "samsung-galaxy-a55", "سامسونگ", 24_300_000, 22_900_000, 22, 4.4),
            Product1(categories["mobile-tablet"], "تبلت لنوو مدل Tab M11 ظرفیت ۱۲۸ گیگابایت", "lenovo-tab-m11", "لنوو", 15_800_000, 14_200_000, 18, 4.2),
            Product1(categories["mobile-tablet"], "گوشی موبایل هواوی مدل Nova 12i ظرفیت ۱۲۸ گیگابایت", "huawei-nova-12i", "هواوی", 13_500_000, 12_400_000, 30, 4.1),

            // ---------------------------------------------------- لپ‌تاپ
            Product1(categories["laptop-computer"], "لپ‌تاپ ایسوس مدل Zenbook 14 OLED پردازنده Core Ultra 7 رم ۱۶ گیگابایت", "asus-zenbook-14-oled", "ایسوس", 79_500_000, 74_900_000, 6, 4.7),
            Product1(categories["laptop-computer"], "لپ‌تاپ لنوو مدل IdeaPad Slim 3 پردازنده Core i5 رم ۱۶ گیگابایت", "lenovo-ideapad-slim-3", "لنوو", 34_900_000, 32_500_000, 14, 4.3),
            Product1(categories["laptop-computer"], "لپ‌تاپ اپل مدل MacBook Air M3 نمایشگر ۱۳.۶ اینچ رم ۸ گیگابایت", "apple-macbook-air-m3", "اپل", 82_000_000, null, 5, 4.9),
            Product1(categories["laptop-computer"], "مانیتور گیمینگ سامسونگ مدل Odyssey G5 سایز ۲۷ اینچ", "samsung-odyssey-g5-27", "سامسونگ", 21_400_000, 19_800_000, 10, 4.5),
            Product1(categories["laptop-computer"], "کیبورد مکانیکال ردراگون مدل Kumara K552", "redragon-kumara-k552", "ردراگون", 2_450_000, 2_150_000, 60, 4.4),
            Product1(categories["laptop-computer"], "ماوس گیمینگ لاجیتک مدل G502 Hero", "logitech-g502-hero", "لاجیتک", 4_300_000, 3_890_000, 42, 4.6),

            // ---------------------------------------------------- صوتی
            Product1(categories["audio-headphone"], "هدفون بی‌سیم سونی مدل WH-1000XM5 نویز کنسلینگ", "sony-wh-1000xm5", "سونی", 26_900_000, 24_500_000, 16, 4.8),
            Product1(categories["audio-headphone"], "هندزفری بلوتوثی اپل مدل AirPods Pro نسل دوم", "apple-airpods-pro-2", "اپل", 21_500_000, 19_900_000, 25, 4.7),
            Product1(categories["audio-headphone"], "هندزفری بی‌سیم شیائومی مدل Redmi Buds 5", "xiaomi-redmi-buds-5", "شیائومی", 2_890_000, 2_450_000, 80, 4.3),
            Product1(categories["audio-headphone"], "اسپیکر بلوتوثی جی‌بی‌ال مدل Charge 5", "jbl-charge-5", "جی‌بی‌ال", 12_400_000, 11_200_000, 20, 4.6),
            Product1(categories["audio-headphone"], "اسپیکر بلوتوثی انکر مدل Soundcore 3", "anker-soundcore-3", "انکر", 4_150_000, 3_650_000, 34, 4.2),

            // ---------------------------------------------------- ساعت هوشمند
            Product1(categories["smart-watch"], "ساعت هوشمند اپل مدل Watch Series 9 سایز ۴۵ میلی‌متری", "apple-watch-series-9", "اپل", 31_900_000, 29_900_000, 9, 4.8),
            Product1(categories["smart-watch"], "ساعت هوشمند سامسونگ مدل Galaxy Watch 6", "samsung-galaxy-watch-6", "سامسونگ", 18_700_000, 16_900_000, 15, 4.5),
            Product1(categories["smart-watch"], "مچ‌بند هوشمند شیائومی مدل Smart Band 8", "xiaomi-smart-band-8", "شیائومی", 1_950_000, 1_690_000, 120, 4.4),
            Product1(categories["smart-watch"], "ساعت هوشمند هوآوی مدل Watch Fit 3", "huawei-watch-fit-3", "هواوی", 5_400_000, 4_890_000, 40, 4.3),

            // ---------------------------------------------------- لوازم خانگی
            Product1(categories["home-appliance"], "چای‌ساز پارس‌خزر مدل ۲۹۰۸ ظرفیت ۱.۸ لیتر", "parskhazar-tea-maker-2908", "پارس‌خزر", 4_890_000, 4_450_000, 30, 4.5),
            Product1(categories["home-appliance"], "جاروبرقی فیلیپس مدل PowerPro Compact 2000 وات", "philips-powerpro-compact", "فیلیپس", 8_750_000, 7_990_000, 12, 4.4),
            Product1(categories["home-appliance"], "مخلوط‌کن و غذا ساز بوش مدل MCM3501 ظرفیت ۸۰۰ وات", "bosch-mcm3501", "بوش", 12_300_000, 11_450_000, 7, 4.6),
            Product1(categories["home-appliance"], "کتری برقی سان‌پاک مدل SK-1700 ظرفیت ۱.۷ لیتر", "sanpak-sk-1700", "سان‌پاک", 2_150_000, 1_890_000, 55, 4.2),
            Product1(categories["home-appliance"], "سرخ‌کن بدون روغن فیلیپس مدل HD9200 ظرفیت ۴.۱ لیتر", "philips-airfryer-hd9200", "فیلیپس", 15_600_000, 14_200_000, 11, 4.7),

            // ---------------------------------------------------- پوشاک
            Product1(categories["fashion"], "هودی مردانه زنانه طرح کلاسیک رنگ سرمه‌ای", "hoodie-classic-navy", "شاپینگ‌استور", 1_850_000, 1_490_000, 90, 4.1),
            Product1(categories["fashion"], "کتونی اسپرت مردانه مدل Runner Pro", "sneaker-runner-pro", "شاپینگ‌استور", 3_450_000, 2_990_000, 45, 4.3),
            Product1(categories["fashion"], "کیف چرم زنانه دست‌دوز مدل ونوس", "bag-venus-leather", "شاپینگ‌استور", 2_890_000, 2_450_000, 38, 4.5),
            Product1(categories["fashion"], "مانتو اداری زنانه جنس کرپ رنگ مشکی", "manto-office-black", "شاپینگ‌استور", 2_250_000, null, 60, 4.0),

            // ---------------------------------------------------- زیبایی و سلامت
            Product1(categories["beauty-health"], "سرم ویتامین C روشن‌کننده ۳۰ میلی‌لیتر", "vitamin-c-serum", "لورآل", 1_250_000, 990_000, 100, 4.4),
            Product1(categories["beauty-health"], "کرم ضد آفتاب SPF50 بدون رنگ ۵۰ میلی‌لیتر", "sunscreen-spf50", "سینره", 890_000, 745_000, 130, 4.6),
            Product1(categories["beauty-health"], "سشوار فیلیپس مدل BHD300 ظرفیت ۲۱۰۰ وات", "philips-hairdryer-bhd300", "فیلیپس", 5_450_000, 4_990_000, 24, 4.3),
            Product1(categories["beauty-health"], "ماشین اصلاح صورت فیلیپس مدل QT3310", "philips-shaver-qt3310", "فیلیپس", 2_450_000, 2_190_000, 50, 4.1),

            // ---------------------------------------------------- ورزش و سفر
            Product1(categories["sport-travel"], "دمبل قابل تنظیم ۲۰ کیلوگرمی تمرین خانگی", "adjustable-dumbbell-20", "بدنسازی", 6_800_000, 5_990_000, 18, 4.2),
            Product1(categories["sport-travel"], "مت یوگا ضد لغزش ضخامت ۸ میلی‌متر", "yoga-mat-8mm", "ورزشی", 1_450_000, 1_190_000, 75, 4.0),
            Product1(categories["sport-travel"], "کوله پشتی کوهنوردی ۴۵ لیتری آب‌گریز", "backpack-45l", "کوه‌نوردی", 3_250_000, 2_890_000, 40, 4.5),
            Product1(categories["sport-travel"], "بطری آب ورزشی استیل ۷۵۰ میلی‌لیتر", "steel-bottle-750", "ورزشی", 690_000, 590_000, 150, 4.1),
            Product1(categories["sport-travel"], "جامدادی و لوازم‌التحریر کامل دانش‌آموزی", "pencil-case-set", "لوازم‌التحریر", 850_000, 720_000, 200, 4.0)
        };

        // ویژگی‌ها و تنوع‌های نمونه برای چند محصول
        AddSpecs(products, "samsung-galaxy-s24-ultra", specs: new[]
        {
            ("حافظه داخلی", "۲۵۶ گیگابایت"), ("رم", "۱۲ گیگابایت"), ("اندازه نمایشگر", "۶.۸ اینچ"),
            ("دوربین اصلی", "۲۰۰ مگاپیکسل"), ("باتری", "۵۰۰۰ میلی‌آمپرساعت"), ("رنگ", "تیتانیوم خاکستری")
        }, variants: new[]
        {
            ("رنگ", new[] { "تیتانیوم خاکستری", "تیتانیوم مشکی", "بنفش" }),
            ("گارانتی", new[] { "۱۸ ماه شرکتی", "۲۴ ماه بین‌الملل" })
        });

        AddSpecs(products, "xiaomi-redmi-note-13-pro", specs: new[]
        {
            ("حافظه داخلی", "۱۲۸ گیگابایت"), ("رم", "۸ گیگابایت"), ("اندازه نمایشگر", "۶.۶۷ اینچ"),
            ("دوربین اصلی", "۲۰۰ مگاپیکسل"), ("باتری", "۵۱۰۰ میلی‌آمپرساعت")
        }, variants: new[] { ("رنگ", new[] { "مشکی", "آبی", "بنفش" }) });

        AddSpecs(products, "apple-iphone-15-pro-max", specs: new[]
        {
            ("حافظه داخلی", "۲۵۶ گیگابایت"), ("اندازه نمایشگر", "۶.۷ اینچ"), ("جنس بدنه", "تیتانیوم"),
            ("دوربین اصلی", "۴۸ مگاپیکسل"), ("شارژ سریع", "دارد")
        }, variants: new[] { ("رنگ", new[] { "تیتانیوم طبیعی", "تیتانیوم آبی", "تیتانیوم مشکی" }) });

        AddSpecs(products, "asus-zenbook-14-oled", specs: new[]
        {
            ("پردازنده", "Intel Core Ultra 7"), ("رم", "۱۶ گیگابایت"), ("حافظه", "۵۱۲ گیگابایت SSD"),
            ("نمایشگر", "۱۴ اینچ OLED تاچ"), ("وزن", "۱.۲ کیلوگرم")
        }, variants: new[] { ("رنگ", new[] { "خاکستری", "آبی" }) });

        AddSpecs(products, "sony-wh-1000xm5", specs: new[]
        {
            ("نوع اتصال", "بلوتوث ۵.۲"), ("نویز کنسلینگ", "فعال (ANC)"), ("زمان شارژدهی", "تا ۳۰ ساعت"),
            ("وزن", "۲۵۰ گرم")
        }, variants: new[] { ("رنگ", new[] { "مشکی", "نقره‌ای", "کرم" }) });

        AddSpecs(products, "hoodie-classic-navy", specs: new[]
        {
            ("جنس", "پنبه درجه یک"), ("نوع یقه", "کلاه‌دار"), ("فصل", "پاییز و زمستان")
        }, variants: new[]
        {
            ("سایز", new[] { "S", "M", "L", "XL", "XXL" }),
            ("رنگ", new[] { "سرمه‌ای", "مشکی", "طوسی" })
        });

        await _db.Products.AddRangeAsync(products, ct);
        await _db.SaveChangesAsync(ct);
        SeedProductImages(products);
    }

    private static void SeedProductImages(IEnumerable<Product> products)
    {
        var extraImages = new Dictionary<string, string[]>
        {
            ["samsung-galaxy-s24-ultra"] = new[] { "/img/products/product-1.svg", "/img/products/product-2.svg" },
            ["apple-iphone-15-pro-max"] = new[] { "/img/products/product-3.svg" },
            ["sony-wh-1000xm5"] = new[] { "/img/products/product-4.svg" },
            ["asus-zenbook-14-oled"] = new[] { "/img/products/product-5.svg" }
        };

        foreach (var product in products)
        {
            if (!extraImages.TryGetValue(product.Slug, out var images)) continue;

            var order = 1;
            foreach (var url in images)
                product.Images.Add(new ProductImage { ProductId = product.Id, Url = url, Alt = product.Name, SortOrder = order++ });
        }
    }

    private static void AddSpecs(
        IEnumerable<Product> products,
        string slug,
        (string Name, string Value)[] specs,
        (string Name, string[] Options)[] variants)
    {
        var product = products.FirstOrDefault(p => p.Slug == slug);
        if (product is null) return;

        product.Specifications = specs.Select(s => new ProductSpecification { Name = s.Name, Value = s.Value }).ToList();
        product.Variants = variants.Select(v => new ProductVariant { Name = v.Name, Options = v.Options.ToList() }).ToList();
    }

    /// <summary>ساخت سریع محصول با مقادیر پیش‌فرض.</summary>
    private static Product Product1(
        Category category,
        string name,
        string slug,
        string brand,
        decimal price,
        decimal? discountPrice,
        int stock,
        double rating)
        => new()
        {
            Name = name,
            Slug = slug,
            Brand = brand,
            CategoryId = category.Id,
            Category = category,
            Price = price,
            DiscountPrice = discountPrice,
            Stock = stock,
            Rating = rating,
            IsActive = true,
            IsFeatured = stock % 5 == 0,
            IsNew = stock % 7 == 0,
            Sku = $"SKU-{Math.Abs(slug.GetHashCode() % 900000) + 100000}",
            MainImageUrl = $"/img/products/product-{Math.Abs(slug.GetHashCode() % 8) + 1}.svg",
            ShortDescription = $"{name} با کیفیت اورجینال، گارانتی معتبر و ارسال سریع به سراسر کشور.",
            Description = $"<p>{name} یکی از محبوب‌ترین محصولات دسته «{category.Name}» در فروشگاه ما است. " +
                          "این محصول با کیفیت ساخت بالا، خدمات پس از فروش معتبر و قیمت مناسب عرضه می‌شود.</p>" +
                          "<ul><li>اصالت کالا تضمین شده است</li><li>ارسال سریع به تمام نقاط کشور</li>" +
                          "<li>۷ روز ضمانت بازگشت کالا</li><li>پشتیبانی ۲۴ ساعته</li></ul>"
        };

    // ------------------------------------------------------------- کد تخفیف
    private async Task SeedDiscountCodesAsync(CancellationToken ct)
    {
        if (await _db.DiscountCodes.AnyAsync(ct)) return;

        _db.DiscountCodes.AddRange(
            new DiscountCode
            {
                Code = "WELCOME50", Description = "تخفیف ۵۰ هزار تومانی خرید اول",
                Type = DiscountType.FixedAmount, Amount = 50_000, MinOrderAmount = 300_000,
                IsActive = true, UsageLimit = 1000
            },
            new DiscountCode
            {
                Code = "OFF10", Description = "۱۰٪ تخفیف تا سقف ۲۰۰ هزار تومان",
                Type = DiscountType.Percentage, Amount = 10, MaxDiscountAmount = 200_000,
                MinOrderAmount = 500_000, IsActive = true, UsageLimit = 500
            },
            new DiscountCode
            {
                Code = "TECH15", Description = "۱۵٪ تخفیف کالای دیجیتال تا سقف ۱٫۵ میلیون تومان",
                Type = DiscountType.Percentage, Amount = 15, MaxDiscountAmount = 1_500_000,
                MinOrderAmount = 5_000_000, IsActive = true, UsageLimit = 200
            },
            new DiscountCode
            {
                Code = "SPRING1405", Description = "جشنواره بهاره ۱۴۰۵ - ۲۰٪ تخفیف",
                Type = DiscountType.Percentage, Amount = 20, MaxDiscountAmount = 1_000_000,
                MinOrderAmount = 1_000_000, IsActive = true,
                StartsAtUtc = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc),
                ExpiresAtUtc = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc)
            });
    }

    // ---------------------------------------------------------------- نظرات
    private async Task SeedReviewsAsync(CancellationToken ct)
    {
        if (await _db.Reviews.AnyAsync(ct)) return;

        var products = await _db.Products.Take(10).ToListAsync(ct);
        var customer = await _db.Users.FirstOrDefaultAsync(u => u.Email == "customer@shopingstore.ir", ct);

        var comments = new[]
        {
            ("کیفیت عالی", "کیفیت ساخت این محصول واقعاً بالاست و از خریدم راضی هستم. بسته‌بندی هم بسیار مرتب بود.", 5),
            ("ارسال سریع", "سفارش رو کمتر از ۲۴ ساعت دریافت کردم. محصول دقیقاً مطابق توضیحات بود.", 5),
            ("خوب اما گران", "محصول خوبیه ولی به نظرم قیمتش کمی بالاست. با این حال ارزش خرید داره.", 4),
            ("مطابق انتظار", "همون چیزی بود که انتظار داشتم، عملکردش راضی‌کننده است.", 4),
            ("پیشنهاد می‌کنم", "به دوستام هم پیشنهاد دادم این محصول رو بخرن. کیفیتش عالیه.", 5)
        };

        var index = 0;
        foreach (var product in products)
        {
            var (title, comment, rating) = comments[index % comments.Length];
            _db.Reviews.Add(new Review
            {
                ProductId = product.Id,
                UserId = index % 3 == 0 ? customer?.Id : null,
                AuthorName = index % 3 == 0 ? (customer?.FullName ?? "زهرا محمدی") : index % 2 == 0 ? "علی احمدی" : "نگار سعیدی",
                Title = title,
                Comment = comment,
                Rating = rating,
                Status = index % 4 == 3 ? ReviewStatus.Pending : ReviewStatus.Approved,
                IsVerifiedPurchase = index % 3 == 0,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-index)
            });
            index++;
        }
    }

    // -------------------------------------------------------------- سفارش‌ها
    private async Task SeedOrdersAsync(CancellationToken ct)
    {
        if (await _db.Orders.AnyAsync(ct)) return;

        var customer = await _db.Users.FirstOrDefaultAsync(u => u.Email == "customer@shopingstore.ir", ct);
        if (customer is null) return;

        var products = await _db.Products.OrderBy(p => p.Id).Take(12).ToListAsync(ct);
        var random = new Random(1405);
        var statuses = new[]
        {
            OrderStatus.Delivered, OrderStatus.Shipped, OrderStatus.Paid, OrderStatus.Processing,
            OrderStatus.Delivered, OrderStatus.Pending, OrderStatus.Delivered, OrderStatus.Canceled
        };

        for (var i = 0; i < statuses.Length; i++)
        {
            var status = statuses[i];
            var createdAt = DateTime.UtcNow.AddDays(-i * 2).AddHours(-random.Next(1, 10));

            var order = new Order
            {
                OrderNumber = OrderNumberGenerator.Generate(createdAt),
                UserId = customer.Id,
                CustomerName = customer.FullName,
                CustomerPhone = customer.PhoneNumber ?? "09120000003",
                CustomerEmail = customer.Email,
                Province = "تهران",
                City = "تهران",
                PostalCode = "1234567890",
                AddressLine = "خیابان ولیعصر، کوچه بهار، پلاک ۱۲، واحد ۳",
                PaymentMethod = i % 4 == 2 ? PaymentMethod.CashOnDelivery : PaymentMethod.Online,
                CreatedAtUtc = createdAt
            };

            var itemCount = random.Next(1, 4);
            for (var j = 0; j < itemCount; j++)
            {
                var product = products[random.Next(products.Count)];
                var quantity = random.Next(1, 3);
                order.Items.Add(OrderItem.FromProduct(product, quantity));
            }

            order.Subtotal = order.Items.Sum(x => x.UnitPrice * x.Quantity);
            order.ShippingCost = order.Subtotal >= 2_000_000 ? 0 : 49_000;
            order.TaxAmount = Math.Round(order.Subtotal * 9 / 100m, 0);
            order.Total = order.Subtotal + order.ShippingCost + order.TaxAmount;
            order.Status = status;

            switch (status)
            {
                case OrderStatus.Delivered:
                case OrderStatus.Shipped:
                case OrderStatus.Paid:
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.PaidAtUtc = createdAt.AddMinutes(12);
                    order.PaymentReference = $"SEED-{random.Next(100000, 999999)}";
                    break;
                default:
                    order.PaymentStatus = PaymentStatus.Unpaid;
                    break;
            }

            if (status is OrderStatus.Shipped or OrderStatus.Delivered)
            {
                order.ShippedAtUtc = createdAt.AddDays(1);
                order.TrackingCode = $"IR-{random.Next(10000000, 99999999)}";
                order.History.Add(new OrderStatusHistory { Status = OrderStatus.Shipped, Note = "تحویل به پست", Actor = "انبار", CreatedAtUtc = order.ShippedAtUtc.Value });
            }

            if (status == OrderStatus.Delivered)
            {
                order.DeliveredAtUtc = createdAt.AddDays(3);
                order.History.Add(new OrderStatusHistory { Status = OrderStatus.Delivered, Note = "تحویل به مشتری", Actor = "پست", CreatedAtUtc = order.DeliveredAtUtc.Value });
            }

            order.History.Add(new OrderStatusHistory { Status = OrderStatus.Pending, Note = "سفارش ثبت شد", Actor = order.CustomerName, CreatedAtUtc = createdAt });

            _db.Orders.Add(order);
        }
    }
}
