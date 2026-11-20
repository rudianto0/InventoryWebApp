using Inventory.Contracts;

namespace Inventory.Application.Common;

public static class Sorting
{
    public static string? NormalizeKey(string? sortBy) =>
        string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim().ToLowerInvariant();

    public static bool IsDesc(SortDirection direction) => direction == SortDirection.Desc;

    public static int NormalizePage(int page) => Math.Max(page, 1);

    public static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, 200);
}
