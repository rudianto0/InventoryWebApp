using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Application.Services;
using Inventory.Contracts;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.IntegrationTests;

public class ProcurementToStockInIntegrationTests
{
    private static StockInService Service(IntegrationDatabase fixture) =>
        new(
            fixture.Db,
            fixture.CurrentUser,
            new FakeUserDirectory(),
            new StockLedger(fixture.Db, new FakeDocumentNumbers()),
            new FakeDocumentNumbers());

    private static void ActAs(IntegrationDatabase fixture, int userId, string role)
    {
        fixture.CurrentUser.UserId = userId;
        fixture.CurrentUser.Roles = new[] { role };
    }

    private static ProcurementOrder ValidOrder(string poNumber = "PO-2026-001") =>
        new(
            poNumber,
            "SUP",
            "WH1",
            new[] { new ProcurementLine("SKU-1", 7, 45000m) });

    [Fact]
    public async Task ProcurementPayload_Valid_CreatesDraft()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, ValidOrder());
        ActAs(fixture, 10, "Staff");

        var header = await Service(fixture).CreateAsync(input);

        header.Status.Should().Be(ApprovalStatus.Draft);
        header.Notes.Should().Contain("PO-2026-001");
        header.Details.Should().ContainSingle(x => x.Quantity == 7);
        (await fixture.Db.StockInHeaders.CountAsync()).Should().Be(1);
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcurementPayload_SubmitAndApprove_UpdatesStockAndAudit()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, ValidOrder());
        ActAs(fixture, 10, "Staff");
        var staffService = Service(fixture);
        var header = await staffService.CreateAsync(input);
        await staffService.SubmitAsync(header.Id);

        ActAs(fixture, 20, "SPV");
        await Service(fixture).ApproveAsync(header.Id, "PO diterima lengkap");

        var saved = await fixture.Db.StockInHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Approved);
        saved.ApproverId.Should().Be(20);

        var stock = await fixture.Db.Stocks.SingleAsync();
        stock.Quantity.Should().Be(7);
        var movement = await fixture.Db.StockMovements.SingleAsync();
        movement.Quantity.Should().Be(7);
        movement.Type.Should().Be(MovementType.In);

        var actions = await fixture.Db.AuditLogs
            .Where(x => x.EntityName == nameof(Inventory.Domain.Entities.StockInHeader))
            .Select(x => x.Action)
            .ToListAsync();
        actions.Should().Contain(new[] { "Create", "Submit", "Approve" });
    }

    [Fact]
    public async Task ProcurementPayload_InactiveSupplier_IsRejected()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        var supplier = await fixture.Db.Suppliers.SingleAsync();
        supplier.IsActive = false;
        await fixture.Db.SaveChangesAsync();
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, ValidOrder());
        ActAs(fixture, 10, "Staff");

        var action = () => Service(fixture).CreateAsync(input);

        await action.Should().ThrowAsync<ValidationException>();
        (await fixture.Db.StockInHeaders.CountAsync()).Should().Be(0);
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcurementPayload_InactiveProduct_IsRejected()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedProductAsync("SKU-OFF", isActive: false);
        var order = new ProcurementOrder(
            "PO-2026-002",
            "SUP",
            "WH1",
            new[] { new ProcurementLine("SKU-OFF", 3, 10000m) });
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, order);
        ActAs(fixture, 10, "Staff");

        var action = () => Service(fixture).CreateAsync(input);

        await action.Should().ThrowAsync<ValidationException>();
        (await fixture.Db.StockInHeaders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcurementPayload_MultiLineOneInvalid_RejectsWholeDocument()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedProductAsync("SKU-OFF", isActive: false);
        var order = new ProcurementOrder(
            "PO-2026-003",
            "SUP",
            "WH1",
            new[]
            {
                new ProcurementLine("SKU-1", 2, 45000m),
                new ProcurementLine("SKU-OFF", 1, 10000m)
            });
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, order);
        ActAs(fixture, 10, "Staff");

        var action = () => Service(fixture).CreateAsync(input);

        await action.Should().ThrowAsync<ValidationException>();
        (await fixture.Db.StockInHeaders.CountAsync()).Should().Be(0);
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcurementPayload_MultiLineAllValid_AppliesAllLines()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedProductAsync("SKU-2");
        var order = new ProcurementOrder(
            "PO-2026-004",
            "SUP",
            "WH1",
            new[]
            {
                new ProcurementLine("SKU-1", 7, 45000m),
                new ProcurementLine("SKU-2", 3, 20000m)
            });
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, order);
        ActAs(fixture, 10, "Staff");
        var service = Service(fixture);
        var header = await service.CreateAsync(input);
        await service.SubmitAsync(header.Id);

        ActAs(fixture, 20, "SPV");
        await Service(fixture).ApproveAsync(header.Id);

        (await fixture.Db.Stocks.CountAsync()).Should().Be(2);
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(2);
        (await fixture.Db.Stocks.SumAsync(x => x.Quantity)).Should().Be(10);
    }

    [Fact]
    public async Task ExternalApproval_Reject_LeavesStockUnchanged()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, ValidOrder());
        ActAs(fixture, 10, "Staff");
        var service = Service(fixture);
        var header = await service.CreateAsync(input);
        await service.SubmitAsync(header.Id);

        ActAs(fixture, 20, "SPV");
        await Service(fixture).RejectAsync(header.Id, "Harga tidak sesuai kontrak");

        var saved = await fixture.Db.StockInHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Rejected);
        saved.RejectionReason.Should().Be("Harga tidak sesuai kontrak");
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ExternalApproval_UnauthorizedApprover_IsRejected()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, ValidOrder());
        ActAs(fixture, 10, "Staff");
        var service = Service(fixture);
        var header = await service.CreateAsync(input);
        await service.SubmitAsync(header.Id);

        ActAs(fixture, 99, "Staff");
        var action = () => Service(fixture).ApproveAsync(header.Id);

        await action.Should().ThrowAsync<ValidationException>();
        (await fixture.Db.StockInHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Submitted);
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcurementPayload_Retry_CreatesSeparateDrafts_CurrentBehavior()
    {
        await using var fixture = new IntegrationDatabase();
        await fixture.SeedBasicsAsync();
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, ValidOrder());
        ActAs(fixture, 10, "Staff");
        var service = Service(fixture);

        await service.CreateAsync(input);
        await service.CreateAsync(input);

        // Catatan: belum ada ExternalReference/idempotency key.
        (await fixture.Db.StockInHeaders.CountAsync()).Should().Be(2);
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }
}
