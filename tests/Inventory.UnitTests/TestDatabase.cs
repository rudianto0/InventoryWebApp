using Inventory.Application.Abstractions;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

internal sealed class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; init; } = 1;
    public string? UserName { get; init; } = "test";
    public string[] Roles { get; init; } = Array.Empty<string>();
    public bool IsInRole(string role) => Roles.Contains(role);
}

internal sealed class FakeUserDirectory : IUserDirectory
{
    public int? ParentId { get; set; } = 2;
    public bool Active { get; set; } = true;

    public Task<int?> GetParentIdAsync(int userId, CancellationToken ct = default) => Task.FromResult(ParentId);
    public Task<bool> IsActiveAsync(int userId, CancellationToken ct = default) => Task.FromResult(Active);
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

    public TestDatabase(ICurrentUserService? currentUser = null)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        Db = new AppDbContext(options, currentUser ?? new FakeCurrentUser(), new HttpContextAccessor());
        Db.Database.EnsureCreated();
    }

    public async Task SeedBasicsAsync()
    {
        var category = new Category { Name = "General" };
        var unit = new Unit { Code = "PCS", Name = "Pieces" };
        Db.Categories.Add(category);
        Db.Units.Add(unit);
        await Db.SaveChangesAsync();
        Db.Products.Add(new Product { Sku = "SKU-1", Name = "Product 1", CategoryId = category.Id, UnitId = unit.Id });
        Db.Locations.Add(new Location { Code = "HQ", Name = "HQ", Type = LocationType.Branch, IsActive = true });
        Db.Locations.Add(new Location { Code = "WH1", Name = "WH1", Type = LocationType.Warehouse, IsActive = true });
        Db.Suppliers.Add(new Supplier { Code = "SUP", Name = "Supplier" });
        await Db.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
