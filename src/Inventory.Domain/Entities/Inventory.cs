using Inventory.Domain.Common;
using Inventory.Contracts.Enums;

namespace Inventory.Domain.Entities;

public class Stock : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public decimal Quantity { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public int? UpdatedById { get; set; }

    DateTime? IAuditableEntity.UpdatedAt
    {
        get => UpdatedAt;
        set => UpdatedAt = value ?? DateTime.UtcNow;
    }
}

public class StockMovement : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string MovementNo { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public decimal Quantity { get; set; }
    public ReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
}
