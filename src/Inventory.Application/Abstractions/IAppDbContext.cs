using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Unit> Units { get; }
    DbSet<Product> Products { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Location> Locations { get; }
    DbSet<Stock> Stocks { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<StockInHeader> StockInHeaders { get; }
    DbSet<StockInDetail> StockInDetails { get; }
    DbSet<StockOutHeader> StockOutHeaders { get; }
    DbSet<StockOutDetail> StockOutDetails { get; }
    DbSet<StockTransferHeader> StockTransferHeaders { get; }
    DbSet<StockTransferDetail> StockTransferDetails { get; }
    DbSet<StockAdjustmentHeader> StockAdjustmentHeaders { get; }
    DbSet<StockAdjustmentDetail> StockAdjustmentDetails { get; }
    DbSet<StockOpnameHeader> StockOpnameHeaders { get; }
    DbSet<StockOpnameDetail> StockOpnameDetails { get; }
    DbSet<DocumentSequence> DocumentSequences { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
