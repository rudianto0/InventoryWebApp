using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Identity;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Persistence;

public static class DatabaseBootstrapper
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseBootstrapper");
        var db = provider.GetRequiredService<AppDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = provider.GetRequiredService<UserManager<AppUser>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "SPV", "Staff", "Manager", "Auditor" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new AppRole { Name = role, Description = role });
        }

        var admin = await EnsureUserAsync(userManager, "admin", "admin@inventory.local", "System Administrator", "Admin", null);
        var spv = await EnsureUserAsync(userManager, "spv", "spv@inventory.local", "Supervisor", "SPV", admin?.Id);
        await EnsureUserAsync(userManager, "staff", "staff@inventory.local", "Warehouse Staff", "Staff", spv?.Id);
        await EnsureUserAsync(userManager, "manager", "manager@inventory.local", "Manager", "Manager", admin?.Id);
        await EnsureUserAsync(userManager, "auditor", "auditor@inventory.local", "Auditor", "Auditor", admin?.Id);

        if (!await db.Categories.AnyAsync())
        {
            var general = new Category { Name = "General", Description = "General purpose items" };
            var consumable = new Category { Name = "Consumable", Description = "Consumable items" };
            db.Categories.AddRange(general, consumable);
            var pcs = new Unit { Code = "PCS", Name = "Pieces" };
            var box = new Unit { Code = "BOX", Name = "Box" };
            db.Units.AddRange(pcs, box);
            await db.SaveChangesAsync();

            var hq = new Location { Code = "HQ", Name = "Kantor Pusat", Type = LocationType.Branch, BranchCode = "HQ" };
            db.Locations.Add(hq);
            await db.SaveChangesAsync();
            db.Locations.Add(new Location
            {
                Code = "HQ-WH1", Name = "Gudang Pusat", Type = LocationType.Warehouse,
                ParentLocationId = hq.Id, BranchCode = "HQ"
            });

            db.Products.AddRange(
                new Product { Sku = "SKU-001", Name = "A4 Paper Ream", CategoryId = general.Id, UnitId = pcs.Id, ReorderLevel = 10 },
                new Product { Sku = "SKU-002", Name = "Printer Toner", CategoryId = consumable.Id, UnitId = pcs.Id, ReorderLevel = 5 });
            db.Suppliers.Add(new Supplier
            {
                Code = "SUP-001", Name = "PT Sumber Makmur", ContactPerson = "Budi",
                Phone = "021-5551234", Email = "sales@sumbermakmur.local", Address = "Jakarta"
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Sample master data seeded.");
        }

        logger.LogInformation("Database initialized using provider {Provider}.", db.Database.ProviderName);
    }

    private static async Task<AppUser?> EnsureUserAsync(
        UserManager<AppUser> userManager, string userName, string email, string fullName, string role, int? parentId)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = userName,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true,
                ParentId = parentId,
                CreatedAt = DateTime.UtcNow
            };
            var create = await userManager.CreateAsync(user, "Admin123!");
            if (!create.Succeeded)
                return null;
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);

        return user;
    }
}
