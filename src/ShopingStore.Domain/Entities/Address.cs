using ShopingStore.Domain.Common;

namespace ShopingStore.Domain.Entities;

public class Address : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Title { get; set; } = "آدرس اصلی";
    public string ReceiverName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Line { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public string FullText => $"{Province} - {City} - {Line} (کدپستی {PostalCode})";
}
