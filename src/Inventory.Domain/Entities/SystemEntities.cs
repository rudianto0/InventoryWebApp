namespace Inventory.Domain.Entities;

public class DocumentSequence
{
    public int Id { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int LastNumber { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
