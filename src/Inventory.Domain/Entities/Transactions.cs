using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class StockInHeader : IApprovable
{
    public int Id { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public ICollection<StockInDetail> Details { get; set; } = new List<StockInDetail>();
}

public class StockInDetail
{
    public int Id { get; set; }
    public int StockInHeaderId { get; set; }
    public StockInHeader? Header { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}

public class StockOutHeader : IApprovable
{
    public int Id { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Purpose { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public ICollection<StockOutDetail> Details { get; set; } = new List<StockOutDetail>();
}

public class StockOutDetail
{
    public int Id { get; set; }
    public int StockOutHeaderId { get; set; }
    public StockOutHeader? Header { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class StockTransferHeader : IApprovable
{
    public int Id { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public int FromLocationId { get; set; }
    public Location? FromLocation { get; set; }
    public int ToLocationId { get; set; }
    public Location? ToLocation { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public ICollection<StockTransferDetail> Details { get; set; } = new List<StockTransferDetail>();
}

public class StockTransferDetail
{
    public int Id { get; set; }
    public int StockTransferHeaderId { get; set; }
    public StockTransferHeader? Header { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class StockAdjustmentHeader : IApprovable
{
    public int Id { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public ICollection<StockAdjustmentDetail> Details { get; set; } = new List<StockAdjustmentDetail>();
}

public class StockAdjustmentDetail
{
    public int Id { get; set; }
    public int StockAdjustmentHeaderId { get; set; }
    public StockAdjustmentHeader? Header { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal Difference { get; set; }
    public string? Notes { get; set; }
}

public class StockOpnameHeader : IApprovable
{
    public int Id { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public ICollection<StockOpnameDetail> Details { get; set; } = new List<StockOpnameDetail>();
}

public class StockOpnameDetail
{
    public int Id { get; set; }
    public int StockOpnameHeaderId { get; set; }
    public StockOpnameHeader? Header { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal? CountedQuantity { get; set; }
    public decimal Difference { get; set; }
    public string? Notes { get; set; }
}
