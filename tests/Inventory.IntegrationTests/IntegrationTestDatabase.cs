using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Inventory.IntegrationTests;

internal sealed class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; set; } = 10;
    public string? UserName { get; set; } = "staff";
    public string[] Roles { get; set; } = new[] { "Staff" };
    public bool IsInRole(string role) => Roles.Contains(role);
}

internal sealed class FakeUserDirectory : IUserDirectory
{
    public int? ParentId { get; set; } = 20;
    public bool Active { get; set; } = true;

    public Task<int?> GetParentIdAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult(ParentId);

    public Task<bool> IsActiveAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult(Active);

    public Task<string?> GetDisplayNameAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult<string?>($"User {userId}");

    public Task<IReadOnlyDictionary<int, string>> GetDisplayNamesAsync(IEnumerable<int> userIds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyDictionary<int, string>>(userIds.Distinct().ToDictionary(x => x, x => $"User {x}"));
}

internal sealed class FakeDocumentNumbers : IDocumentNumberService
{
    private int _sequence;
    public Task<string> NextAsync(string prefix, DateTime date, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-{date:yyyyMM}-{++_sequence:0000}");
}

internal sealed class IntegrationDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Db { get; }
    public FakeCurrentUser CurrentUser { get; }

    public Guid CategoryId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid SupplierId { get; private set; }

    public IntegrationDatabase(FakeCurrentUser? currentUser = null)
    {
        CurrentUser = currentUser ?? new FakeCurrentUser();
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
        var warehouse = new Location { Code = "WH1", Name = "WH1", Type = LocationType.Warehouse, IsActive = true };
        var supplier = new Supplier { Code = "SUP", Name = "Supplier" };
        Db.Products.Add(product);
        Db.Locations.Add(warehouse);
        Db.Suppliers.Add(supplier);
        await Db.SaveChangesAsync();

        CategoryId = category.Id;
        UnitId = unit.Id;
        ProductId = product.Id;
        WarehouseId = warehouse.Id;
        SupplierId = supplier.Id;
    }

    public async Task<Product> SeedProductAsync(string sku, bool isActive = true)
    {
        var product = new Product
        {
            Sku = sku,
            Name = sku,
            CategoryId = CategoryId,
            UnitId = UnitId,
            IsActive = isActive
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
