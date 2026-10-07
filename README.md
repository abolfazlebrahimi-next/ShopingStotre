# 🛍️ ShopingStore — فروشگاه اینترنتی کامل با ASP.NET Core 10

یک فروشگاه اینترنتی **کامل، فارسی و راست‌به‌چپ** که کاملاً با **.NET 10** نوشته شده است؛
هم بک‌اند (منطق فروشگاه، پنل مدیریت، پرداخت، مدیریت داده) و هم **فرانت‌اند داخل خود دات‌نت**
با **ASP.NET Core Razor Pages** پیاده‌سازی شده است (بدون نیاز به React/Vue یا هر پروژه‌ی جدا).

> طراحی این پروژه لایه‌بندی‌شده است: `Domain` (مدل و قواعد کسب‌وکار) ← `Infrastructure` (Entity Framework Core، سرویس‌ها، درگاه پرداخت) ← `Web` (رابط کاربری فروشگاه و پنل مدیریت).

---

## ✨ امکانات

### بخش فروشگاه (مشتری)
| امکان | توضیح |
|---|---|
| صفحه اصلی | هیرو، دسته‌بندی‌ها، پیشنهادهای شگفت‌انگیز، منتخب، جدیدترین‌ها، پرفروش‌ترین‌ها، نظرات مشتریان |
| فروشگاه و فیلتر | جست‌وجو، فیلتر دسته‌بندی/برند/قیمت، فقط موجود، فقط تخفیف‌دار، مرتب‌سازی (۶ حالت)، صفحه‌بندی |
| صفحه محصول | گالری تصاویر، انتخاب تنوع (رنگ/سایز)، مشخصات فنی، توضیحات HTML، نظرات و ثبت نظر، محصولات مشابه |
| سبد خرید | افزودن با AJAX، تغییر تعداد، حذف، اعمال/حذف کد تخفیف، محاسبه ارسال/مالیات، هشدار ارسال رایگان |
| فرآیند خرید | انتخاب آدرس ذخیره‌شده یا آدرس جدید، صفحه‌بندی مراحل، دو شیوه پرداخت (اینترنتی / در محل) |
| درگاه پرداخت | درگاه **آزمایشی (شبیه‌ساز)** با توکن `Authority`، بازگشت Callback، تأیید تراکنش و شناسه پرداخت (قابل تعویض با درگاه واقعی) |
| حساب کاربری | ثبت‌نام، ورود، خروج، داشبورد، ویرایش پروفایل، تغییر رمز، سفارش‌ها، جزئیات سفارش، لغو سفارش، آدرس‌ها، علاقه‌مندی‌ها |
| سفارش‌ها | پیگیری وضعیت، تاریخچه تغییر وضعیت، کد رهگیری مرسوله |
| صفحات عمومی | درباره ما، تماس با ما (فرم پیام)، سؤالات متداول، صفحه خطا/عدم دسترسی |

### بخش پنل مدیریت
- **داشبورد**: فروش کل/امروز/ماه، تعداد سفارش‌ها، محصولات ناموجود، مشتریان، نظرات در انتظار تأیید،
  نمودار فروش ۱۴ روز گذشته، پرفروش‌ترین محصولات، فروش بر اساس دسته‌بندی، آخرین سفارش‌ها، کالاهای کم‌موجود.
- **محصولات**: فهرست با فیلتر و جست‌وجو، افزودن/ویرایش با ویژگی‌های فنی داینامیک، تنوع‌ها،
  **بارگذاری تصویر**، قیمت/قیمت با تخفیف/موجودی، فعال‌سازی و حذف نرم.
- **دسته‌بندی‌ها**: ساختار درختی (والد/فرزند)، نامک خودکار (Slug)، آیکون، ترتیب نمایش.
- **سفارش‌ها**: فیلتر بر اساس وضعیت/پرداخت/جست‌وجو، تغییر وضعیت با تاریخچه، ثبت کد رهگیری، مشاهده اقلام و مبالغ.
- **کاربران**: جست‌وجو و فیلتر، تعداد سفارش و مجموع خرید هر کاربر، فعال/غیرفعال کردن، تغییر نقش (مشتری/پشتیبان/مدیر).
- **کدهای تخفیف**: درصدی یا مبلغ ثابت، سقف تخفیف، حداقل سفارش، بازه زمانی، محدودیت تعداد استفاده.
- **نظرات**: تأیید/رد/حذف نظر با به‌روزرسانی خودکار امتیاز محصول.
- **تنظیمات فروشگاه**: نام، شعار، اطلاعات تماس، هزینه ارسال، سقف ارسال رایگان، درصد مالیات، کارمزد پرداخت در محل، اطلاعات کارت.

---

## 🧱 تکنولوژی‌ها

| مورد | نسخه |
|---|---|
| .NET / ASP.NET Core | **10.0** (Razor Pages) |
| Entity Framework Core | 10.0 (**SQL Server**) |
| دیتابیس | Microsoft SQL Server (LocalDB / Express / Docker) |
| احراز هویت | کوکی (Cookie Authentication) با نقش‌های `Customer` / `Support` / `Admin` |
| هش رمز عبور | PBKDF2-SHA256 با ۱۰۰٬۰۰۰ تکرار و نمک تصادفی |
| تاریخ | تاریخ شمسی با `PersianCalendar` داخلی دات‌نت + ارقام فارسی |
| فونت | Vazirmatn (وزیرمتن) — به‌صورت محلی در `wwwroot/fonts` |
| JS/CSS | جاوااسکریپت و CSS خالص (بدون بسته‌های npm) |

---

## 📁 ساختار پروژه

```
ShopingStore.sln
├── src/
│   ├── ShopingStore.Domain/            # موجودیت‌ها، DTOها، قراردادها (Interfaces)، قواعد کسب‌وکار
│   │   ├── Common/                     # BaseEntity، Enums، PersianDate، SlugHelper، Guard، PagedResult
│   │   ├── Entities/                   # User, Product, Category, Cart, Order, Review, DiscountCode, ...
│   │   ├── Dtos/                       # DTOهای ورودی/خروجی سرویس‌ها
│   │   └── Interfaces/                 # IProductRepository, IOrderService, IPaymentGateway, ...
│   │
│   ├── ShopingStore.Infrastructure/    # پیاده‌سازی داده و سرویس‌ها
│   │   ├── Data/                       # ApplicationDbContext، Configurations، DbInitializer (داده اولیه)
│   │   ├── Repositories/               # پیاده‌سازی مخازن با EF Core + UnitOfWork
│   │   ├── Services/                   # CatalogService, CartService, OrderService, AuthService, Dashboard...
│   │   ├── Mapping/                    # تبدیل موجودیت ↔ DTO + محاسبه مبالغ سبد (CartPricing)
│   │   ├── Payment/                    # DemoPaymentGateway (شبیه‌ساز درگاه بانکی)
│   │   ├── Security/                   # PasswordHasher, SystemClock
│   │   └── Storage/                    # LocalFileStorage (آپلود تصاویر)
│   │
│   └── ShopingStore.Web/               # رابط کاربری (Razor Pages) و APIهای سبک
│       ├── Pages/                      # Index, Products, Cart, Checkout, Payment, Account, Admin, ...
│       ├── Endpoints/                  # /api/products/suggest، /api/cart/count، /api/wishlist/toggle، sitemap
│       ├── Infrastructure/             # CartIdentity (کوکی سبد)، HttpCurrentUser، ToastService، PaginationModel
│       ├── Properties/                 # launchSettings.json (پروفایل‌های اجرا در ویژوال استودیو)
│       ├── appsettings.json            # رشته اتصال و تنظیمات فروشگاه
│       ├── appsettings.Local.json.example  # نمونه بازنویسی تنظیمات محلی (رشته اتصال ویندوز)
│       └── wwwroot/                    # css، js، فونت وزیرمتن، تصاویر SVG
│
├── preview/                            # پیش‌نمایش استاتیک طراحی (HTML/CSS) برای مشاهده سریع ظاهر سایت
├── docker-compose.yml                  # راه‌اندازی سریع SQL Server
├── .editorconfig                       # یکسان‌سازی سبک کد و کدگذاری UTF-8 (BOM)
└── README.md
```

---

## 🚀 راه‌اندازی

### پیش‌نیازها
1. [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
2. SQL Server (یکی از موارد زیر):
   - **Docker** (پیشنهادی و ساده‌ترین راه):
     ```bash
     docker compose up -d
     ```
     این دستور SQL Server 2022 را روی پورت `1433` با کاربر `sa` و رمز `Shop@Store12345` بالا می‌آورد.
   - یا **SQL Server Express / LocalDB** روی ویندوز.

### ۱) تنظیم رشته اتصال
در فایل `src/ShopingStore.Web/appsettings.json` مقدار `ConnectionStrings:Default` را تنظیم کنید:

```jsonc
// Docker / لینوکس
"Default": "Server=localhost,1433;Database=ShopingStoreDb;User Id=sa;Password=Shop@Store12345;TrustServerCertificate=True;MultipleActiveResultSets=true"

// ویندوز - SQL Server Express
"Default": "Server=.\\SQLEXPRESS;Database=ShopingStoreDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

// ویندوز - LocalDB
"Default": "Server=(localdb)\\MSSQLLocalDB;Database=ShopingStoreDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

> می‌توانید تنظیمات محلی خود را در فایل `appsettings.Local.json` قرار دهید
> (نمونه: `appsettings.Local.json.example`). این فایل بلافاصله **بعد از** `appsettings.json`
> بارگذاری می‌شود و مقدارهای آن اولویت دارند؛ در `.gitignore` هم قرار دارد و به مخزن ارسال نمی‌شود.

### ۲) اجرا
```bash
dotnet restore
dotnet run --project src/ShopingStore.Web
```

برنامه در آدرس‌های زیر بالا می‌آید (تعریف‌شده در `src/ShopingStore.Web/Properties/launchSettings.json`):

```
https://localhost:7100   یا   http://localhost:5100
```

در اولین اجرا:
- اگر مایگریشنی وجود داشته باشد، به‌طور خودکار `Migrate` می‌شود.
- اگر مایگریشنی نساخته باشید، دیتابیس **از روی مدل** ساخته می‌شود (`EnsureCreated`).
- داده‌های نمونه (۸ دسته‌بندی + زیردسته‌ها، ۴۰ محصول ایرانی، کاربران، کدهای تخفیف، تنظیمات، نظرات و ۸ سفارش نمونه) درج می‌شود.

### ۳) (اختیاری) ساخت مایگریشن EF Core
برای محیط واقعی و کنترل نسخه‌ی اسکیمای دیتابیس:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/ShopingStore.Infrastructure --startup-project src/ShopingStore.Web
dotnet ef database update --project src/ShopingStore.Infrastructure --startup-project src/ShopingStore.Web
```

---

## 🧑‍💻 اجرا در ویژوال استودیو

پروژه یک راه‌اندازی آماده برای ویژوال استودیو دارد (`src/ShopingStore.Web/Properties/launchSettings.json`).

### پیش‌نیاز مهم
| مورد | نسخه لازم |
|---|---|
| ویژوال استودیو | **Visual Studio 2026 (نسخه ۱۸.x)** — هدف‌گذاری `net10.0` فقط از VS 2026 پشتیبانی می‌شود |
| .NET SDK | **10.0.x** (نصب آن همراه VS 2026 یا از [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0)) |
| دیتابیس | Docker Desktop (`docker compose up -d`) یا SQL Server Express/LocalDB |

> اگر ویژوال استودیو ۲۰۲۲ دارید، خطای `NETSDK1045` می‌گیرید
> («The current .NET SDK does not support targeting .NET 10.0»)؛ چون VS 2022 از .NET 10
> پشتیبانی نمی‌کند. در این حالت یا VS 2026 را نصب کنید یا برنامه را از خط فرمان با
> `dotnet run --project src/ShopingStore.Web` اجرا کنید (همان SDK کار می‌کند).

### گام‌ها
1. فایل **`ShopingStore.sln`** را باز کنید (نه پوشه پروژه).
2. روی پروژه **`ShopingStore.Web`** راست‌کلیک کنید و **Set as Startup Project** را بزنید.
   (در صورت نیاز: راست‌کلیک روی Solution → **Restore NuGet Packages**.)
3. در نوار ابزار، یکی از پروفایل‌های `https` یا `http` را انتخاب کنید و **F5** بزنید.

برنامه روی این آدرس‌ها بالا می‌آید (قابل تغییر در `launchSettings.json`):

```
https://localhost:7100      پروفایل https
http://localhost:5100       پروفایل http
```

4. در پنجره **Package Manager Console** (منوی Tools → NuGet Package Manager) می‌توانید
   مایگریشن‌ها را هم بسازید — کافی است «Default project» را روی `ShopingStore.Infrastructure` بگذارید:

```powershell
Add-Migration InitialCreate -Project ShopingStore.Infrastructure -StartupProject ShopingStore.Web
Update-Database -Project ShopingStore.Infrastructure -StartupProject ShopingStore.Web
```

> اگر مایگریشنی نسازید هم مشکلی نیست؛ در اولین اجرا دیتابیس از روی مدل ساخته و
> داده‌های نمونه درج می‌شود. فقط باید دیتابیس در دسترس باشد.

### ترفندهای ویژه ویژوال استودیو
- **بدون دست‌زدن به فایل‌های اصلی:** فایل `appsettings.Local.json.example` را به
  `appsettings.Local.json` تغییر نام دهید و رشته اتصال محلی خود را در آن بگذارید
  (سه نمونه آماده: LocalDB، SQL Express و Docker). این فایل خودکار خوانده می‌شود.
- **User Secrets:** پروژه `UserSecretsId` دارد؛ راست‌کلیک روی پروژه → **Manage User Secrets**
  و می‌توانید رمزها را آنجا نگه دارید.
- **گواهی HTTPS:** اگر مرورگر هشدار داد، یک‌بار `dotnet dev-certs https --trust` را اجرا کنید
  یا از پروفایل `http` استفاده کنید.
- **خطای «cannot connect to database»:** برنامه به کار ادامه می‌دهد ولی صفحه‌ها داده‌ای
  ندارند؛ ابتدا SQL Server را بالا بیاورید (`docker compose up -d`) یا رشته اتصال را به
  LocalDB/SQL Express تغییر دهید.
- **خطای طول مسیر ویندوز:** اگر مسیر `Path too long` گرفتید، مخزن را در مسیر کوتاه‌تری
  مثل `C:\Projects\ShopingStore` کلون کنید.

---

## 🔑 حساب‌های نمونه

| نقش | ایمیل | رمز عبور |
|---|---|---|
| مدیر سیستم | `admin@shopingstore.ir` | `Admin@12345` |
| پشتیبان | `support@shopingstore.ir` | `Support@12345` |
| مشتری | `customer@shopingstore.ir` | `Customer@12345` |

**کدهای تخفیف نمونه:** `WELCOME50` (۵۰ هزار تومان)، `OFF10` (۱۰٪ تا سقف ۲۰۰ هزار)، `TECH15` (۱۵٪ تا سقف ۱٫۵ میلیون)، `SPRING1405` (۲۰٪ جشنواره بهاره).

---

## 🖼️ پیش‌نمایش طراحی (بدون اجرای دات‌نت)

پوشه `preview/` شامل چند صفحه HTML استاتیک با همان CSS و فونت سایت است تا ظاهر پروژه را
بدون اجرای دات‌نت ببینید:

```
preview/index.html     → صفحه اصلی، فهرست محصولات و کارت محصولات
preview/product.html   → صفحه جزئیات محصول (گالری، تنوع، مشخصات، نظرات)
preview/cart.html      → سبد خرید و کد تخفیف
preview/checkout.html  → فرآیند پرداخت و درگاه آزمایشی
preview/admin.html     → داشبورد پنل مدیریت
```

برای مشاهده، پروژه را با هر وب‌سرور ساده‌ای سرو کنید (مثلاً `python3 -m http.server`)
و آدرس `preview/index.html` را باز کنید.

---

## 🔌 اتصال درگاه پرداخت واقعی

در حال حاضر `DemoPaymentGateway` (در `ShopingStore.Infrastructure/Payment`) نقش بانک را شبیه‌سازی می‌کند:

1. `RequestPaymentAsync` یک توکن (`Authority`) می‌سازد و کاربر را به صفحه درگاه برمی‌گرداند.
2. در صفحه درگاه، کاربر «پرداخت موفق» یا «انصراف» را انتخاب می‌کند.
3. بازگشت به `/payment/callback` و سپس `VerifyPaymentAsync` تراکنش را تأیید و سفارش را «پرداخت‌شده» می‌کند.

برای استفاده از درگاه واقعی (زرین‌پال، ملت، سامان، آی‌دی‌پی و ...) کافی است:

```csharp
public class ZarinPalGateway : IPaymentGateway { /* ... */ }

// در DependencyInjection.cs
services.AddSingleton<IPaymentGateway, ZarinPalGateway>();
```

## 📨 ارسال پیامک/ایمیل

`NotificationService` پیام‌ها را در لاگ ثبت می‌کند. برای ارسال واقعی، همین کلاس را با سرویس‌دهنده
دلخواه (کاوه‌نگار، ملی‌پیامک، SMTP و ...) پیاده‌سازی کنید (`INotificationService`).

---

## 🔐 نکات امنیتی پیاده‌سازی‌شده
- هش رمز عبور با **PBKDF2-SHA256** (۱۰۰٬۰۰۰ تکرار، نمک ۱۶ بایتی، مقایسه زمان‌ثابت).
- **Anti-Forgery Token** روی همه فرم‌ها و درخواست‌های AJAX (`RequestVerificationToken`).
- محافظت از صفحات با سیاست‌های نقش‌محور: `AdminOnly`، `StaffOnly`، `CustomerOnly`.
- **حذف نرم** (Soft Delete) برای محصولات، سفارش‌ها، کاربران، نظرات و ... با کوئری‌فیلترهای EF Core.
- اعتبارسنجی ورودی‌ها در لایه دامنه (`Guard`) + پیام‌های خطای فارسی یکپارچه (`BusinessException`).
- محدودسازی نوع/حجم فایل آپلودی تصاویر (حداکثر ۳ مگابایت و پسوندهای مجاز).

---

## 🧪 سؤالات متداول و عیب‌یابی

**دیتابیس ساخته نمی‌شود / خطای اتصال**
رشته اتصال را بررسی کنید و مطمئن شوید SQL Server در حال اجراست (`docker compose up -d`).
پیام خطای دقیق در کنسول لاگ می‌شود.

**صفحه `/admin` باز نمی‌شود**
باید با حساب مدیر (`admin@shopingstore.ir`) وارد شوید؛ برای نقش پشتیبان بخش تنظیمات مخفی است.

**تاریخ‌ها میلادی نمایش داده می‌شوند**
همه تاریخ‌ها در رابط کاربری با کمکی `PersianDate` (تاریخ شمسی + ارقام فارسی) نمایش داده می‌شوند.
اگر `InvariantGlobalization=true` را فعال کنید، `PersianCalendar` کار نمی‌کند؛ این مقدار در `Directory.Build.props`
عمداً `false` گذاشته شده است.

**تصاویر آپلودشده نمایش داده نمی‌شوند**
تصاویر در `wwwroot/uploads/products` ذخیره می‌شوند؛ دسترسی نوشتن این پوشه را بررسی کنید.

**در ویژوال استودیو خطای `NETSDK1045` می‌گیرم**
یعنی نسخه ویژوال استودیو از .NET 10 پشتیبانی نمی‌کند. هدف‌گذاری `net10.0` فقط در
**Visual Studio 2026 (۱۸.x)** پشتیبانی می‌شود؛ ارتقا دهید یا از خط فرمان اجرا کنید:
`dotnet run --project src/ShopingStore.Web`.

**پروژه در ویژوال استودیو باز نمی‌شود / صفحه‌ها نمی‌آید**
باید `ShopingStore.sln` (و نه پوشه پروژه) باز شود و پروژه `ShopingStore.Web` به‌عنوان
Startup Project انتخاب شود. همچنین مطمئن شوید NuGet Restore کامل انجام شده است (بسته‌های
`Microsoft.EntityFrameworkCore.*` نسخه 10.0.0 باید دانلود شوند).

**برنامه اجرا می‌شود ولی همه صفحه‌ها خطا می‌دهند**
یعنی دیتابیس در دسترس نیست. خروجی کنسول پیام «امکان اتصال به دیتابیس وجود ندارد» را نشان می‌دهد.
`docker compose up -d` را اجرا کنید یا در `appsettings.Local.json` رشته اتصال LocalDB/SQL Express بگذارید.

**مرورگر هشدار امنیتی گواهی می‌دهد**
یک‌بار `dotnet dev-certs https --trust` را اجرا کنید یا پروفایل `http` را از نوار ابزار انتخاب کنید.

---

## 🗺️ گام‌های بعدی پیشنهادی
- اتصال درگاه پرداخت واقعی و ارسال پیامک
- ورود با گوگل (ساختار `ExternalProvider`/`ExternalProviderKey` در موجودیت `User` آماده است)
- افزودن لیست مقایسه کالا و نظرسنجی
- نوشتن تست‌های واحد (xUnit) برای سرویس‌های `Domain` و `Infrastructure`
- کش کردن صفحات پربازدید (IMemoryCache/Redis) و بهینه‌سازی کوئری‌ها

---

ساخته‌شده با ❤️ روی .NET 10 — همه حقوق محفوظ است.
