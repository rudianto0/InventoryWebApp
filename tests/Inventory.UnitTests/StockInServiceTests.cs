using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class StockInServiceTests
{
    private static StockInInput ValidInput(TestDatabase fixture, int quantity = 5) => new()
    {
        SupplierId = fixture.SupplierId,
        LocationId = fixture.WarehouseId,
        Date = new DateTime(2026, 11, 15),
        Lines = new List<DocumentLineInput>
        {
            new() { ProductId = fixture.ProductId, Quantity = quantity, UnitCost = 100m }
        }
    };

    [Fact]
    public async Task Create_PersistsDraftWithDetails()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff();
        var service = ServiceFactory.StockIn(fixture, user, ServiceFactory.Directory());

        var header = await service.CreateAsync(ValidInput(fixture));

        header.Status.Should().Be(ApprovalStatus.Draft);
        header.DocNo.Should().NotBeNullOrWhiteSpace();
        header.CreatedById.Should().Be(10);
        header.Details.Should().ContainSingle(x => x.Quantity == 5);
    }

    [Fact]
    public async Task Create_RejectsEmptyLines()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var input = ValidInput(fixture);
        input = new StockInInput { SupplierId = input.SupplierId, LocationId = input.LocationId, Lines = new() };

        var action = () => service.CreateAsync(input);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Minimal satu baris*");
    }

    [Fact]
    public async Task Create_RejectsInactiveSupplier()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var supplier = await fixture.Db.Suppliers.SingleAsync(x => x.Id == fixture.SupplierId);
        supplier.IsActive = false;
        await fixture.Db.SaveChangesAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var action = () => service.CreateAsync(ValidInput(fixture));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Supplier tidak aktif*");
    }

    [Fact]
    public async Task Create_RejectsInactiveLocation()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var location = await fixture.Db.Locations.SingleAsync(x => x.Id == fixture.WarehouseId);
        location.IsActive = false;
        await fixture.Db.SaveChangesAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var action = () => service.CreateAsync(ValidInput(fixture));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Lokasi tidak aktif*");
    }

    [Fact]
    public async Task Create_RejectsDuplicateProduct()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var input = new StockInInput
        {
            SupplierId = fixture.SupplierId,
            LocationId = fixture.WarehouseId,
            Lines = new()
            {
                new() { ProductId = fixture.ProductId, Quantity = 1 },
                new() { ProductId = fixture.ProductId, Quantity = 2 }
            }
        };

        var action = () => service.CreateAsync(input);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*duplikat*");
    }

    [Fact]
    public async Task Create_RejectsNonPositiveQuantity()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());

        var action = () => service.CreateAsync(ValidInput(fixture, 0));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Quantity*");
    }

    [Fact]
    public async Task Create_RejectsNegativeUnitCost()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(), ServiceFactory.Directory());
        var input = new StockInInput
        {
            SupplierId = fixture.SupplierId,
            LocationId = fixture.WarehouseId,
            Lines = new() { new() { ProductId = fixture.ProductId, Quantity = 1, UnitCost = -1m } }
        };

        var action = () => service.CreateAsync(input);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Unit cost*");
    }

    [Fact]
    public async Task Update_RejectsNonOwner()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var creator = ServiceFactory.Staff(10);
        var created = await ServiceFactory.StockIn(fixture, creator, ServiceFactory.Directory()).CreateAsync(ValidInput(fixture));
        var other = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(99), ServiceFactory.Directory());

        var action = () => other.UpdateAsync(created.Id, ValidInput(fixture, 8));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*tidak berhak*");
    }

    [Fact]
    public async Task Update_RejectsNonEditableStatus()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var directory = ServiceFactory.Directory();
        var service = ServiceFactory.StockIn(fixture, user, directory);
        var created = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(created.Id);

        var action = () => service.UpdateAsync(created.Id, ValidInput(fixture, 8));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Draft atau Rejected*");
    }

    [Fact]
    public async Task Delete_RejectsNonDraft()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var directory = ServiceFactory.Directory();
        var service = ServiceFactory.StockIn(fixture, user, directory);
        var created = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(created.Id);

        var action = () => service.DeleteAsync(created.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Draft*");
    }

    [Fact]
    public async Task Submit_RejectsWhenNoParent()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var directory = new FakeUserDirectory { ParentId = null };
        var service = ServiceFactory.StockIn(fixture, user, directory);
        var created = await service.CreateAsync(ValidInput(fixture));

        var action = () => service.SubmitAsync(created.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*belum memiliki atasan*");
    }

    [Fact]
    public async Task Submit_RejectsSelfApprover()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var directory = ServiceFactory.Directory();
        directory.Parents[10] = 10;
        var service = ServiceFactory.StockIn(fixture, user, directory);
        var created = await service.CreateAsync(ValidInput(fixture));

        var action = () => service.SubmitAsync(created.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*approver dokumen sendiri*");
    }

    [Fact]
    public async Task Submit_RejectsInactiveParent()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var directory = ServiceFactory.Directory(20, active: false);
        var service = ServiceFactory.StockIn(fixture, user, directory);
        var created = await service.CreateAsync(ValidInput(fixture));

        var action = () => service.SubmitAsync(created.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*tidak aktif*");
    }

    [Fact]
    public async Task Submit_SetsSubmittedAndApprover()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        var created = await service.CreateAsync(ValidInput(fixture));

        await service.SubmitAsync(created.Id);

        var saved = await fixture.Db.StockInHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Submitted);
        saved.ApproverId.Should().Be(20);
        saved.SubmittedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Approve_RejectsOwnDocument()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var service = ServiceFactory.StockIn(fixture, user, ServiceFactory.Directory(20));
        var created = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(created.Id);

        var action = () => service.ApproveAsync(created.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*milik sendiri*");
    }

    [Fact]
    public async Task Approve_RejectsNonSubmittedDocument()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var service = ServiceFactory.StockIn(fixture, user, ServiceFactory.Directory(20));
        var created = await service.CreateAsync(ValidInput(fixture));
        var approverService = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        var action = () => approverService.ApproveAsync(created.Id);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Submitted*");
    }

    [Fact]
    public async Task AdminSubmit_AutoApprovesAndAppliesStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var admin = ServiceFactory.Admin(1);
        var service = ServiceFactory.StockIn(fixture, admin, ServiceFactory.Directory());

        var created = await service.CreateAsync(ValidInput(fixture, 7));
        await service.SubmitAsync(created.Id);

        var saved = await fixture.Db.StockInHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Approved);
        saved.ApprovedById.Should().Be(1);
        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(7);
    }

    [Fact]
    public async Task Reject_RequiresReasonAndUpdatesStatus()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var user = ServiceFactory.Staff(10);
        var service = ServiceFactory.StockIn(fixture, user, ServiceFactory.Directory(20));
        var created = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(created.Id);
        var approver = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(20), ServiceFactory.Directory(20));

        await FluentActions.Awaiting(() => approver.RejectAsync(created.Id, "  "))
            .Should().ThrowAsync<ValidationException>();
        await approver.RejectAsync(created.Id, "  not valid  ");

        var saved = await fixture.Db.StockInHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Rejected);
        saved.RejectionReason.Should().Be("not valid");
        saved.ApprovedAt.Should().BeNull();
    }

    [Fact]
    public async Task List_FiltersByStatusAndDate()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = ServiceFactory.StockIn(fixture, ServiceFactory.Staff(10), ServiceFactory.Directory(20));
        await service.CreateAsync(ValidInput(fixture));
        var submitted = await service.CreateAsync(ValidInput(fixture));
        await service.SubmitAsync(submitted.Id);

        var draft = await service.ListAsync(new TransactionFilter { Status = ApprovalStatus.Draft });
        var inRange = await service.ListAsync(new TransactionFilter { From = new DateTime(2026, 11, 15), To = new DateTime(2026, 11, 15) });
        var outOfRange = await service.ListAsync(new TransactionFilter { From = new DateTime(2026, 11, 16) });

        draft.TotalCount.Should().Be(1);
        inRange.TotalCount.Should().Be(2);
        outOfRange.TotalCount.Should().Be(0);
    }
}