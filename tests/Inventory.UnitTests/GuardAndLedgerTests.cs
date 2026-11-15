using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class GuardValidationTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(-0.0001)]
    public void Positive_ThrowsOnNonPositive(double value) =>
        FluentActions.Invoking(() => Guard.Positive((decimal)value, "Qty")).Should().Throw<ValidationException>();

    [Fact]
    public void Positive_AllowsPositiveValue() =>
        FluentActions.Invoking(() => Guard.Positive(0.0001m, "Qty")).Should().NotThrow();

    [Fact]
    public void NotNegative_ThrowsOnNegative() =>
        FluentActions.Invoking(() => Guard.NotNegative(-0.01m, "Qty")).Should().Throw<ValidationException>();

    [Fact]
    public void NotNegative_AllowsZero() =>
        FluentActions.Invoking(() => Guard.NotNegative(0m, "Qty")).Should().NotThrow();

    [Theory]
    [InlineData("   ", null)]
    [InlineData(" x ", "x")]
    [InlineData(null, null)]
    public void TrimOrNull_ReturnsExpected(string? input, string? expected) =>
        Guard.TrimOrNull(input).Should().Be(expected);
}

public class StockLedgerExtraTests
{
    [Fact]
    public async Task ApplyAsync_SkipsZeroQuantity()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        await ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.Adjustment, ReferenceType.StockAdjustment, Guid.NewGuid(),
                fixture.ProductId, fixture.WarehouseId, 0m, 1, null)
        });
        await fixture.Db.SaveChangesAsync();

        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ApplyAsync_DecreasesStockWithinBalance()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        await ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.Out, ReferenceType.StockOut, Guid.NewGuid(),
                fixture.ProductId, fixture.WarehouseId, -4m, 1, null)
        });
        await fixture.Db.SaveChangesAsync();

        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(6m);
        (await fixture.Db.StockMovements.SingleAsync()).Quantity.Should().Be(-4m);
    }

    [Fact]
    public async Task ApplyAsync_AccumulatesChangesForSameKey()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        await ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.In, ReferenceType.StockIn, Guid.NewGuid(), fixture.ProductId, fixture.WarehouseId, 5m, 1, null),
            new StockChange(MovementType.In, ReferenceType.StockIn, Guid.NewGuid(), fixture.ProductId, fixture.WarehouseId, 3m, 1, null)
        });
        await fixture.Db.SaveChangesAsync();

        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(8m);
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task ApplyAsync_FillsMovementFieldsAndVersion()
    {
        await using var fixture = new TestDatabase(new FakeCurrentUser { UserId = 7 });
        await fixture.SeedBasicsAsync();
        var referenceId = Guid.NewGuid();
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        await ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.In, ReferenceType.StockIn, referenceId,
                fixture.ProductId, fixture.WarehouseId, 10m, 7, "receive")
        });
        await fixture.Db.SaveChangesAsync();

        var movement = await fixture.Db.StockMovements.SingleAsync();
        movement.MovementNo.Should().StartWith("MOV-");
        movement.Type.Should().Be(MovementType.In);
        movement.ReferenceType.Should().Be(ReferenceType.StockIn);
        movement.ReferenceId.Should().Be(referenceId);
        movement.CreatedById.Should().Be(7);
        movement.Notes.Should().Be("receive");

        var stock = await fixture.Db.Stocks.SingleAsync();
        stock.Version.Should().Be(1);
        stock.UpdatedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task ApplyAsync_InsufficientMessageContainsProductAndLocation()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        var action = () => ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.Out, ReferenceType.StockOut, Guid.NewGuid(),
                fixture.ProductId, fixture.WarehouseId, -2m, 1, null)
        });

        var exception = await action.Should().ThrowAsync<ValidationException>();
        exception.Which.Message.Should().Contain("Product 1").And.Contain("WH1");
    }

    [Fact]
    public async Task ApplyAsync_TransferMovesStockBetweenLocations()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 10m);
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        await ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.TransferOut, ReferenceType.StockTransfer, Guid.NewGuid(), fixture.ProductId, fixture.WarehouseId, -4m, 1, null),
            new StockChange(MovementType.TransferIn, ReferenceType.StockTransfer, Guid.NewGuid(), fixture.ProductId, fixture.Warehouse2Id, 4m, 1, null)
        });
        await fixture.Db.SaveChangesAsync();

        (await fixture.Db.Stocks.SingleAsync(x => x.LocationId == fixture.WarehouseId)).Quantity.Should().Be(6m);
        (await fixture.Db.Stocks.SingleAsync(x => x.LocationId == fixture.Warehouse2Id)).Quantity.Should().Be(4m);
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task ApplyAsync_AdjustmentCanBringStockToZero()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 5m);
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        await ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.Adjustment, ReferenceType.StockAdjustment, Guid.NewGuid(),
                fixture.ProductId, fixture.WarehouseId, -5m, 1, null)
        });
        await fixture.Db.SaveChangesAsync();

        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(0m);
    }
}

public class QueryFilterTests
{
    [Fact]
    public async Task SoftDeletedProduct_HidesStockAndMovements()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 5m);
        var product = await fixture.Db.Products.SingleAsync(x => x.Id == fixture.ProductId);
        product.IsDeleted = true;
        product.IsActive = false;
        await fixture.Db.SaveChangesAsync();

        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
        (await fixture.Db.Stocks.IgnoreQueryFilters().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SoftDeletedLocation_HidesStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 5m);
        var location = await fixture.Db.Locations.SingleAsync(x => x.Id == fixture.WarehouseId);
        location.IsDeleted = true;
        location.IsActive = false;
        await fixture.Db.SaveChangesAsync();

        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
        (await fixture.Db.Stocks.IgnoreQueryFilters().CountAsync()).Should().Be(1);
    }
}
