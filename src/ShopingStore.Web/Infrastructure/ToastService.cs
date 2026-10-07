using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ShopingStore.Web.Infrastructure;

/// <summary>پیام‌های موقت (Toast) که پس از Redirect در صفحه بعد نمایش داده می‌شوند.</summary>
public class ToastService
{
    private const string SuccessKey = "Toast.Success";
    private const string ErrorKey = "Toast.Error";
    private const string InfoKey = "Toast.Info";

    private readonly ITempDataDictionaryFactory _factory;
    private readonly IHttpContextAccessor _accessor;

    public ToastService(ITempDataDictionaryFactory factory, IHttpContextAccessor accessor)
    {
        _factory = factory;
        _accessor = accessor;
    }

    private ITempDataDictionary TempData => _factory.GetTempData(_accessor.HttpContext!);

    public void Success(string message) => TempData[SuccessKey] = message;
    public void Error(string message) => TempData[ErrorKey] = message;
    public void Info(string message) => TempData[InfoKey] = message;

    public static string SuccessKeyName => SuccessKey;
    public static string ErrorKeyName => ErrorKey;
    public static string InfoKeyName => InfoKey;
}
