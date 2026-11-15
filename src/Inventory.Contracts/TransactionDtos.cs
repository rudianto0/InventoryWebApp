using Inventory.Contracts.Enums;

namespace Inventory.Contracts;

public sealed class DocumentLineInput
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal? UnitCost { get; init; }
    public string? Notes { get; init; }
}

public sealed class AdjustmentLineInput
{
    public Guid ProductId { get; init; }
    public int CountedQuantity { get; init; }
    public string? Notes { get; init; }
}

public sealed class OpnameLineInput
{
    public Guid DetailId { get; init; }
    public int? CountedQuantity { get; init; }
    public string? Notes { get; init; }
}

public sealed class StockInInput
{
    public Guid SupplierId { get; init; }
    public Guid LocationId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string? Notes { get; init; }
    public List<DocumentLineInput> Lines { get; init; } = new();
}

public sealed class StockOutInput
{
    public Guid LocationId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string? Purpose { get; init; }
    public string? Notes { get; init; }
    public List<DocumentLineInput> Lines { get; init; } = new();
}

public sealed class StockTransferInput
{
    public Guid FromLocationId { get; init; }
    public Guid ToLocationId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string? Notes { get; init; }
    public List<DocumentLineInput> Lines { get; init; } = new();
}

public sealed class StockAdjustmentInput
{
    public Guid LocationId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string Reason { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<AdjustmentLineInput> Lines { get; init; } = new();
}

public sealed class StockOpnameInput
{
    public Guid LocationId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string? Notes { get; init; }
}



public sealed class TransactionFilter
{
    public ApprovalStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class TransactionListItem
{
    public Guid Id { get; init; }
    public string DocNo { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string? PartnerName { get; init; }
    public string? SecondaryLocationName { get; init; }
    public ApprovalStatus Status { get; init; }
    public string? CreatedByName { get; init; }
    public string? ApproverName { get; init; }
    public DateTime CreatedAt { get; init; }
    public int LineCount { get; init; }
}

public sealed class DocumentLineView
{
    public Guid DetailId { get; init; }
    public Guid ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal? UnitCost { get; init; }
    public int? SystemQuantity { get; init; }
    public int? CountedQuantity { get; init; }
    public int? Difference { get; init; }
    public string? Notes { get; init; }
}

public sealed class TransactionDetailView
{
    public Guid Id { get; init; }
    public string DocNo { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public ApprovalStatus Status { get; init; }
    public Guid? SupplierId { get; init; }
    public Guid LocationId { get; init; }
    public Guid? FromLocationId { get; init; }
    public Guid? ToLocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string? SecondaryLocationName { get; init; }
    public string? PartnerName { get; init; }
    public string? Purpose { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public string? CreatedByName { get; init; }
    public string? ApproverName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? RejectionReason { get; init; }
    public IReadOnlyList<DocumentLineView> Lines { get; init; } = Array.Empty<DocumentLineView>();
}

public sealed class SelectOption
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}

public sealed class StockAvailability
{
    public Guid ProductId { get; init; }
    public Guid LocationId { get; init; }
    public int Quantity { get; init; }
}

public sealed class ApprovalItem
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string DocNo { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string? RequesterName { get; init; }
    public DateTime? SubmittedAt { get; init; }
}
