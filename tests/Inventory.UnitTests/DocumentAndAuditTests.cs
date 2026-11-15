using FluentAssertions;
using Inventory.Contracts;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class DocumentNumberServiceTests
{
    [Fact]
    public async Task Next_UsesFormatAndIncrements()
    {
        await using var fixture = new TestDatabase();
        var service = new DocumentNumberService(fixture.Db);
        var date = new DateTime(2026, 11, 15);

        var first = await service.NextAsync(" in ", date);
        var second = await service.NextAsync("IN", date);

        first.Should().Be("IN-202611-0001");
        second.Should().Be("IN-202611-0002");
    }

    [Fact]
    public async Task Next_ResetsPerMonthAndKeepsPrefixesIndependent()
    {
        await using var fixture = new TestDatabase();
        var service = new DocumentNumberService(fixture.Db);

        await service.NextAsync("IN", new DateTime(2026, 11, 1));
        var december = await service.NextAsync("IN", new DateTime(2026, 12, 1));
        var outNumber = await service.NextAsync("OUT", new DateTime(2026, 11, 1));

        december.Should().Be("IN-202612-0001");
        outNumber.Should().Be("OUT-202611-0001");
    }
}

public class AuditQueryServiceTests
{
    private static async Task SeedAsync(TestDatabase fixture)
    {
        fixture.Db.AuditLogs.AddRange(
            new AuditLog
            {
                EntityName = "Category", EntityId = "1", Action = "Create", UserId = 10,
                OldValues = null, NewValues = "{\"Name\":\"A\"}", IpAddress = "127.0.0.1",
                CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "Product", EntityId = "2", Action = "Update", UserId = 20,
                OldValues = "{\"Name\":\"A\"}", NewValues = "{\"Name\":\"B\"}",
                CreatedAt = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "Category", EntityId = "3", Action = "Delete", UserId = null,
                OldValues = "{\"Name\":\"C\"}", CreatedAt = new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc)
            });
        await fixture.Db.SaveChangesAsync();
    }

    [Fact]
    public async Task List_AppliesFiltersAndPagination()
    {
        await using var fixture = new TestDatabase();
        await SeedAsync(fixture);
        var service = new AuditQueryService(fixture.Db, new FakeUserDirectory());

        var byEntity = await service.ListAsync(new AuditFilter { EntityName = "Category" });
        var byAction = await service.ListAsync(new AuditFilter { Action = "Update" });
        var byUser = await service.ListAsync(new AuditFilter { UserId = 10 });
        var byDate = await service.ListAsync(new AuditFilter { From = new DateOnly(2026, 11, 2) });
        var clamped = await service.ListAsync(new AuditFilter { Page = 0, PageSize = 0 });

        byEntity.TotalCount.Should().Be(2);
        byAction.TotalCount.Should().Be(1);
        byUser.TotalCount.Should().Be(1);
        byDate.TotalCount.Should().Be(2);
        clamped.Page.Should().Be(1);
        clamped.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task List_ResolvesUserNames()
    {
        await using var fixture = new TestDatabase();
        await SeedAsync(fixture);
        var service = new AuditQueryService(fixture.Db, new FakeUserDirectory());

        var result = await service.ListAsync(new AuditFilter { Action = "Update" });

        result.Items.Single().UserName.Should().Be("User 20");
    }

    [Fact]
    public async Task Get_ReturnsDetailOrNull()
    {
        await using var fixture = new TestDatabase();
        await SeedAsync(fixture);
        var service = new AuditQueryService(fixture.Db, new FakeUserDirectory());
        var existing = await fixture.Db.AuditLogs.FirstAsync();

        var detail = await service.GetAsync(existing.Id);
        var missing = await service.GetAsync(9999);

        detail.Should().NotBeNull();
        detail!.EntityName.Should().Be(existing.EntityName);
        detail.UserName.Should().Be("User 10");
        missing.Should().BeNull();
    }

    [Fact]
    public async Task GetEntityNames_ReturnsDistinctSorted()
    {
        await using var fixture = new TestDatabase();
        await SeedAsync(fixture);
        var service = new AuditQueryService(fixture.Db, new FakeUserDirectory());

        var names = await service.GetEntityNamesAsync();

        names.Should().Contain(new[] { "Category", "Product" }).And.BeInAscendingOrder();
    }
}

public class AuditTrailExtraTests
{
    [Fact]
    public async Task Update_WritesOldAndNewValues()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        var category = await service.CreateCategoryAsync(new CategoryInput { Name = "Before" });

        await service.UpdateCategoryAsync(category.Id, new CategoryInput { Name = "After", Description = "d" });

        var log = await fixture.Db.AuditLogs
            .Where(x => x.EntityName == nameof(Category) && x.Action == "Update")
            .OrderByDescending(x => x.Id)
            .FirstAsync();
        log.OldValues.Should().Contain("Before");
        log.NewValues.Should().Contain("After");
    }

    [Fact]
    public async Task Submit_WritesSubmitAction()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(new StockInInput
        {
            SupplierId = fixture.SupplierId,
            LocationId = fixture.WarehouseId,
            Lines = new() { new DocumentLineInput { ProductId = fixture.ProductId, Quantity = 1m } }
        });

        await service.SubmitAsync(header.Id);

        var log = await fixture.Db.AuditLogs
            .SingleAsync(x => x.EntityName == nameof(StockInHeader) && x.Action == "Submit");
        log.EntityId.Should().Be(header.Id.ToString());
    }
}
