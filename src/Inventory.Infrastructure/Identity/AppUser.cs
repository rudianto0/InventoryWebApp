using Inventory.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Inventory.Infrastructure.Identity;

public class AppUser : IdentityUser<int>, IAuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public AppUser? Parent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
}

public class AppRole : IdentityRole<int>
{
    public string? Description { get; set; }
}
