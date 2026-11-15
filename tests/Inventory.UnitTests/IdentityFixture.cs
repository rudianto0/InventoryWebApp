using Inventory.Infrastructure.Identity;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventory.UnitTests;

internal sealed class IdentityFixture : IAsyncDisposable
{
    public ServiceProvider Provider { get; }
    public UserManager<AppUser> Users { get; }
    public RoleManager<AppRole> Roles { get; }

    public IdentityFixture(TestDatabase fixture)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton(fixture.Db);
        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        Provider = services.BuildServiceProvider();
        Users = Provider.GetRequiredService<UserManager<AppUser>>();
        Roles = Provider.GetRequiredService<RoleManager<AppRole>>();
    }

    public async Task SeedRolesAsync(params string[] names)
    {
        foreach (var name in names)
        {
            if (!await Roles.RoleExistsAsync(name))
                await Roles.CreateAsync(new AppRole { Name = name });
        }
    }

    public async Task<AppUser> CreateUserAsync(string userName, string role, bool isActive = true, string password = "Password1!", int? parentId = null)
    {
        var user = new AppUser
        {
            UserName = userName,
            Email = $"{userName}@test.local",
            FullName = userName,
            IsActive = isActive,
            ParentId = parentId,
            EmailConfirmed = true
        };
        var result = await Users.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        await Users.AddToRoleAsync(user, role);
        return user;
    }

    public ValueTask DisposeAsync()
    {
        Provider.Dispose();
        return ValueTask.CompletedTask;
    }
}