namespace ShopingStore.Domain.Common;

/// <summary>نتیجه صفحه‌بندی‌شده.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    public int FirstItemOnPage => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
    public int LastItemOnPage => Math.Min(Page * PageSize, TotalCount);

    public static PagedResult<T> Empty(int pageSize = 12) => new() { Items = Array.Empty<T>(), TotalCount = 0, Page = 1, PageSize = pageSize };

    public PagedResult<TDestination> Map<TDestination>(Func<T, TDestination> mapper) => new()
    {
        Items = Items.Select(mapper).ToList(),
        TotalCount = TotalCount,
        Page = Page,
        PageSize = PageSize
    };
}
