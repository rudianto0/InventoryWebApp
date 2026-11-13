using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class Stock
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    public decimal Quantity { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; }
}

public class StockMovement
{
    public int Id { get; set; }
    public string MovementNo { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    public decimal Quantity { get; set; }
    public ReferenceType ReferenceType { get; set; }
    public int ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedById { get; set; }
}
