using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Contracts;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class SupplierServiceTests
{
    [Fact]
    public async Task Create_NormalizesCodeAndTrimsFields()
    {
        await using var fixture = new TestDatabase();
        var service = new SupplierService(fixture.Db);

        var supplier = await service.CreateAsync(new SupplierInput
        {
            Code = " sup-2 ",
            Name = " Supplier 2 ",
            ContactPerson = "  John ",
            Email = " a@b.com "
        });

        supplier.Code.Should().Be("SUP-2");
        supplier.Name.Should().Be("Supplier 2");
        supplier.ContactPerson.Should().Be("John");
        supplier.Email.Should().Be("a@b.com");
    }

    [Theory]
    [InlineData("", "Name")]
    [InlineData("SUP", "")]
    public async Task Create_RequiresCodeAndName(string code, string name)
    {
        await using var fixture = new TestDatabase();
        var service = new SupplierService(fixture.Db);

        var action = () => service.CreateAsync(new SupplierInput { Code = code, Name = name });

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Create_RejectsDuplicateCode()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new SupplierService(fixture.Db);

        var action = () => service.CreateAsync(new SupplierInput { Code = "sup", Name = "Other" });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*sudah digunakan*");
    }

    [Fact]
    public async Task Create_RejectsInvalidEmail()
    {
        await using var fixture = new TestDatabase();
        var service = new SupplierService(fixture.Db);

        var action = () => service.CreateAsync(new SupplierInput { Code = "SUP-2", Name = "S2", Email = "not-an-email" });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*email*");
    }

    [Fact]
    public async Task Update_RejectsDuplicateCodeAndMissingEntity()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new SupplierService(fixture.Db);
        var other = await service.CreateAsync(new SupplierInput { Code = "SUP-2", Name = "S2" });

        await FluentActions.Awaiting(() => service.UpdateAsync(other.Id, new SupplierInput { Code = "sup", Name = "S2" }))
            .Should().ThrowAsync<ValidationException>();
        await FluentActions.Awaiting(() => service.UpdateAsync(Guid.NewGuid(), new SupplierInput { Code = "SUP-9", Name = "S9" }))
            .Should().ThrowAsync<ValidationException>().WithMessage("*tidak ditemukan*");
    }

    [Fact]
    public async Task Deactivate_SetsInactive()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new SupplierService(fixture.Db);

        await service.DeactivateAsync(fixture.SupplierId);

        (await fixture.Db.Suppliers.SingleAsync(x => x.Id == fixture.SupplierId)).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_RejectsWhenUsedByStockIn()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        fixture.Db.StockInHeaders.Add(new StockInHeader
        {
            DocNo = "IN-1",
            SupplierId = fixture.SupplierId,
            LocationId = fixture.WarehouseId
        });
        await fixture.Db.SaveChangesAsync();
        var service = new SupplierService(fixture.Db);

        var action = () => service.DeleteAsync(fixture.SupplierId);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*tidak dapat dihapus*");
    }

    [Fact]
    public async Task Delete_SoftDeletesWhenUnused()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new SupplierService(fixture.Db);
        var supplier = await service.CreateAsync(new SupplierInput { Code = "SUP-2", Name = "S2" });

        await service.DeleteAsync(supplier.Id);

        (await fixture.Db.Suppliers.AnyAsync(x => x.Id == supplier.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task List_AppliesSearchAndActiveFilter()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new SupplierService(fixture.Db);
        await service.CreateAsync(new SupplierInput { Code = "OFF", Name = "Off", IsActive = false });

        var inactive = await service.ListAsync(activeOnly: false);
        var active = await service.ListAsync("off", activeOnly: true);

        inactive.Items.Should().HaveCount(2);
        active.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Options_ReturnOnlyActive()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new SupplierService(fixture.Db);
        await service.CreateAsync(new SupplierInput { Code = "OFF", Name = "Off", IsActive = false });

        var options = await service.GetOptionsAsync();

        options.Should().ContainSingle(x => x.Text.StartsWith("SUP"));
    }
}

public class LocationServiceTests
{
    [Fact]
    public async Task Create_NormalizesCodes()
    {
        await using var fixture = new TestDatabase();
        var service = new LocationService(fixture.Db);

        var location = await service.CreateAsync(new LocationInput
        {
            Code = " wh9 ",
            Name = " WH9 ",
            Type = LocationType.Warehouse,
            BranchCode = " br1 "
        });

        location.Code.Should().Be("WH9");
        location.Name.Should().Be("WH9");
        location.BranchCode.Should().Be("BR1");
    }

    [Theory]
    [InlineData("", "Name")]
    [InlineData("WH", "")]
    public async Task Create_RequiresCodeAndName(string code, string name)
    {
        await using var fixture = new TestDatabase();
        var service = new LocationService(fixture.Db);

        var action = () => service.CreateAsync(new LocationInput { Code = code, Name = name, Type = LocationType.Warehouse });

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Create_RejectsDuplicateCode()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new LocationService(fixture.Db);

        var action = () => service.CreateAsync(new LocationInput { Code = "wh1", Name = "Other", Type = LocationType.Warehouse });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*sudah digunakan*");
    }

    [Fact]
    public async Task Update_RejectsSelfAsParent()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new LocationService(fixture.Db);

        var action = () => service.UpdateAsync(fixture.WarehouseId, new LocationInput
        {
            Code = "WH1",
            Name = "WH1",
            Type = LocationType.Warehouse,
            ParentLocationId = fixture.WarehouseId
        });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*parent dirinya sendiri*");
    }

    [Fact]
    public async Task Create_RejectsMissingInactiveOrWrongParent()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var inactive = await fixture.SeedLocationAsync("WH-OFF", LocationType.Warehouse, isActive: false);
        var service = new LocationService(fixture.Db);

        var missing = new LocationInput { Code = "WH-A", Name = "A", Type = LocationType.Warehouse, ParentLocationId = Guid.NewGuid() };
        var inactiveParent = new LocationInput { Code = "WH-B", Name = "B", Type = LocationType.Warehouse, ParentLocationId = inactive.Id };
        var wrongType = new LocationInput { Code = "WH-C", Name = "C", Type = LocationType.Warehouse, ParentLocationId = fixture.WarehouseId };

        await FluentActions.Awaiting(() => service.CreateAsync(missing))
            .Should().ThrowAsync<ValidationException>().WithMessage("*tidak ditemukan*");
        await FluentActions.Awaiting(() => service.CreateAsync(inactiveParent))
            .Should().ThrowAsync<ValidationException>().WithMessage("*tidak aktif*");
        await FluentActions.Awaiting(() => service.CreateAsync(wrongType))
            .Should().ThrowAsync<ValidationException>().WithMessage("*harus bertipe Branch*");
    }

    [Fact]
    public async Task Create_AllowsBranchChildUnderNonBranchParent()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new LocationService(fixture.Db);

        var location = await service.CreateAsync(new LocationInput
        {
            Code = "BR2",
            Name = "BR2",
            Type = LocationType.Branch,
            ParentLocationId = fixture.WarehouseId
        });

        location.ParentLocationId.Should().Be(fixture.WarehouseId);
    }

    [Fact]
    public async Task Update_RejectsHierarchyCycle()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new LocationService(fixture.Db);
        var a = await service.CreateAsync(new LocationInput { Code = "A", Name = "A", Type = LocationType.Branch });
        var b = await service.CreateAsync(new LocationInput { Code = "B", Name = "B", Type = LocationType.Branch, ParentLocationId = a.Id });

        var action = () => service.UpdateAsync(a.Id, new LocationInput
        {
            Code = "A",
            Name = "A",
            Type = LocationType.Branch,
            ParentLocationId = b.Id
        });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*siklus*");
    }

    [Fact]
    public async Task Deactivate_RejectsActiveChildrenOrStock()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new LocationService(fixture.Db);
        var child = await fixture.SeedLocationAsync("WH-CHILD", LocationType.Warehouse, parentId: fixture.WarehouseId);
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 1);

        await FluentActions.Awaiting(() => service.DeactivateAsync(fixture.WarehouseId))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*sub-lokasi*");
        await FluentActions.Awaiting(() => service.DeactivateAsync(fixture.Warehouse2Id))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Deactivate_RejectsWhenStockBalanceExists()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 1);
        var service = new LocationService(fixture.Db);

        var action = () => service.DeactivateAsync(fixture.WarehouseId);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*saldo stok*");
    }

    [Fact]
    public async Task Delete_RejectsWhenUsedOrHasChild()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 1);
        var service = new LocationService(fixture.Db);

        var action = () => service.DeleteAsync(fixture.WarehouseId);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*tidak dapat dihapus*");
    }

    [Fact]
    public async Task Delete_SoftDeletesWhenUnused()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new LocationService(fixture.Db);
        var location = await fixture.SeedLocationAsync("WH-UNUSED", LocationType.Warehouse);

        await service.DeleteAsync(location.Id);

        (await fixture.Db.Locations.AnyAsync(x => x.Id == location.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task List_AppliesTypeAndActiveFilter()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedLocationAsync("WH-OFF", LocationType.Warehouse, isActive: false);
        var service = new LocationService(fixture.Db);

        var warehouses = await service.ListAsync(LocationType.Warehouse, activeOnly: true);

        warehouses.Items.Should().OnlyContain(x => x.Type == LocationType.Warehouse && x.IsActive);
    }

    [Fact]
    public async Task Options_RespectActiveOnly()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedLocationAsync("WH-OFF", LocationType.Warehouse, isActive: false);
        var service = new LocationService(fixture.Db);

        var active = await service.GetOptionsAsync(activeOnly: true);
        var all = await service.GetOptionsAsync(activeOnly: false);

        active.Should().HaveCount(3);
        all.Should().HaveCount(4);
    }
}