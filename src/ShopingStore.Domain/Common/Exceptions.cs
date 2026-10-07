namespace ShopingStore.Domain.Common;

/// <summary>خطای منطق کسب‌وکار که پیام آن مستقیماً به کاربر نمایش داده می‌شود.</summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

/// <summary>خطای «یافت نشد».</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message = "موردی که درخواست کرده‌اید یافت نشد.") : base(message) { }
}

/// <summary>خطای عدم دسترسی.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "شما به این بخش دسترسی ندارید.") : base(message) { }
}
