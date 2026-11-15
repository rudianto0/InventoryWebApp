namespace Inventory.Contracts;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public sealed class PageQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int Skip => (Math.Max(Page, 1) - 1) * Math.Max(PageSize, 1);
    public int Take => Math.Clamp(PageSize, 1, 200);
    public int NormalizedPage => Math.Max(Page, 1);
}
