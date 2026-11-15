using FluentAssertions;
using Inventory.Contracts;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class ReportServiceTests
{
    [Fact]
    public async Task Dashboard_SumsActiveDataAndPendingApprovals()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var product = await fixture.Db.Products.SingleAsync();
        product.ReorderLevel = 20;
        fixture.Db.StockInHeaders.Add(new StockInHeader
        {
            DocNo = "IN-1", LocationId = fixture.WarehouseId, SupplierId = fixture.SupplierId,
            Status = ApprovalStatus.Submitted, ApproverId = 20, CreatedById = 10
        });
        await fixture.Db.SaveChangesAsync();
        var service = new ReportService(fixture.Db);

        var dashboard = await service.GetDashboardAsync();

        dashboard.TotalProducts.Should().Be(1);
        dashboard.TotalStockQuantity.Should().Be(10);
        dashboard.TotalLocations.Should().Be(3);
        dashboard.PendingApprovals.Should().Be(1);
        dashboard.BelowReorderCount.Should().Be(1);
        dashboard.ReorderItems.Should().ContainSingle(x => x.ProductId == fixture.ProductId);
    }

    [Fact]
    public async Task Dashboard_FiltersStockByLocation()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        await fixture.SeedStockAsync(fixture.ProductId, fixture.Warehouse2Id, 5);
        var service = new ReportService(fixture.Db);

        var dashboard = await service.GetDashboardAsync(fixture.Warehouse2Id);

        dashboard.TotalStockQuantity.Should().Be(5);
    }

    [Fact]
    public async Task StockBalance_AppliesFiltersAndPagination()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        await fixture.SeedStockAsync(fixture.ProductId, fixture.Warehouse2Id, 5);
        var service = new ReportService(fixture.Db);

        var all = await service.GetStockBalanceAsync(new ReportFilter());
        var filtered = await service.GetStockBalanceAsync(new ReportFilter { LocationId = fixture.Warehouse2Id });
        var clamped = await service.GetStockBalanceAsync(new ReportFilter { Page = 0, PageSize = 500 });

        all.TotalCount.Should().Be(2);
        filtered.TotalCount.Should().Be(1);
        clamped.Page.Should().Be(1);
        clamped.PageSize.Should().Be(200);
    }

    [Fact]
    public async Task StockCard_ReturnsEmptyWithoutProduct()
    {
        await using var fixture = new TestDatabase();
        var service = new ReportService(fixture.Db);

        var result = await service.GetStockCardAsync(new ReportFilter());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task StockCard_ComputesOpeningAndRunningBalance()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        fixture.Db.StockMovements.AddRange(
            new StockMovement
            {
                MovementNo = "MOV-1", Type = MovementType.In, ProductId = fixture.ProductId,
                LocationId = fixture.WarehouseId, Quantity = 5, CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new StockMovement
            {
                MovementNo = "MOV-2", Type = MovementType.In, ProductId = fixture.ProductId,
                LocationId = fixture.WarehouseId, Quantity = 3, CreatedAt = new DateTime(2026, 11, 15, 0, 0, 0, DateTimeKind.Utc)
            });
        await fixture.Db.SaveChangesAsync();
        var opening = await fixture.Db.StockMovements.SingleAsync(x => x.MovementNo == "MOV-1");
        opening.CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var current = await fixture.Db.StockMovements.SingleAsync(x => x.MovementNo == "MOV-2");
        current.CreatedAt = new DateTime(2026, 11, 15, 0, 0, 0, DateTimeKind.Utc);
        await fixture.Db.SaveChangesAsync();
        var service = new ReportService(fixture.Db);

        var result = await service.GetStockCardAsync(new ReportFilter
        {
            ProductId = fixture.ProductId,
            From = new DateOnly(2026, 11, 10),
            To = new DateOnly(2026, 11, 15)
        });

        result.Should().ContainSingle();
        result[0].RunningBalance.Should().Be(8);
    }

    [Fact]
    public async Task Movements_ApplyFiltersAndOrderDescending()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        fixture.Db.StockMovements.AddRange(
            new StockMovement
            {
                MovementNo = "MOV-1", Type = MovementType.In, ProductId = fixture.ProductId,
                LocationId = fixture.WarehouseId, Quantity = 5, CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new StockMovement
            {
                MovementNo = "MOV-2", Type = MovementType.In, ProductId = fixture.ProductId,
                LocationId = fixture.WarehouseId, Quantity = 3, CreatedAt = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc)
            });
        await fixture.Db.SaveChangesAsync();
        var first = await fixture.Db.StockMovements.SingleAsync(x => x.MovementNo == "MOV-1");
        first.CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var second = await fixture.Db.StockMovements.SingleAsync(x => x.MovementNo == "MOV-2");
        second.CreatedAt = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc);
        await fixture.Db.SaveChangesAsync();
        var service = new ReportService(fixture.Db);

        var result = await service.GetMovementsAsync(new ReportFilter { ProductId = fixture.ProductId });

        result.TotalCount.Should().Be(2);
        result.Items[0].MovementNo.Should().Be("MOV-2");
    }

    [Fact]
    public async Task Reorder_ReturnsOnlyAtOrBelowLevel()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 5);
        var product = await fixture.Db.Products.SingleAsync();
        product.ReorderLevel = 5;
        await fixture.Db.SaveChangesAsync();
        var service = new ReportService(fixture.Db);

        var result = await service.GetReorderAsync(new ReportFilter());

        result.TotalCount.Should().Be(1);
        result.Items.Single().ReorderLevel.Should().Be(5);
    }

    [Fact]
    public async Task ExportCsv_WritesHeadersAndEscapesValues()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        fixture.Db.StockMovements.Add(new StockMovement
        {
            MovementNo = "MOV-1", Type = MovementType.In, ProductId = fixture.ProductId,
            LocationId = fixture.WarehouseId, Quantity = 5, Notes = "a,b", CreatedAt = DateTime.UtcNow
        });
        await fixture.Db.SaveChangesAsync();
        var service = new ReportService(fixture.Db);

        var csv = await service.ExportCsvAsync("movements", new ReportFilter());

        csv.Should().Contain("Date,MovementNo,Type,SKU,Product,Location,Quantity,Notes");
        csv.Should().Contain("\"a,b\"");
    }

    [Fact]
    public async Task ExportCsv_StockBalanceAndReorderHaveHeaders()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 1);
        var service = new ReportService(fixture.Db);

        var balance = await service.ExportCsvAsync("stock-balance", new ReportFilter());
        var reorder = await service.ExportCsvAsync("reorder", new ReportFilter());

        balance.Should().StartWith("SKU,Product,Unit,Location,BranchCode,Quantity,UpdatedAt");
        reorder.Should().StartWith("SKU,Product,Location,Quantity,ReorderLevel");
    }
}