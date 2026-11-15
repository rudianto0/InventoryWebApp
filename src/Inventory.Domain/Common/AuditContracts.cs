namespace Inventory.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    int? CreatedById { get; set; }
    DateTime? UpdatedAt { get; set; }
    int? UpdatedById { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedById { get; set; }
}
