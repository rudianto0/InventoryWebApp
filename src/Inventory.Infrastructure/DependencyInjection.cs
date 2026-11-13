using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Dtos;
using Inventory.Application.Services;
using Inventory.Infrastructure.Identity;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connection = configuration.GetConnectionString("Default") ?? configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Connection string 'Default' belum dikonfigurasi.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connection, b => b.MigrationsAssembly("Inventory.Migrations.PostgreSql"));
            else
                options.UseSqlServer(connection, b => b.MigrationsAssembly("Inventory.Migrations.SqlServer"));
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IDocumentNumberService, Services.DocumentNumberService>();
        services.AddScoped<IUserDirectory, Services.UserDirectory>();
        services.AddScoped<IUserService, Services.UserService>();

        services.AddScoped<CatalogService>();
        services.AddScoped<SupplierService>();
        services.AddScoped<LocationService>();
        services.AddScoped<StockLedger>();
        services.AddScoped<StockInService>();
        services.AddScoped<StockOutService>();
        services.AddScoped<StockTransferService>();
        services.AddScoped<StockAdjustmentService>();
        services.AddScoped<StockOpnameService>();
        services.AddScoped<ApprovalService>();
        services.AddScoped<ReportService>();
        services.AddScoped<AuditQueryService>();

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.Name = "Inventory.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.Events.OnValidatePrincipal = async context =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                var user = await userManager.GetUserAsync(context.Principal!);
                if (user is null || !user.IsActive)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                }
            };
        });

        return services;
    }
}
