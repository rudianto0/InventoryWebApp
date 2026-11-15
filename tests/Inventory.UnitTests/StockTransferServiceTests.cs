using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Contracts;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class StockTransferServiceTests
{
    private static StockTransferInput ValidInput(TestDatabase fixture, decimal quantity = 4m) => new()
    {
        FromLocationId = fixture.WarehouseId,
        ToLocationId = fixture.Warehouse2Id,
        Date = new DateTime(2026, 11, 15),
        Lines = new List<DocumentLineInput> { new() { ProductId = fixture.ProductId, Quantity = quantity } }
    };

    [Fact]
    public async Task Create_PersistsDraft()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var header = await service.CreateAsync(ValidInput(fixture));

        header.Status.Should().Be(ApprovalStatus.Draft);
        header.Details.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_RejectsSameLocation()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var input = new StockTransferInput
        {
            FromLocationId = fixture.WarehouseId,
            ToLocationId = fixture.WarehouseId,
            Lines = ValidInput(fixture).Lines
        };

        var action = () => service.CreateAsync(input);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*tidak boleh sama*");
    }

    [Fact]
    public async Task Create_RejectsMissingOrInactiveLocations()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var missing = new StockTransferInput
        {
            FromLocationId = Guid.NewGuid(),
            ToLocationId = fixture.Warehouse2Id,
            Lines = ValidInput(fixture).Lines
        };
        var location = await fixture.Db.Locations.SingleAsync(x => x.Id == fixture.WarehouseId);
        location.IsActive = false;
        await fixture.Db.SaveChangesAsync();

        await FluentActions.Awaiting(() => service.CreateAsync(missing))
            .Should().ThrowAsync<ValidationException>().WithMessage("*tidak ditemukan*");
        await FluentActions.Awaiting(() => service.CreateAsync(ValidInput(fixture)))
            .Should().ThrowAsync<ValidationException>().WithMessage("*harus aktif*");
    }

    [Fact]
    public async Task Create_RejectsDuplicateOrNonPositiveLines()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var duplicate = new StockTransferInput
        {
            FromLocationId = fixture.WarehouseId,
            ToLocationId = fixture.Warehouse2Id,
            Lines = new()
            {
                new() { ProductId = fixture.ProductId, Quantity = 1m },
                new() { ProductId = fixture.ProductId, Quantity = 1m }
            }
        };

        await FluentActions.Awaiting(() => service.CreateAsync(duplicate))
            .Should().ThrowAsync<ValidationException>().WithMessage("*duplikat*");
        await FluentActions.Awaiting(() => service.CreateAsync(ValidInput(fixture, 0m)))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Quantity*");
    }

    [Fact]
    public async Task Submit_RejectsInsufficientSourceStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 2m);
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 5m));

        var action = () => service.SubmitAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Stok asal tidak mencukupi*");
    }

    [Fact]
    public async Task Submit_SucceedsWithEnoughStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture));

        await service.SubmitAsync(header.Id);

        (await fixture.Db.StockTransferHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Submitted);
    }

    [Fact]
    public async Task Approve_MovesStockAndCreatesTwoMovements()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 4m));
        await service.SubmitAsync(header.Id);
        var approver = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await approver.ApproveAsync(header.Id);

        (await fixture.Db.Stocks.SingleAsync(x => x.LocationId == fixture.WarehouseId)).Quantity.Should().Be(6m);
        (await fixture.Db.Stocks.SingleAsync(x => x.LocationId == fixture.Warehouse2Id)).Quantity.Should().Be(4m);
        var movements = await fixture.Db.StockMovements.ToListAsync();
        movements.Should().HaveCount(2);
        movements.Should().Contain(x => x.Type == MovementType.TransferOut);
        movements.Should().Contain(x => x.Type == MovementType.TransferIn);
    }

    [Fact]
    public async Task Approve_RejectsWhenSourceStockChanged()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 5m));
        await service.SubmitAsync(header.Id);
        var stock = await fixture.Db.Stocks.SingleAsync(x => x.LocationId == fixture.WarehouseId);
        stock.Quantity = 1m;
        await fixture.Db.SaveChangesAsync();
        var approver = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        var action = () => approver.ApproveAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Stok asal tidak mencukupi*");
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AdminSubmit_AutoApprovesAndMovesStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Admin(1), ServiceFactory.Directory());

        var header = await service.CreateAsync(ValidInput(fixture, 3m));
        await service.SubmitAsync(header.Id);

        (await fixture.Db.StockTransferHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Approved);
        (await fixture.Db.Stocks.SingleAsync(x => x.LocationId == fixture.WarehouseId)).Quantity.Should().Be(7m);
        (await fixture.Db.Stocks.SingleAsync(x => x.LocationId == fixture.Warehouse2Id)).Quantity.Should().Be(3m);
    }

    [Fact]
    public async Task Reject_SetsRejectedStatus()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(header.Id);
        var approver = ServiceFactory.Transfer(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await approver.RejectAsync(header.Id, "no");

        (await fixture.Db.StockTransferHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Rejected);
    }
}
