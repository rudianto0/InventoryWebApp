using Inventory.Contracts.Enums;

namespace Inventory.Contracts;

public sealed class CategoryInput
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UnitInput
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public sealed class ProductInput
{
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Guid UnitId { get; init; }
    public int ReorderLevel { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class SupplierInput
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class LocationInput
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Enums.LocationType Type { get; init; }
    public Guid? ParentLocationId { get; init; }
    public string? BranchCode { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class ProductListItem
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public int ReorderLevel { get; init; }
    public bool IsActive { get; init; }
}

public sealed class ProductFormData
{
    public ProductInput Input { get; init; } = new();
    public Guid? Id { get; init; }
}
