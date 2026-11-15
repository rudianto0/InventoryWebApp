using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Contracts;
using Inventory.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace Inventory.UnitTests;

public class UserServiceTests
{
    private static UserInput Input(string userName, string role, string? password = "Password1!", int? parentId = null) => new()
    {
        UserName = userName,
        Email = $"{userName}@test.local",
        FullName = userName,
        RoleName = role,
        ParentId = parentId,
        Password = password,
        IsActive = true
    };

    private static async Task<(TestDatabase Db, IdentityFixture Identity, UserService Service)> SetupAsync()
    {
        var db = new TestDatabase();
        var identity = new IdentityFixture(db);
        await identity.SeedRolesAsync("Admin", "SPV", "Manager", "Staff");
        return (db, identity, new UserService(identity.Users, identity.Roles, db.CurrentUser));
    }

    [Fact]
    public async Task Create_PersistsUserAndRole()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;

        var id = await service.CreateAsync(Input("staff1", "Staff"));

        var user = await identity.Users.FindByIdAsync(id.ToString());
        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        (await identity.Users.GetRolesAsync(user)).Should().ContainSingle("Staff");
    }

    [Theory]
    [InlineData("", "Password1!", "*Username*")]
    [InlineData("user", "short", "*Password*")]
    public async Task Create_RejectsInvalidRequiredFieldsOrShortPassword(string userName, string password, string message)
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;

        var action = () => service.CreateAsync(Input(userName, "Staff", password));

        await action.Should().ThrowAsync<ValidationException>().WithMessage(message);
    }

    [Fact]
    public async Task Create_RejectsInvalidRole()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;

        var action = () => service.CreateAsync(Input("staff1", "Unknown"));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Role tidak valid*");
    }

    [Fact]
    public async Task Create_RejectsDuplicateUsernameAndEmail()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        await service.CreateAsync(Input("staff1", "Staff"));

        var duplicateName = Input("staff1", "Staff");
        duplicateName = new UserInput
        {
            UserName = "staff1", Email = "other@test.local", FullName = "Other",
            RoleName = "Staff", Password = "Password1!"
        };
        var duplicateEmail = new UserInput
        {
            UserName = "staff2", Email = "staff1@test.local", FullName = "Other",
            RoleName = "Staff", Password = "Password1!"
        };

        await FluentActions.Awaiting(() => service.CreateAsync(duplicateName))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Username sudah digunakan*");
        await FluentActions.Awaiting(() => service.CreateAsync(duplicateEmail))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Email sudah digunakan*");
    }

    [Fact]
    public async Task Create_ValidatesParent()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        var spv = await identity.CreateUserAsync("spv1", "SPV");
        var inactiveSpv = await identity.CreateUserAsync("spv-off", "SPV", isActive: false);
        var staffParent = await identity.CreateUserAsync("staff-parent", "Staff");

        await FluentActions.Awaiting(() => service.CreateAsync(Input("u1", "Staff", parentId: 99999)))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Parent tidak ditemukan*");
        await FluentActions.Awaiting(() => service.CreateAsync(Input("u2", "Staff", parentId: inactiveSpv.Id)))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Parent tidak aktif*");
        await FluentActions.Awaiting(() => service.CreateAsync(Input("u3", "Staff", parentId: staffParent.Id)))
            .Should().ThrowAsync<ValidationException>().WithMessage("*SPV atau Manager*");

        await FluentActions.Awaiting(() => service.CreateAsync(Input("u4", "Staff", parentId: spv.Id)))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Update_RejectsSelfAsParent()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        var user = await identity.CreateUserAsync("staff1", "Staff");

        var action = () => service.UpdateAsync(user.Id, new UserInput
        {
            UserName = "staff1",
            Email = "staff1@test.local",
            FullName = "staff1",
            RoleName = "Staff",
            ParentId = user.Id,
            IsActive = true
        });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*diri sendiri*");
    }

    [Fact]
    public async Task Update_RejectsChangingLastActiveAdmin()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        var admin = await identity.CreateUserAsync("admin1", "Admin");

        var action = () => service.UpdateAsync(admin.Id, new UserInput
        {
            UserName = "admin1",
            Email = "admin1@test.local",
            FullName = "admin1",
            RoleName = "Staff",
            IsActive = true
        });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Admin aktif terakhir*");
    }

    [Fact]
    public async Task Deactivate_RejectsSelfAndLastAdmin()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        var admin = await identity.CreateUserAsync("admin1", "Admin");
        db.CurrentUser.UserId = admin.Id;

        await FluentActions.Awaiting(() => service.DeactivateAsync(admin.Id))
            .Should().ThrowAsync<ValidationException>().WithMessage("*akun sendiri*");

        db.CurrentUser.UserId = 999;
        await FluentActions.Awaiting(() => service.DeactivateAsync(admin.Id))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Admin aktif terakhir*");
    }

    [Fact]
    public async Task ResetPassword_ValidatesLengthAndChangesPassword()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        var user = await identity.CreateUserAsync("staff1", "Staff");

        await FluentActions.Awaiting(() => service.ResetPasswordAsync(user.Id, "short"))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Password*");

        await service.ResetPasswordAsync(user.Id, "NewPassword1!");
        (await identity.Users.CheckPasswordAsync(user, "NewPassword1!")).Should().BeTrue();
    }

    [Fact]
    public async Task List_AppliesSearchRoleAndActiveFilter()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        await identity.CreateUserAsync("spv1", "SPV");
        await identity.CreateUserAsync("staff1", "Staff", isActive: false);

        var bySearch = await service.ListAsync(new UserFilter { Search = "spv" });
        var byRole = await service.ListAsync(new UserFilter { RoleName = "Staff" });
        var active = await service.ListAsync(new UserFilter { IsActive = true });
        var clamped = await service.ListAsync(new UserFilter { Page = 0, PageSize = 500 });

        bySearch.TotalCount.Should().Be(1);
        byRole.TotalCount.Should().Be(1);
        active.TotalCount.Should().Be(1);
        clamped.Page.Should().Be(1);
        clamped.PageSize.Should().Be(200);
    }

    [Fact]
    public async Task GetParentCandidates_ReturnsOnlyActiveSupervisors()
    {
        var (db, identity, service) = await SetupAsync();
        await using var _ = db;
        await using var __ = identity;
        await identity.CreateUserAsync("spv1", "SPV");
        await identity.CreateUserAsync("mgr1", "Manager");
        await identity.CreateUserAsync("spv-off", "SPV", isActive: false);
        await identity.CreateUserAsync("staff1", "Staff");

        var candidates = await service.GetParentCandidatesAsync();

        candidates.Should().HaveCount(2);
        candidates.Select(x => x.Text).Should().BeInAscendingOrder();
    }
}
