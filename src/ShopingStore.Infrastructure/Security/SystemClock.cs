using ShopingStore.Domain.Interfaces;

namespace ShopingStore.Infrastructure.Security;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
