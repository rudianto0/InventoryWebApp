using Inventory.Domain.Enums;

namespace Inventory.Application.Dtos;

public sealed class ReportFilter
{
    public int? LocationId { get; set; }
    public string? BranchCode { get; set; }
    public int? ProductId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class AuditFilter
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public int? UserId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class DashboardSummary
{
    public int TotalProducts { get; init; }
    public decimal TotalStockQuantity { get; init; }
    public int BelowReorderCount { get; init; }
    public int PendingApprovals { get; init; }
    public int TotalLocations { get; init; }
    public IReadOnlyList<ReorderItem> ReorderItems { get; init; } = Array.Empty<ReorderItem>();
    public IReadOnlyList<StockMovementItem> RecentMovements { get; init; } = Array.Empty<StockMovementItem>();
}

public sealed class StockBalanceItem
{
    public int ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string? BranchCode { get; init; }
    public decimal Quantity { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class StockCardItem
{
    public DateTime CreatedAt { get; init; }
    public string MovementNo { get; init; } = string.Empty;
    public MovementType Type { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal RunningBalance { get; init; }
    public string? Notes { get; init; }
}

public sealed class StockMovementItem
{
    public DateTime CreatedAt { get; init; }
    public string MovementNo { get; init; } = string.Empty;
    public MovementType Type { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string? Notes { get; init; }
}

public sealed class ReorderItem
{
    public int ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public int ReorderLevel { get; init; }
}



public sealed class AuditListItem
{
    public int Id { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public string Action { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public string? UserName { get; init; }
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class AuditDetailItem
{
    public int Id { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? UserName { get; init; }
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}
