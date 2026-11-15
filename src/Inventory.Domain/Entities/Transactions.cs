using Inventory.Domain.Common;
using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class StockInHeader : IApprovable, IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string DocNo { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public string? Notes { get; set; }
    public ICollection<StockInDetail> Details { get; set; } = new List<StockInDetail>();
}

public class StockInDetail : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid StockInHeaderId { get; set; }
    public StockInHeader? Header { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
}

public class StockOutHeader : IApprovable, IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string DocNo { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Purpose { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public string? Notes { get; set; }
    public ICollection<StockOutDetail> Details { get; set; } = new List<StockOutDetail>();
}

public class StockOutDetail : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid StockOutHeaderId { get; set; }
    public StockOutHeader? Header { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
}

public class StockTransferHeader : IApprovable, IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string DocNo { get; set; } = string.Empty;
    public Guid FromLocationId { get; set; }
    public Location? FromLocation { get; set; }
    public Guid ToLocationId { get; set; }
    public Location? ToLocation { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public string? Notes { get; set; }
    public ICollection<StockTransferDetail> Details { get; set; } = new List<StockTransferDetail>();
}

public class StockTransferDetail : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid StockTransferHeaderId { get; set; }
    public StockTransferHeader? Header { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
}

public class StockAdjustmentHeader : IApprovable, IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string DocNo { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public string? Notes { get; set; }
    public ICollection<StockAdjustmentDetail> Details { get; set; } = new List<StockAdjustmentDetail>();
}

public class StockAdjustmentDetail : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid StockAdjustmentHeaderId { get; set; }
    public StockAdjustmentHeader? Header { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal Difference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
}

public class StockOpnameHeader : IApprovable, IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string DocNo { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public string? Notes { get; set; }
    public ICollection<StockOpnameDetail> Details { get; set; } = new List<StockOpnameDetail>();
}

public class StockOpnameDetail : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid StockOpnameHeaderId { get; set; }
    public StockOpnameHeader? Header { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal? CountedQuantity { get; set; }
    public decimal Difference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
}
