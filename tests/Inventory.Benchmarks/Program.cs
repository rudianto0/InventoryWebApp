using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Inventory.Application.Abstractions;
using Inventory.Contracts;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

BenchmarkRunner.Run<InventoryBenchmarks>();

public sealed class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; set; } = 20;
    public string? UserName { get; set; } = "bench";
    public string[] Roles { get; set; } = Array.Empty<string>();
    public bool IsInRole(string role) => Roles.Contains(role);
}

public sealed class FakeUserDirectory : IUserDirectory
{
    public int? ParentId { get; set; } = 20;
    public bool Active { get; set; } = true;
    public Task<int?> GetParentIdAsync(int userId, CancellationToken ct = default) => Task.FromResult(ParentId);
    public Task<bool> IsActiveAsync(int userId, CancellationToken ct = default) => Task.FromResult(Active);
    public Task<string?> GetDisplayNameAsync(int userId, CancellationToken ct = default) => Task.FromResult<string?>($"User {userId}");
    public Task<IReadOnlyDictionary<int, string>> GetDisplayNamesAsync(IEnumerable<int> userIds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyDictionary<int, string>>(userIds.Distinct().ToDictionary(x => x, x => $"User {x}"));
}

public sealed class FakeDocumentNumbers : IDocumentNumberService
{
    private int _sequence;
    public Task<string> NextAsync(string prefix, DateTime date, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-{date:yyyyMM}-{++_sequence:0000}");
}

[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class InventoryBenchmarks
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;
    private Guid _productId;
    private Guid _locationId;
    private Guid _categoryId;
    private Guid _unitId;
    private List<StockInHeader> _headers = new();
    private List<(int Id, string Name)> _users = new();
    private Guid[] _productIds = Array.Empty<Guid>();

    [GlobalSetup]
    public async Task Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new AppDbContext(options, new FakeCurrentUser(), new HttpContextAccessor());
        await _db.Database.EnsureCreatedAsync();

        var category = new Category { Name = "Bench" };
        var unit = new Unit { Code = "PCS", Name = "Pieces" };
        _db.Categories.Add(category);
        _db.Units.Add(unit);
        await _db.SaveChangesAsync();

        var location = new Location { Code = "WH", Name = "Warehouse", Type = LocationType.Warehouse, IsActive = true };
        var supplier = new Supplier { Code = "SUP", Name = "Supplier" };
        _db.Locations.Add(location);
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        _categoryId = category.Id;
        _unitId = unit.Id;
        _locationId = location.Id;

        var products = Enumerable.Range(0, 100)
            .Select(i => new Product { Sku = $"BENCH-{i:000}", Name = $"Bench {i}", CategoryId = _categoryId, UnitId = _unitId })
            .ToList();
        _db.Products.AddRange(products);
        await _db.SaveChangesAsync();
        _productIds = products.Select(x => x.Id).ToArray();
        _productId = products[0].Id;

        _db.Stocks.Add(new Stock { ProductId = _productId, LocationId = _locationId, Quantity = 1_000_000 });
        await _db.SaveChangesAsync();

        for (var i = 0; i < 100; i++)
        {
            _headers.Add(new StockInHeader
            {
                DocNo = $"BENCH-IN-{i:0000}",
                SupplierId = supplier.Id,
                LocationId = _locationId,
                Status = ApprovalStatus.Submitted,
                ApproverId = 20,
                CreatedById = 10,
                SubmittedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        _db.StockInHeaders.AddRange(_headers);

        for (var i = 0; i < 10_000; i++)
        {
            _db.StockMovements.Add(new StockMovement
            {
                MovementNo = $"MOV-B-{i:00000}",
                Type = MovementType.In,
                ProductId = _productId,
                LocationId = _locationId,
                Quantity = 1,
                ReferenceType = ReferenceType.StockIn,
                ReferenceId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddSeconds(-i)
            });
        }
        await _db.SaveChangesAsync();

        _users = Enumerable.Range(1, 100).Select(i => (i, $"User {i}")).ToList();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Benchmark]
    public async Task Ledger_Apply_100()
    {
        var ledger = new StockLedger(_db, new FakeDocumentNumbers());
        var changes = Enumerable.Range(0, 100).Select(_ => new StockChange(
            MovementType.In, ReferenceType.StockIn, Guid.NewGuid(),
            _productId, _locationId, 1, 1, null)).ToList();
        await ledger.ApplyAsync(changes);
        await _db.SaveChangesAsync();
    }

    [Benchmark]
    public async Task Approval_Pending_100()
    {
        var service = new ApprovalService(_db, new FakeCurrentUser { UserId = 20 }, new FakeUserDirectory());
        await service.GetPendingAsync();
    }

    [Benchmark]
    public async Task StockCard_10000()
    {
        var service = new ReportService(_db);
        await service.GetStockCardAsync(new ReportFilter { ProductId = _productId });
    }

    [Benchmark]
    public async Task StockCheck_Naive_100()
    {
        var total = 0;
        foreach (var id in _productIds)
        {
            total += await _db.Stocks
                .Where(x => x.ProductId == id && x.LocationId == _locationId)
                .Select(x => x.Quantity)
                .FirstOrDefaultAsync();
        }
        GC.KeepAlive(total);
    }

    [Benchmark]
    public async Task StockCheck_Batch_100()
    {
        var stocks = await _db.Stocks
            .Where(x => _productIds.Contains(x.ProductId) && x.LocationId == _locationId)
            .Select(x => new { x.ProductId, x.Quantity })
            .ToListAsync();
        GC.KeepAlive(stocks);
    }

    [Benchmark]
    public int CreatorLookup_Naive_100()
    {
        var total = 0;
        foreach (var header in _headers)
            total += _users.First(u => u.Id == header.CreatedById).Id;
        return total;
    }

    [Benchmark]
    public int CreatorLookup_Batch_100()
    {
        var lookup = _users.ToDictionary(x => x.Id, x => x.Name);
        var total = 0;
        foreach (var header in _headers)
            total += lookup[header.CreatedById!.Value].Length;
        return total;
    }
}
