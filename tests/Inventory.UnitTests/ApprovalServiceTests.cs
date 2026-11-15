using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;

namespace Inventory.UnitTests;

public class ApprovalServiceTests
{
    private static async Task SeedPendingAsync(TestDatabase fixture)
    {
        fixture.Db.StockInHeaders.Add(new StockInHeader
        {
            DocNo = "IN-1", SupplierId = fixture.SupplierId, LocationId = fixture.WarehouseId,
            Status = ApprovalStatus.Submitted, ApproverId = 20, CreatedById = 10,
            SubmittedAt = new DateTime(2026, 11, 15, 8, 0, 0, DateTimeKind.Utc)
        });
        fixture.Db.StockOutHeaders.Add(new StockOutHeader
        {
            DocNo = "OUT-1", LocationId = fixture.WarehouseId, Status = ApprovalStatus.Submitted,
            ApproverId = 20, CreatedById = 11, SubmittedAt = new DateTime(2026, 11, 15, 9, 0, 0, DateTimeKind.Utc)
        });
        fixture.Db.StockTransferHeaders.Add(new StockTransferHeader
        {
            DocNo = "TRF-1", FromLocationId = fixture.WarehouseId, ToLocationId = fixture.Warehouse2Id,
            Status = ApprovalStatus.Submitted, ApproverId = 30, CreatedById = 12,
            SubmittedAt = new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc)
        });
        fixture.Db.StockAdjustmentHeaders.Add(new StockAdjustmentHeader
        {
            DocNo = "ADJ-1", LocationId = fixture.WarehouseId, Reason = "x", Status = ApprovalStatus.Submitted,
            ApproverId = 20, CreatedById = 13, SubmittedAt = new DateTime(2026, 11, 15, 11, 0, 0, DateTimeKind.Utc)
        });
        fixture.Db.StockOpnameHeaders.Add(new StockOpnameHeader
        {
            DocNo = "OPN-1", LocationId = fixture.WarehouseId, Status = ApprovalStatus.Submitted,
            ApproverId = 20, CreatedById = 14, SubmittedAt = new DateTime(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc)
        });
        fixture.Db.StockInHeaders.Add(new StockInHeader
        {
            DocNo = "IN-DRAFT", SupplierId = fixture.SupplierId, LocationId = fixture.WarehouseId,
            Status = ApprovalStatus.Draft, ApproverId = 20, CreatedById = 10
        });
        await fixture.Db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPending_ReturnsOnlySubmittedAssignedToUser()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await SeedPendingAsync(fixture);
        var user = new FakeCurrentUser { UserId = 20, Roles = Array.Empty<string>() };
        var service = new ApprovalService(fixture.Db, user, new FakeUserDirectory());

        var items = await service.GetPendingAsync();

        items.Should().HaveCount(4);
        items.Should().OnlyContain(x => x.Type != "StockTransfer");
    }

    [Fact]
    public async Task GetPending_AdminSeesAllSubmitted()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await SeedPendingAsync(fixture);
        var user = new FakeCurrentUser { UserId = 99, Roles = new[] { "Admin" } };
        var service = new ApprovalService(fixture.Db, user, new FakeUserDirectory());

        var items = await service.GetPendingAsync();

        items.Should().HaveCount(5);
        items.Select(x => x.Type).Should().BeEquivalentTo(new[] { "StockIn", "StockOut", "StockTransfer", "StockAdjustment", "StockOpname" });
    }

    [Fact]
    public async Task GetPending_OrdersBySubmittedAtAndResolvesRequester()
    {
        await using var fixture = new TestDatabase(new FakeCurrentUser { UserId = 10 });
        await fixture.SeedBasicsAsync();
        await SeedPendingAsync(fixture);
        var user = new FakeCurrentUser { UserId = 20, Roles = Array.Empty<string>() };
        var service = new ApprovalService(fixture.Db, user, new FakeUserDirectory());

        var items = await service.GetPendingAsync();

        items.Select(x => x.SubmittedAt).Should().BeInAscendingOrder();
        items[0].RequesterName.Should().Be("User 10");
        items.Should().OnlyContain(x => x.LocationName != string.Empty);
    }

    [Fact]
    public async Task GetPending_ThrowsWithoutUserSession()
    {
        await using var fixture = new TestDatabase();
        var user = new FakeCurrentUser { UserId = null };
        var service = new ApprovalService(fixture.Db, user, new FakeUserDirectory());

        var action = () => service.GetPendingAsync();

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Sesi pengguna*");
    }
}
