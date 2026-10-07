#!/usr/bin/env bash
# ابزار تشخیصی ShopingStore برای لینوکس/مک (و WSL)
# اجرا:  bash tools/check-environment.sh
set -u

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ok=0; bad=0
green="\033[0;32m"; red="\033[0;31m"; gray="\033[0;90m"; cyan="\033[0;36m"; reset="\033[0m"

head() { printf "\n${cyan}=== %s ===${reset}\n" "$1"; }
good() { printf "  ${green}[ OK ]${reset}  %s\n" "$1"; ok=$((ok+1)); }
fail() { printf "  ${red}[خطا]${reset}  %s\n" "$1"; bad=$((bad+1)); }
info() { printf "  ${gray}[ .. ]${reset}  %s\n" "$1"; }

printf "بررسی پیش‌نیازهای ShopingStore در: %s\n" "$ROOT"

head "‏.NET SDK"
if command -v dotnet >/dev/null 2>&1; then
  dotnet --list-sdks | while read -r line; do info "$line"; done
  if dotnet --list-sdks | grep -qE '^10\.'; then
    good "SDK دات‌نت ۱۰ نصب است."
  else
    fail "SDK دات‌نت ۱۰ نصب نیست (net10.0 نیازمند آن است): https://dotnet.microsoft.com/download/dotnet/10.0"
  fi
  info "EF CLI: $(dotnet ef --version 2>/dev/null | tail -1 || echo 'نصب نیست → dotnet tool install --global dotnet-ef')"
else
  fail "دستور dotnet پیدا نشد؛ .NET SDK 10 را نصب کنید."
fi

head "فایل‌های پروژه"
for f in ShopingStore.sln ShopingStore.slnx global.json Directory.Build.props \
         src/ShopingStore.Web/ShopingStore.Web.csproj \
         src/ShopingStore.Web/Properties/launchSettings.json; do
  [ -e "$ROOT/$f" ] && good "$f" || fail "$f پیدا نشد"
done

head "دیتابیس (فقط یکی لازم است)"
if command -v docker >/dev/null 2>&1; then
  if docker info >/dev/null 2>&1; then good "Docker در حال اجراست؛ docker compose up -d را بزنید."
  else info "Docker نصب است ولی سرویس اجرا نیست."; fi
else info "Docker نصب نیست."; fi

if command -v sqlcmd >/dev/null 2>&1; then
  info "sqlcmd موجود است؛ با آن می‌توانید اتصال را تست کنید."
else info "sqlcmd نصب نیست (اجباری نیست)."; fi

head "نتیجه"
if [ "$bad" -eq 0 ]; then
  printf "  ${green}همه‌چیز آماده است: dotnet run --project src/ShopingStore.Web${reset}\n"
else
  printf "  ${red}%s مورد نیاز به رفع دارد (موارد قرمز بالا).${reset}\n" "$bad"
fi
printf "\n"
