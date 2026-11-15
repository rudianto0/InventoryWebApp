using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Contracts;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class StockAdjustmentServiceTests
{
    private static StockAdjustmentInput ValidInput(TestDatabase fixture, int counted = 8) => new()
    {
        LocationId = fixture.WarehouseId,
        Date = new DateTime(2026, 11, 15),
        Reason = "  correction  ",
        Lines = new List<AdjustmentLineInput> { new() { ProductId = fixture.ProductId, CountedQuantity = counted } }
    };

    [Fact]
    public async Task Create_ComputesSystemQuantityAndDifference()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var header = await service.CreateAsync(ValidInput(fixture, 8));

        header.Reason.Should().Be("correction");
        var detail = header.Details.Single();
        detail.SystemQuantity.Should().Be(10);
        detail.CountedQuantity.Should().Be(8);
        detail.Difference.Should().Be(-2);
    }

    [Fact]
    public async Task Create_RequiresReasonAndRejectsNegativeCounted()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var noReason = new StockAdjustmentInput
        {
            LocationId = fixture.WarehouseId,
            Reason = " ",
            Lines = ValidInput(fixture).Lines
        };

        await FluentActions.Awaiting(() => service.CreateAsync(noReason))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Alasan*");
        await FluentActions.Awaiting(() => service.CreateAsync(ValidInput(fixture, -1)))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Counted quantity*");
    }

    [Fact]
    public async Task Create_RejectsDuplicateProductAndInactiveLocation()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var duplicate = new StockAdjustmentInput
        {
            LocationId = fixture.WarehouseId,
            Reason = "dup",
            Lines = new()
            {
                new() { ProductId = fixture.ProductId, CountedQuantity = 1 },
                new() { ProductId = fixture.ProductId, CountedQuantity = 2 }
            }
        };
        var inactiveLocation = new StockAdjustmentInput
        {
            LocationId = Guid.NewGuid(),
            Reason = "x",
            Lines = ValidInput(fixture).Lines
        };

        await FluentActions.Awaiting(() => service.CreateAsync(duplicate))
            .Should().ThrowAsync<ValidationException>().WithMessage("*duplikat*");
        await FluentActions.Awaiting(() => service.CreateAsync(inactiveLocation))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Lokasi tidak ditemukan*");
    }

    [Fact]
    public async Task Update_RecomputesSystemQuantityAfterStockChange()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var header = await service.CreateAsync(ValidInput(fixture, 8));
        await fixture.Db.Database.ExecuteSqlRawAsync("UPDATE Stocks SET Quantity = 12");

        await service.UpdateAsync(header.Id, ValidInput(fixture, 9));

        var detail = await fixture.Db.StockAdjustmentDetails.SingleAsync();
        detail.SystemQuantity.Should().Be(12);
        detail.Difference.Should().Be(-3);
    }

    [Fact]
    public async Task AdminSubmit_AutoApprovesAndAppliesDifference()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Adjustment(fixture, ServiceFactory.Admin(1), ServiceFactory.Directory());

        var header = await service.CreateAsync(ValidInput(fixture, 12));
        await service.SubmitAsync(header.Id);

        (await fixture.Db.StockAdjustmentHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Approved);
        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(12);
        var movement = await fixture.Db.StockMovements.SingleAsync();
        movement.Type.Should().Be(MovementType.Adjustment);
        movement.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task Approve_AppliesNegativeDifference()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 7));
        await service.SubmitAsync(header.Id);
        var approver = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await approver.ApproveAsync(header.Id);

        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(7);
        (await fixture.Db.StockMovements.SingleAsync()).Quantity.Should().Be(-3);
    }

    [Fact]
    public async Task Reject_SetsRejectedStatus()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(header.Id);
        var approver = ServiceFactory.Adjustment(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await approver.RejectAsync(header.Id, "no");

        (await fixture.Db.StockAdjustmentHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Rejected);
    }
}

public class StockOpnameServiceTests
{
    private static StockOpnameInput Input(TestDatabase fixture) => new()
    {
        LocationId = fixture.WarehouseId,
        Date = new DateTime(2026, 11, 15)
    };

    [Fact]
    public async Task Create_UsesStocksFromSelectedLocation()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        await fixture.SeedStockAsync(fixture.ProductId, fixture.Warehouse2Id, 99);
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var header = await service.CreateAsync(Input(fixture));

        var detail = header.Details.Single();
        detail.SystemQuantity.Should().Be(10);
        detail.CountedQuantity.Should().BeNull();
        detail.Difference.Should().Be(0);
    }

    [Fact]
    public async Task Create_RejectsLocationWithoutStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var action = () => service.CreateAsync(Input(fixture));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Belum ada stok*");
    }

    [Fact]
    public async Task Update_SetsCountedAndDifference()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var header = await service.CreateAsync(Input(fixture));
        var detail = header.Details.Single();

        await service.UpdateAsync(header.Id, new[]
        {
            new OpnameLineInput { DetailId = detail.DetailId, CountedQuantity = 7, Notes = "  counted  " }
        }, "  note  ");

        var saved = await fixture.Db.StockOpnameDetails.SingleAsync();
        saved.CountedQuantity.Should().Be(7);
        saved.Difference.Should().Be(-3);
        saved.Notes.Should().Be("counted");
        (await fixture.Db.StockOpnameHeaders.SingleAsync()).Notes.Should().Be("note");
    }

    [Fact]
    public async Task Update_RejectsNegativeCounted()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var header = await service.CreateAsync(Input(fixture));

        var action = () => service.UpdateAsync(header.Id, new[]
        {
            new OpnameLineInput { DetailId = header.Details.Single().DetailId, CountedQuantity = -1 }
        }, null);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Counted quantity*");
    }

    [Fact]
    public async Task Submit_RejectsMissingCountedQuantity()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(Input(fixture));

        var action = () => service.SubmitAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*harus diisi*");
    }

    [Fact]
    public async Task Submit_RejectsSecondSubmittedSessionForSameLocation()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var first = await service.CreateAsync(Input(fixture));
        await service.UpdateAsync(first.Id, new[]
        {
            new OpnameLineInput { DetailId = first.Details.Single().DetailId, CountedQuantity = 10 }
        }, null);
        await service.SubmitAsync(first.Id);
        var second = await service.CreateAsync(Input(fixture));
        await service.UpdateAsync(second.Id, new[]
        {
            new OpnameLineInput { DetailId = second.Details.Single().DetailId, CountedQuantity = 10 }
        }, null);

        var action = () => service.SubmitAsync(second.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*sudah ada sesi opname*");
    }

    [Fact]
    public async Task AdminSubmit_AutoApprovesAndAppliesCountedDifference()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Admin(1), ServiceFactory.Directory());
        var header = await service.CreateAsync(Input(fixture));
        await service.UpdateAsync(header.Id, new[]
        {
            new OpnameLineInput { DetailId = header.Details.Single().DetailId, CountedQuantity = 7 }
        }, null);

        await service.SubmitAsync(header.Id);

        (await fixture.Db.StockOpnameHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Approved);
        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(7);
        (await fixture.Db.StockMovements.SingleAsync()).Type.Should().Be(MovementType.Opname);
    }

    [Fact]
    public async Task Approve_AppliesDifference()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10);
        var service = ServiceFactory.Opname(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(Input(fixture));
        await service.UpdateAsync(header.Id, new[]
        {
            new OpnameLineInput { DetailId = header.Details.Single().DetailId, CountedQuantity = 12 }
        }, null);
        await service.SubmitAsync(header.Id);
        var approver = ServiceFactory.Opname(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await approver.ApproveAsync(header.Id);

        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(12);
        (await fixture.Db.StockOpnameHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Approved);
    }
}