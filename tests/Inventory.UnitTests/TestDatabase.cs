using Inventory.Application.Abstractions;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

internal sealed class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "test";
    public string[] Roles { get; set; } = Array.Empty<string>();
    public bool IsInRole(string role) => Roles.Contains(role);
}

internal sealed class FakeUserDirectory : IUserDirectory
{
    public int? ParentId { get; set; } = 2;
    public bool Active { get; set; } = true;
    public Dictionary<int, int?> Parents { get; } = new();
    public Dictionary<int, bool> ActiveUsers { get; } = new();

    public Task<int?> GetParentIdAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult(Parents.TryGetValue(userId, out var parent) ? parent : ParentId);

    public Task<bool> IsActiveAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult(ActiveUsers.TryGetValue(userId, out var active) ? active : Active);

    public Task<string?> GetDisplayNameAsync(int userId, CancellationToken ct = default) => Task.FromResult<string?>($"User {userId}");

    public Task<IReadOnlyDictionary<int, string>> GetDisplayNamesAsync(IEnumerable<int> userIds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyDictionary<int, string>>(userIds.Distinct().ToDictionary(x => x, x => $"User {x}"));
}

internal sealed class FakeDocumentNumbers : IDocumentNumberService
{
    private int _sequence;
    public Task<string> NextAsync(string prefix, DateTime date, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-{date:yyyyMM}-{++_sequence:0000}");
}

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Db { get; }
    public FakeCurrentUser CurrentUser { get; }

    public Guid CategoryId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid Warehouse2Id { get; private set; }
    public Guid SupplierId { get; private set; }

    public TestDatabase(ICurrentUserService? currentUser = null)
    {
        CurrentUser = currentUser as FakeCurrentUser ?? new FakeCurrentUser();
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        Db = new AppDbContext(options, CurrentUser, new HttpContextAccessor());
        Db.Database.EnsureCreated();
    }

    public async Task SeedBasicsAsync()
    {
        var category = new Category { Name = "General" };
        var unit = new Unit { Code = "PCS", Name = "Pieces" };
        Db.Categories.Add(category);
        Db.Units.Add(unit);
        await Db.SaveChangesAsync();

        var product = new Product { Sku = "SKU-1", Name = "Product 1", CategoryId = category.Id, UnitId = unit.Id };
        var branch = new Location { Code = "HQ", Name = "HQ", Type = LocationType.Branch, IsActive = true };
        var warehouse = new Location { Code = "WH1", Name = "WH1", Type = LocationType.Warehouse, IsActive = true };
        var warehouse2 = new Location { Code = "WH2", Name = "WH2", Type = LocationType.Warehouse, IsActive = true };
        var supplier = new Supplier { Code = "SUP", Name = "Supplier" };
        Db.Products.Add(product);
        Db.Locations.Add(branch);
        Db.Locations.Add(warehouse);
        Db.Locations.Add(warehouse2);
        Db.Suppliers.Add(supplier);
        await Db.SaveChangesAsync();

        CategoryId = category.Id;
        UnitId = unit.Id;
        ProductId = product.Id;
        BranchId = branch.Id;
        WarehouseId = warehouse.Id;
        Warehouse2Id = warehouse2.Id;
        SupplierId = supplier.Id;
    }

    public async Task SeedStockAsync(Guid productId, Guid locationId, int quantity)
    {
        Db.Stocks.Add(new Stock { ProductId = productId, LocationId = locationId, Quantity = quantity });
        await Db.SaveChangesAsync();
    }

    public async Task<Location> SeedLocationAsync(string code, LocationType type, bool isActive = true, Guid? parentId = null)
    {
        var location = new Location { Code = code, Name = code, Type = type, IsActive = isActive, ParentLocationId = parentId };
        Db.Locations.Add(location);
        await Db.SaveChangesAsync();
        return location;
    }

    public async Task<Product> SeedProductAsync(string sku, bool isActive = true, int reorderLevel = 0)
    {
        var product = new Product
        {
            Sku = sku,
            Name = sku,
            CategoryId = CategoryId,
            UnitId = UnitId,
            IsActive = isActive,
            ReorderLevel = reorderLevel
        };
        Db.Products.Add(product);
        await Db.SaveChangesAsync();
        return product;
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}