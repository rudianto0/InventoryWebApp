namespace Inventory.Domain.Enums;

public enum LocationType
{
    Branch = 0,
    Warehouse = 1,
    Store = 2
}

public enum ApprovalStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum MovementType
{
    In = 0,
    Out = 1,
    TransferOut = 2,
    TransferIn = 3,
    Adjustment = 4,
    Opname = 5
}

public enum ReferenceType
{
    StockIn = 0,
    StockOut = 1,
    StockTransfer = 2,
    StockAdjustment = 3,
    StockOpname = 4
}

public enum AuditActionType
{
    Create = 0,
    Update = 1,
    Delete = 2,
    Submit = 3,
    Approve = 4,
    Reject = 5
}
