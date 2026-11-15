using System.Linq.Expressions;
using System.Text.Json;
using Inventory.Application.Abstractions;
using Inventory.Domain.Common;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>, IAppDbContext
{
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _auditing;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockInHeader> StockInHeaders => Set<StockInHeader>();
    public DbSet<StockInDetail> StockInDetails => Set<StockInDetail>();
    public DbSet<StockOutHeader> StockOutHeaders => Set<StockOutHeader>();
    public DbSet<StockOutDetail> StockOutDetails => Set<StockOutDetail>();
    public DbSet<StockTransferHeader> StockTransferHeaders => Set<StockTransferHeader>();
    public DbSet<StockTransferDetail> StockTransferDetails => Set<StockTransferDetail>();
    public DbSet<StockAdjustmentHeader> StockAdjustmentHeaders => Set<StockAdjustmentHeader>();
    public DbSet<StockAdjustmentDetail> StockAdjustmentDetails => Set<StockAdjustmentDetail>();
    public DbSet<StockOpnameHeader> StockOpnameHeaders => Set<StockOpnameHeader>();
    public DbSet<StockOpnameDetail> StockOpnameDetails => Set<StockOpnameDetail>();
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    private static readonly HashSet<Type> AuditedTypes =
    [
        typeof(Category), typeof(Unit), typeof(Product), typeof(Supplier), typeof(Location),
        typeof(StockInHeader), typeof(StockOutHeader), typeof(StockTransferHeader),
        typeof(StockAdjustmentHeader), typeof(StockOpnameHeader), typeof(AppUser)
    ];

    private string ActiveRowFilter
    {
        get
        {
            var provider = Database.ProviderName ?? string.Empty;
            if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
                return "\"IsDeleted\" = false";
            if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                return "IsDeleted = 0";
            return "[IsDeleted] = 0";
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var activeFilter = ActiveRowFilter;

        builder.Entity<AppUser>(b =>
        {
            b.ToTable("Users");
            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            b.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ParentId);
        });
        builder.Entity<AppRole>(b => b.ToTable("Roles"));
        builder.Entity<IdentityUserRole<int>>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<int>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<int>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<int>>(b => b.ToTable("UserTokens"));

        builder.Entity<Category>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasIndex(x => x.Name).IsUnique().HasFilter(activeFilter);
        });

        builder.Entity<Unit>(b =>
        {
            b.Property(x => x.Code).HasMaxLength(30).IsRequired();
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Code).IsUnique().HasFilter(activeFilter);
        });

        builder.Entity<Product>(b =>
        {
            b.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.Sku).IsUnique().HasFilter(activeFilter);
            b.HasIndex(x => x.CategoryId);
            b.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany(x => x.Products).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Supplier>(b =>
        {
            b.Property(x => x.Code).HasMaxLength(30).IsRequired();
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.ContactPerson).HasMaxLength(150);
            b.Property(x => x.Phone).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(150);
            b.Property(x => x.Address).HasMaxLength(300);
            b.HasIndex(x => x.Code).IsUnique().HasFilter(activeFilter);
        });

        builder.Entity<Location>(b =>
        {
            b.Property(x => x.Code).HasMaxLength(30).IsRequired();
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.BranchCode).HasMaxLength(30);
            b.Property(x => x.Address).HasMaxLength(300);
            b.HasIndex(x => x.Code).IsUnique().HasFilter(activeFilter);
            b.HasIndex(x => x.ParentLocationId);
            b.HasIndex(x => x.BranchCode);
            b.HasOne(x => x.ParentLocation).WithMany(x => x.Children).HasForeignKey(x => x.ParentLocationId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Stock>(b =>
        {
            b.Property(x => x.Version).IsConcurrencyToken();
            b.HasIndex(x => new { x.ProductId, x.LocationId }).IsUnique();
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(x => !x.Product!.IsDeleted && !x.Location!.IsDeleted);
        });

        builder.Entity<StockMovement>(b =>
        {
            b.Property(x => x.MovementNo).HasMaxLength(50).IsRequired();
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasIndex(x => new { x.ProductId, x.LocationId, x.CreatedAt });
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(x => !x.Product!.IsDeleted && !x.Location!.IsDeleted);
        });

        builder.Entity<StockInHeader>(b =>
        {
            ConfigureHeader(b, x => x.DocNo, activeFilter);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasIndex(x => x.CreatedById);
            b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Details).WithOne(x => x.Header).HasForeignKey(x => x.StockInHeaderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockInDetail>(b =>
        {
            b.Property(x => x.UnitCost).HasPrecision(18, 4);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockOutHeader>(b =>
        {
            ConfigureHeader(b, x => x.DocNo, activeFilter);
            b.Property(x => x.Purpose).HasMaxLength(200);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Details).WithOne(x => x.Header).HasForeignKey(x => x.StockOutHeaderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockOutDetail>(b =>
        {
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockTransferHeader>(b =>
        {
            ConfigureHeader(b, x => x.DocNo, activeFilter);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.FromLocation).WithMany().HasForeignKey(x => x.FromLocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ToLocation).WithMany().HasForeignKey(x => x.ToLocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Details).WithOne(x => x.Header).HasForeignKey(x => x.StockTransferHeaderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockTransferDetail>(b =>
        {
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockAdjustmentHeader>(b =>
        {
            ConfigureHeader(b, x => x.DocNo, activeFilter);
            b.Property(x => x.Reason).HasMaxLength(300).IsRequired();
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Details).WithOne(x => x.Header).HasForeignKey(x => x.StockAdjustmentHeaderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockAdjustmentDetail>(b =>
        {
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockOpnameHeader>(b =>
        {
            ConfigureHeader(b, x => x.DocNo, activeFilter);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Details).WithOne(x => x.Header).HasForeignKey(x => x.StockOpnameHeaderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockOpnameDetail>(b =>
        {
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DocumentSequence>(b =>
        {
            b.Property(x => x.Prefix).HasMaxLength(10).IsRequired();
            b.HasIndex(x => new { x.Prefix, x.Year, x.Month }).IsUnique();
        });

        builder.Entity<AuditLog>(b =>
        {
            b.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            b.Property(x => x.EntityId).HasMaxLength(64).IsRequired();
            b.Property(x => x.Action).HasMaxLength(30).IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(50);
            b.HasIndex(x => new { x.EntityName, x.CreatedAt });
            b.HasIndex(x => x.UserId);
        });

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var body = Expression.Not(Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)));
            builder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }

    private static void ConfigureHeader<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> b,
        Expression<Func<T, object?>> docNo, string activeFilter) where T : class
    {
        b.Property(docNo).HasMaxLength(30).IsRequired();
        b.HasIndex(docNo).IsUnique().HasFilter(activeFilter);
        b.HasIndex("Status");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_auditing)
            return await base.SaveChangesAsync(cancellationToken);

        var snapshots = CaptureAuditSnapshots();
        ApplyAuditAndSoftDelete();
        var result = await base.SaveChangesAsync(cancellationToken);

        if (snapshots.Count > 0)
        {
            _auditing = true;
            try
            {
                foreach (var snapshot in snapshots)
                {
                    var entityId = snapshot.EntityId ?? ResolveEntityId(snapshot.Entry);
                    AuditLogs.Add(new AuditLog
                    {
                        EntityName = snapshot.EntityName,
                        EntityId = entityId,
                        Action = snapshot.Action,
                        OldValues = snapshot.OldValues,
                        NewValues = snapshot.NewValues,
                        UserId = _currentUser.UserId,
                        IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await base.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _auditing = false;
            }
        }
        return result;
    }

    private void ApplyAuditAndSoftDelete()
    {
        var userId = _currentUser.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is IAuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = now;
                    auditable.CreatedById = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = now;
                    auditable.UpdatedById = userId;
                }
            }

            if (entry.State != EntityState.Deleted || entry.Entity is not ISoftDeletable soft)
                continue;

            entry.State = EntityState.Modified;
            soft.IsDeleted = true;
            soft.DeletedAt = now;
            soft.DeletedById = userId;

            var isActiveProperty = entry.Metadata.FindProperty("IsActive");
            if (isActiveProperty is not null)
                entry.Property("IsActive").CurrentValue = false;
        }
    }

    private List<AuditSnapshot> CaptureAuditSnapshots()
    {
        var snapshots = new List<AuditSnapshot>();
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => AuditedTypes.Contains(e.Entity.GetType()))
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Deleted => "Delete",
                _ => ResolveUpdateAction(entry)
            };

            Dictionary<string, object?>? oldValues = null;
            Dictionary<string, object?>? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                oldValues = new Dictionary<string, object?>();
                newValues = new Dictionary<string, object?>();
                foreach (var property in entry.Properties)
                {
                    if (IsSensitive(property.Metadata.Name))
                        continue;
                    var original = property.OriginalValue;
                    var current = property.CurrentValue;
                    if (!Equals(original, current))
                    {
                        oldValues[property.Metadata.Name] = original;
                        newValues[property.Metadata.Name] = current;
                    }
                }
                if (newValues.Count == 0)
                    continue;
            }
            else if (entry.State == EntityState.Added)
            {
                newValues = entry.Properties.Where(p => !IsSensitive(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
            }
            else
            {
                oldValues = entry.Properties.Where(p => !IsSensitive(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
            }

            snapshots.Add(new AuditSnapshot
            {
                Entry = entry,
                EntityName = entry.Entity.GetType().Name,
                Action = action,
                OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues),
                EntityId = entry.State == EntityState.Deleted ? ResolveEntityId(entry) : null
            });
        }
        return snapshots;
    }

    private static string ResolveUpdateAction(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.Entity is IApprovable)
        {
            var statusProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(IApprovable.Status));
            if (statusProperty is not null && !Equals(statusProperty.OriginalValue, statusProperty.CurrentValue))
            {
                return statusProperty.CurrentValue?.ToString() switch
                {
                    "Submitted" => "Submit",
                    "Approved" => "Approve",
                    "Rejected" => "Reject",
                    _ => "Update"
                };
            }
        }
        return "Update";
    }

    private static string ResolveEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        var value = idProperty?.CurrentValue ?? idProperty?.OriginalValue;
        return value switch
        {
            null => string.Empty,
            Guid guid => guid.ToString(),
            _ => Convert.ToString(value) ?? string.Empty
        };
    }

    private static bool IsSensitive(string propertyName) =>
        propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("SecurityStamp", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("ConcurrencyStamp", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase);

    private sealed class AuditSnapshot
    {
        public required Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; init; }
        public required string EntityName { get; init; }
        public required string Action { get; init; }
        public string? OldValues { get; init; }
        public string? NewValues { get; init; }
        public string? EntityId { get; init; }
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);
}