using Inventory.Contracts.Enums;

namespace Inventory.Contracts;

public sealed class CategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int? CreatedById { get; init; }
}

public sealed class UnitResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public sealed class ProductResponse
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Guid UnitId { get; init; }
    public int ReorderLevel { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class SupplierResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; }
}

public sealed class LocationResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public LocationType Type { get; init; }
    public Guid? ParentLocationId { get; init; }
    public string? ParentLocationName { get; init; }
    public string? BranchCode { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; }
}

public sealed class TransactionCreatedResponse
{
    public Guid Id { get; init; }
    public string DocNo { get; init; } = string.Empty;
    public ApprovalStatus Status { get; init; }
    public int? CreatedById { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? SupplierId { get; init; }
    public Guid? LocationId { get; init; }
    public Guid? FromLocationId { get; init; }
    public Guid? ToLocationId { get; init; }
    public string? Purpose { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<DocumentLineView> Details { get; init; } = Array.Empty<DocumentLineView>();
}
