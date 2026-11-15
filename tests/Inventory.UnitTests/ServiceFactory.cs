using Inventory.Application.Services;

namespace Inventory.UnitTests;

internal static class ServiceFactory
{
    private static void Use(TestDatabase fixture, FakeCurrentUser user)
    {
        fixture.CurrentUser.UserId = user.UserId;
        fixture.CurrentUser.UserName = user.UserName;
        fixture.CurrentUser.Roles = user.Roles;
    }

    public static StockInService StockIn(TestDatabase fixture, FakeCurrentUser user, FakeUserDirectory directory)
    {
        Use(fixture, user);
        return new(fixture.Db, user, directory, new StockLedger(fixture.Db, new FakeDocumentNumbers()), new FakeDocumentNumbers());
    }

    public static StockOutService StockOut(TestDatabase fixture, FakeCurrentUser user, FakeUserDirectory directory)
    {
        Use(fixture, user);
        return new(fixture.Db, user, directory, new StockLedger(fixture.Db, new FakeDocumentNumbers()), new FakeDocumentNumbers());
    }

    public static StockTransferService Transfer(TestDatabase fixture, FakeCurrentUser user, FakeUserDirectory directory)
    {
        Use(fixture, user);
        return new(fixture.Db, user, directory, new StockLedger(fixture.Db, new FakeDocumentNumbers()), new FakeDocumentNumbers());
    }

    public static StockAdjustmentService Adjustment(TestDatabase fixture, FakeCurrentUser user, FakeUserDirectory directory)
    {
        Use(fixture, user);
        return new(fixture.Db, user, directory, new StockLedger(fixture.Db, new FakeDocumentNumbers()), new FakeDocumentNumbers());
    }

    public static StockOpnameService Opname(TestDatabase fixture, FakeCurrentUser user, FakeUserDirectory directory)
    {
        Use(fixture, user);
        return new(fixture.Db, user, directory, new StockLedger(fixture.Db, new FakeDocumentNumbers()), new FakeDocumentNumbers());
    }

    public static FakeCurrentUser Staff(int id = 10) => new() { UserId = id, Roles = new[] { "Staff" } };

    public static FakeCurrentUser Admin(int id = 1) => new() { UserId = id, Roles = new[] { "Admin" } };

    public static FakeUserDirectory Directory(int? parentId = 20, bool active = true) =>
        new() { ParentId = parentId, Active = active };
}
