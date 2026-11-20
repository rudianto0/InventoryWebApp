using FluentAssertions;
using Inventory.Application.Services;
using Inventory.Contracts;
using Inventory.Contracts.Enums;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class MasterSortingTests
{
    [Fact]
    public async Task Categories_SortByName_Descending_IsCaseInsensitive()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        await service.CreateCategoryAsync(new CategoryInput { Name = "alpha" });
        await service.CreateCategoryAsync(new CategoryInput { Name = "Beta" });
        await service.CreateCategoryAsync(new CategoryInput { Name = "GAMMA" });

        var result = await service.GetCategoriesAsync(sort: new SortSpec { SortBy = "name", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.Name).Should().ContainInOrder("GAMMA", "Beta", "alpha");
    }

    [Fact]
    public async Task Categories_InvalidSortKey_FallsBackToNameAscending()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        await service.CreateCategoryAsync(new CategoryInput { Name = "Zeta" });
        await service.CreateCategoryAsync(new CategoryInput { Name = "Alpha" });

        var result = await service.GetCategoriesAsync(sort: new SortSpec { SortBy = "not-a-column", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.Name).Should().ContainInOrder("Alpha", "Zeta");
    }

    [Fact]
    public async Task Categories_Pagination_ReturnsRequestedPage()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        await service.CreateCategoryAsync(new CategoryInput { Name = "A" });
        await service.CreateCategoryAsync(new CategoryInput { Name = "B" });
        await service.CreateCategoryAsync(new CategoryInput { Name = "C" });

        var first = await service.GetCategoriesAsync(page: 1, pageSize: 2);
        var second = await service.GetCategoriesAsync(page: 2, pageSize: 2);

        first.TotalCount.Should().Be(3);
        first.Items.Select(x => x.Name).Should().ContainInOrder("A", "B");
        second.Items.Select(x => x.Name).Should().ContainInOrder("C");
    }

    [Fact]
    public async Task Products_SortByReorderLevel_Descending()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedProductAsync("SKU-LOW", reorderLevel: 1);
        await fixture.SeedProductAsync("SKU-HIGH", reorderLevel: 50);
        var service = new CatalogService(fixture.Db);

        var result = await service.GetProductsAsync(sort: new SortSpec { SortBy = "reorder", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.ReorderLevel).Should().BeInDescendingOrder();
        result.Items.First().Sku.Should().Be("SKU-HIGH");
    }

    [Fact]
    public async Task Locations_SortByName_Descending()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new LocationService(fixture.Db);

        var result = await service.ListAsync(sort: new SortSpec { SortBy = "name", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.Name).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Suppliers_SortByName_Descending()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new SupplierService(fixture.Db);
        await service.CreateAsync(new SupplierInput { Code = "SUP-A", Name = "alpha" });
        await service.CreateAsync(new SupplierInput { Code = "SUP-B", Name = "Zulu" });

        var result = await service.ListAsync(sort: new SortSpec { SortBy = "name", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.Name).Should().ContainInOrder("Zulu", "Supplier", "alpha");
    }
}

public class TransactionSortingTests
{
    private static StockInInput Input(TestDatabase fixture, DateTime date) => new()
    {
        SupplierId = fixture.SupplierId,
        LocationId = fixture.WarehouseId,
        Date = date,
        Lines = new List<DocumentLineInput>
        {
            new() { ProductId = fixture.ProductId, Quantity = 1, UnitCost = 10m }
        }
    };

    [Fact]
    public async Task StockIn_SortByDocNo_Ascending()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        await service.CreateAsync(Input(fixture, new DateTime(2026, 11, 15)));
        await service.CreateAsync(Input(fixture, new DateTime(2026, 11, 16)));

        var result = await service.ListAsync(new TransactionFilter { SortBy = "docno", SortDir = SortDirection.Asc });

        result.Items.Select(x => x.DocNo).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task StockIn_SortByDate_Descending_PutsNewestFirst()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var older = await service.CreateAsync(Input(fixture, new DateTime(2026, 11, 10)));
        var newer = await service.CreateAsync(Input(fixture, new DateTime(2026, 11, 20)));

        var result = await service.ListAsync(new TransactionFilter { SortBy = "date", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.Id).Should().ContainInOrder(newer.Id, older.Id);
    }
}

public class ReportSortingTests
{
    [Fact]
    public async Task StockBalance_SortByQuantity_Descending()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        await fixture.SeedStockAsync(fixture.ProductId, fixture.Warehouse2Id, 5);
        var service = new ReportService(fixture.Db);

        var result = await service.GetStockBalanceAsync(new ReportFilter { SortBy = "quantity", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.Quantity).Should().BeInDescendingOrder();
        result.Items.First().Quantity.Should().Be(10);
    }

    [Fact]
    public async Task Reorder_SortByQuantity_Descending()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var product = await fixture.Db.Products.SingleAsync();
        product.ReorderLevel = 20;
        await fixture.Db.SaveChangesAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 5);
        await fixture.SeedStockAsync(fixture.ProductId, fixture.Warehouse2Id, 10);
        var service = new ReportService(fixture.Db);

        var result = await service.GetReorderAsync(new ReportFilter { SortBy = "quantity", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.Quantity).Should().BeInDescendingOrder();
        result.Items.First().Quantity.Should().Be(10);
    }
}

public class AuditSortingTests
{
    [Fact]
    public async Task Audit_SortByEntity_Descending()
    {
        await using var fixture = new TestDatabase();
        fixture.Db.AuditLogs.AddRange(
            new AuditLog
            {
                EntityName = "Category", EntityId = "1", Action = "Create",
                CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "Product", EntityId = "2", Action = "Update",
                CreatedAt = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc)
            });
        await fixture.Db.SaveChangesAsync();
        var service = new AuditQueryService(fixture.Db, new FakeUserDirectory());

        var result = await service.ListAsync(new AuditFilter { SortBy = "entity", SortDir = SortDirection.Desc });

        result.Items.Select(x => x.EntityName).Should().ContainInOrder("Product", "Category");
    }
}
