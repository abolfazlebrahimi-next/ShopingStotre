#requires -version 5.1
<#
    ابزار تشخیصی ShopingStore برای ویندوز
    ---------------------------------------------------------
    این اسکریپت وضعیت پیش‌نیازهای اجرای پروژه را بررسی می‌کند:
      • نسخه‌های .NET SDK نصب‌شده (نیاز: 10.0.x)
      • نسخه ویژوال استودیو و بارهای کاری (نیاز: VS 2026 نسخه 18.x)
      • درستی فایل‌های سولوشن و پروژه‌ها
      • وضعیت SQL Server / Docker برای دیتابیس
    اجرا:  powershell -ExecutionPolicy Bypass -File tools\check-environment.ps1
#>

$ErrorActionPreference = 'SilentlyContinue'
$ok = 0; $bad = 0

function Head($t) { Write-Host "`n=== $t ===" -ForegroundColor Cyan }
function Good($t) { Write-Host "  [ OK ]  $t" -ForegroundColor Green; $script:ok++ }
function Fail($t) { Write-Host "  [خطا]  $t" -ForegroundColor Red; $script:bad++ }
function Info($t) { Write-Host "  [ .. ]  $t" -ForegroundColor Gray }

$root = Split-Path -Parent $PSScriptRoot
Write-Host "بررسی پیش‌نیازهای ShopingStore در مسیر: $root" -ForegroundColor White

# ---------------------------------------------------------------- ۱) .NET SDK
Head "نسخه‌های .NET SDK نصب‌شده"
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Fail "دستور dotnet پیدا نشد؛ .NET SDK 10 را از https://dotnet.microsoft.com/download/dotnet/10.0 نصب کنید."
} else {
    $sdks = @(& dotnet --list-sdks 2>$null)
    if ($sdks.Count -eq 0) { Fail "هیچ .NET SDK نصب نیست." }
    foreach ($s in $sdks) { Info $s }

    $ten = @($sdks | Where-Object { $_ -match '^\s*10\.' })
    if ($ten.Count -gt 0) { Good "SDK دات‌نت ۱۰ نصب است ($($ten[0].Split(' ')[0]))." }
    else { Fail "SDK دات‌نت ۱۰ نصب نیست؛ پروژه net10.0 است و بدون آن باز/کامپایل نمی‌شود." }
}

# -------------------------------------------------------- ۲) ویژوال استودیو
Head "ویژوال استودیو"
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) {
    Fail "vswhere پیدا نشد؛ به‌نظر ویژوال استودیو نصب نیست."
} else {
    $vsList = @(& $vswhere -all -prerelease -products * -format json | ConvertFrom-Json)
    if ($vsList.Count -eq 0) { Fail "هیچ نسخه‌ای از ویژوال استودیو یافت نشد." }
    $has2026 = $false
    foreach ($vs in $vsList) {
        $major = [int]($vs.installationVersion.Split('.')[0])
        if ($major -ge 18) { $has2026 = $true }
        Info "$($vs.displayName) — نسخه $($vs.installationVersion)"

        # بارهای کاری نصب‌شده
        $w = & $vswhere -products * -requires Microsoft.VisualStudio.Workload.NetWeb -property installationPath 2>$null
        if ($w) { Good "بار کاری «ASP.NET and web development» روی $($vs.displayName) نصب است." }
        else { Fail "بار کاری «ASP.NET and web development» نصب نیست (Visual Studio Installer → Workloads)." }
    }
    if ($has2026) { Good "ویژوال استودیو ۲۰۲۶ (نسخه ۱۸+) نصب است." }
    else { Fail "ویژوال استودیو ۲۰۲۶ (۱۸.x) نصب نیست؛ نسخه‌های ۱۷.x نمی‌توانند net10.0 را باز کنند (خطای NETSDK1045)." }
}

# ------------------------------------------------------ ۳) سولوشن و پروژه‌ها
Head "سولوشن و پروژه‌ها"
foreach ($f in @('ShopingStore.sln', 'ShopingStore.slnx', 'global.json', 'Directory.Build.props')) {
    if (Test-Path (Join-Path $root $f)) { Good "$f موجود است." } else { Fail "$f پیدا نشد." }
}
foreach ($p in @(
        'src\ShopingStore.Domain\ShopingStore.Domain.csproj',
        'src\ShopingStore.Infrastructure\ShopingStore.Infrastructure.csproj',
        'src\ShopingStore.Web\ShopingStore.Web.csproj',
        'src\ShopingStore.Web\Properties\launchSettings.json')) {
    if (Test-Path (Join-Path $root $p)) { Good "$p" } else { Fail "$p پیدا نشد." }
}

# ---------------------------------------------------------------- ۴) دیتابیس
Head "دیتابیس (فقط یکی لازم است)"
if (Get-Command docker -ErrorAction SilentlyContinue) {
    $dv = & docker version --format '{{.Server.Version}}' 2>$null
    if ($dv) { Good "Docker در حال اجراست (نسخه سرور $dv) — می‌توانید docker compose up -d بزنید." }
    else { Info "Docker نصب است ولی سرویس آن اجرا نیست (Docker Desktop را باز کنید)." }
} else { Info "Docker نصب نیست." }

if (Get-Command sqllocaldb -ErrorAction SilentlyContinue) {
    $inst = @(& sqllocaldb info 2>$null)
    $mssqllocaldb = @($inst | Where-Object { $_ -match 'MSSQLLocalDB' })
    if ($mssqllocaldb.Count -gt 0) { Good "SQL Server LocalDB موجود است؛ در appsettings.Local.json از رشته اتصال LocalDB استفاده کنید." }
    else { Info "LocalDB نصب است ولی نمونه MSSQLLocalDB ساخته نشده (sqllocaldb create MSSQLLocalDB)." }
} else { Info "SQL Server LocalDB نصب نیست." }

# ------------------------------------------------------------------ نتیجه
Head "نتیجه"
if ($bad -eq 0) {
    Write-Host "همه‌چیز آماده است. مراحل اجرا:" -ForegroundColor Green
    Write-Host "  1) ShopingStore.sln را باز کنید" -ForegroundColor Green
    Write-Host "  2) روی ShopingStore.Web راست‌کلیک → Set as Startup Project" -ForegroundColor Green
    Write-Host "  3) پروفایل https یا http را انتخاب و F5 بزنید" -ForegroundColor Green
} else {
    Write-Host "$bad مورد نیاز به رفع دارد (موارد قرمز بالا)." -ForegroundColor Yellow
    Write-Host "اگر پروژه در ویژوال استودیو با برچسب «(بارگذاری نشده)» یا خطای NETSDK1045 باز می‌شود،" -ForegroundColor Yellow
    Write-Host "علت همان نبودن SDK دات‌نت ۱۰ یا نسخه قدیمی‌تر ویژوال استودیو است." -ForegroundColor Yellow
}
Write-Host ""
