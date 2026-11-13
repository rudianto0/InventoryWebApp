using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class Location
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public int? ParentLocationId { get; set; }
    public Location? ParentLocation { get; set; }
    public string? BranchCode { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Location> Children { get; set; } = new List<Location>();
}
