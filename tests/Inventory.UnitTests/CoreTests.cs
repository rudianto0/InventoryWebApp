using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.Services;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class StockLedgerTests
{
    [Fact]
    public async Task ApplyAsync_IncreasesStockAndCreatesMovement()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        await ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.In, ReferenceType.StockIn, 1, 1, 2, 10m, 1, "receive")
        });
        await fixture.Db.SaveChangesAsync();

        var stock = await fixture.Db.Stocks.SingleAsync();
        stock.Quantity.Should().Be(10m);
        var movement = await fixture.Db.StockMovements.SingleAsync();
        movement.Quantity.Should().Be(10m);
        movement.Type.Should().Be(MovementType.In);
    }

    [Fact]
    public async Task ApplyAsync_RejectsNegativeBalance()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());

        var action = () => ledger.ApplyAsync(new[]
        {
            new StockChange(MovementType.Out, ReferenceType.StockOut, 1, 1, 2, -1m, 1, null)
        });

        await action.Should().ThrowAsync<ValidationException>();
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }
}

public class StockTransactionApprovalTests
{
    [Fact]
    public async Task SubmitStockOut_WithInsufficientStock_IsRejected()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var currentUser = new FakeCurrentUser { UserId = 10, Roles = new[] { "Staff" } };
        fixture.Db.Stocks.Add(new Inventory.Domain.Entities.Stock { ProductId = 1, LocationId = 2, Quantity = 2m });
        await fixture.Db.SaveChangesAsync();

        var service = new StockOutService(fixture.Db, currentUser, new FakeUserDirectory(), new StockLedger(fixture.Db, new FakeDocumentNumbers()), new FakeDocumentNumbers());
        var header = await service.CreateAsync(new Inventory.Application.Dtos.StockOutInput
        {
            LocationId = 2,
            Lines = new() { new Inventory.Application.Dtos.DocumentLineInput { ProductId = 1, Quantity = 5m } }
        });

        var action = () => service.SubmitAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Stok tidak mencukupi*");
    }

    [Fact]
    public async Task ApproveStockIn_AsAssignedApprover_UpdatesStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var creator = new FakeCurrentUser { UserId = 10, Roles = new[] { "Staff" } };
        var directory = new FakeUserDirectory { ParentId = 20 };
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());
        var creatorService = new StockInService(fixture.Db, creator, directory, ledger, new FakeDocumentNumbers());

        var header = await creatorService.CreateAsync(new Inventory.Application.Dtos.StockInInput
        {
            SupplierId = 1,
            LocationId = 2,
            Lines = new() { new Inventory.Application.Dtos.DocumentLineInput { ProductId = 1, Quantity = 7m } }
        });
        await creatorService.SubmitAsync(header.Id);

        var approver = new FakeCurrentUser { UserId = 20, Roles = new[] { "SPV" } };
        var approverService = new StockInService(fixture.Db, approver, directory, ledger, new FakeDocumentNumbers());
        await approverService.ApproveAsync(header.Id);

        var stock = await fixture.Db.Stocks.SingleAsync();
        stock.Quantity.Should().Be(7m);
        var savedHeader = await fixture.Db.StockInHeaders.SingleAsync();
        savedHeader.Status.Should().Be(ApprovalStatus.Approved);
        savedHeader.ApprovedById.Should().Be(20);
    }

    [Fact]
    public async Task ApproveStockIn_ByNonApprover_IsRejected()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var creator = new FakeCurrentUser { UserId = 10, Roles = new[] { "Staff" } };
        var directory = new FakeUserDirectory { ParentId = 20 };
        var ledger = new StockLedger(fixture.Db, new FakeDocumentNumbers());
        var creatorService = new StockInService(fixture.Db, creator, directory, ledger, new FakeDocumentNumbers());

        var header = await creatorService.CreateAsync(new Inventory.Application.Dtos.StockInInput
        {
            SupplierId = 1,
            LocationId = 2,
            Lines = new() { new Inventory.Application.Dtos.DocumentLineInput { ProductId = 1, Quantity = 1m } }
        });
        await creatorService.SubmitAsync(header.Id);

        var other = new FakeCurrentUser { UserId = 99, Roles = new[] { "SPV" } };
        var otherService = new StockInService(fixture.Db, other, directory, ledger, new FakeDocumentNumbers());

        var action = () => otherService.ApproveAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("*bukan approver*");
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }
}

public class GuardTests
{
    [Fact]
    public void Positive_ThrowsOnZero() =>
        FluentActions.Invoking(() => Guard.Positive(0m, "Quantity")).Should().Throw<ValidationException>();

    [Fact]
    public void Required_ThrowsOnWhitespace() =>
        FluentActions.Invoking(() => Guard.Required("   ", "Nama")).Should().Throw<ValidationException>();
}
