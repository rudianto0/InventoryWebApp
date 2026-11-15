using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Contracts;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class StockOutServiceTests
{
    private static StockOutInput ValidInput(TestDatabase fixture, decimal quantity = 5m) => new()
    {
        LocationId = fixture.WarehouseId,
        Date = new DateTime(2026, 11, 15),
        Purpose = "  usage  ",
        Lines = new List<DocumentLineInput> { new() { ProductId = fixture.ProductId, Quantity = quantity } }
    };

    [Fact]
    public async Task Create_AllowsQuantityAboveStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var header = await service.CreateAsync(ValidInput(fixture, 100m));

        header.Status.Should().Be(ApprovalStatus.Draft);
        header.Purpose.Should().Be("usage");
    }

    [Fact]
    public async Task Create_RejectsEmptyLinesInactiveLocationOrInvalidQuantity()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var empty = new StockOutInput { LocationId = fixture.WarehouseId, Lines = new() };
        var inactiveLocation = new StockOutInput { LocationId = Guid.NewGuid(), Lines = ValidInput(fixture).Lines };

        await FluentActions.Awaiting(() => service.CreateAsync(empty))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Minimal satu baris*");
        await FluentActions.Awaiting(() => service.CreateAsync(inactiveLocation))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Lokasi tidak ditemukan*");
        await FluentActions.Awaiting(() => service.CreateAsync(ValidInput(fixture, 0m)))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Quantity*");
    }

    [Fact]
    public async Task Create_RejectsDuplicateOrInactiveProduct()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var inactive = await fixture.SeedProductAsync("SKU-OFF", isActive: false);
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var duplicate = new StockOutInput
        {
            LocationId = fixture.WarehouseId,
            Lines = new()
            {
                new() { ProductId = fixture.ProductId, Quantity = 1m },
                new() { ProductId = fixture.ProductId, Quantity = 1m }
            }
        };
        var inactiveLine = new StockOutInput
        {
            LocationId = fixture.WarehouseId,
            Lines = new() { new() { ProductId = inactive.Id, Quantity = 1m } }
        };

        await FluentActions.Awaiting(() => service.CreateAsync(duplicate))
            .Should().ThrowAsync<ValidationException>().WithMessage("*duplikat*");
        await FluentActions.Awaiting(() => service.CreateAsync(inactiveLine))
            .Should().ThrowAsync<ValidationException>().WithMessage("*tidak aktif*");
    }

    [Fact]
    public async Task Submit_RejectsInsufficientStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 2m);
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 5m));

        var action = () => service.SubmitAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Stok tidak mencukupi*");
    }

    [Fact]
    public async Task Submit_RejectsMissingOrInactiveParent()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var noParent = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(10), new FakeUserDirectory { ParentId = null });
        var inactiveParent = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20, active: false));
        var header = await noParent.CreateAsync(ValidInput(fixture));

        await FluentActions.Awaiting(() => noParent.SubmitAsync(header.Id))
            .Should().ThrowAsync<ValidationException>().WithMessage("*belum memiliki atasan*");
        await FluentActions.Awaiting(() => inactiveParent.SubmitAsync(header.Id))
            .Should().ThrowAsync<ValidationException>().WithMessage("*tidak aktif*");
    }

    [Fact]
    public async Task Submit_SucceedsWithEnoughStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 4m));

        await service.SubmitAsync(header.Id);

        var saved = await fixture.Db.StockOutHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Submitted);
        saved.ApproverId.Should().Be(20);
        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(10m);
    }

    [Fact]
    public async Task Approve_RechecksStockAndRejectsWhenInsufficient()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 5m));
        await service.SubmitAsync(header.Id);
        var stock = await fixture.Db.Stocks.SingleAsync();
        stock.Quantity = 2m;
        await fixture.Db.SaveChangesAsync();
        var approver = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        var action = () => approver.ApproveAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Stok tidak mencukupi*");
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Approve_DecreasesStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture, 4m));
        await service.SubmitAsync(header.Id);
        var approver = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await approver.ApproveAsync(header.Id);

        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(6m);
        (await fixture.Db.StockMovements.SingleAsync()).Type.Should().Be(MovementType.Out);
    }

    [Fact]
    public async Task Approve_RejectsOwnDocumentOrNonSubmitted()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var creator = ServiceFactory.Staff(10);
        var service = ServiceFactory.StockOut(fixture, creator, ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture));
        var approver = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await FluentActions.Awaiting(() => approver.ApproveAsync(header.Id))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Submitted*");
        await service.SubmitAsync(header.Id);
        var creatorService = ServiceFactory.StockOut(fixture, creator, ServiceFactory.Directory(20));
        await FluentActions.Awaiting(() => creatorService.ApproveAsync(header.Id))
            .Should().ThrowAsync<ValidationException>().WithMessage("*milik sendiri*");
    }

    [Fact]
    public async Task AdminSubmit_AutoApprovesAndDecreasesStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var admin = ServiceFactory.Admin(1);
        var service = ServiceFactory.StockOut(fixture, admin, ServiceFactory.Directory());

        var header = await service.CreateAsync(ValidInput(fixture, 3m));
        await service.SubmitAsync(header.Id);

        var saved = await fixture.Db.StockOutHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Approved);
        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(7m);
    }

    [Fact]
    public async Task Reject_RequiresReason()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var service = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var header = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(header.Id);
        var approver = ServiceFactory.StockOut(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await FluentActions.Awaiting(() => approver.RejectAsync(header.Id, " "))
            .Should().ThrowAsync<ValidationException>();
        await approver.RejectAsync(header.Id, "invalid");

        (await fixture.Db.StockOutHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Rejected);
    }
}
