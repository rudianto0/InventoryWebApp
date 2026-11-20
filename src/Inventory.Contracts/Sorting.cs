namespace Inventory.Contracts;

public enum SortDirection
{
    Asc = 0,
    Desc = 1
}

public sealed class SortSpec
{
    public string? SortBy { get; init; }
    public SortDirection SortDir { get; init; } = SortDirection.Asc;
}
