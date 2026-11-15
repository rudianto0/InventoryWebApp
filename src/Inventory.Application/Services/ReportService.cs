using System.Text;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class ReportService(IAppDbContext db)
{
    public async Task<DashboardSummary> GetDashboardAsync(Guid? locationId = null, CancellationToken ct = default)
    {
        var stockQuery = db.Stocks.AsNoTracking();
        if (locationId.HasValue)
            stockQuery = stockQuery.Where(x => x.LocationId == locationId.Value);

        var totalProducts = await db.Products.CountAsync(x => x.IsActive, ct);
        var totalQuantity = await stockQuery.SumAsync(x => (decimal?)x.Quantity, ct) ?? 0m;
        var totalLocations = await db.Locations.CountAsync(x => x.IsActive, ct);
        var pending = await db.StockInHeaders.CountAsync(x => x.Status == ApprovalStatus.Submitted, ct)
            + await db.StockOutHeaders.CountAsync(x => x.Status == ApprovalStatus.Submitted, ct)
            + await db.StockTransferHeaders.CountAsync(x => x.Status == ApprovalStatus.Submitted, ct)
            + await db.StockAdjustmentHeaders.CountAsync(x => x.Status == ApprovalStatus.Submitted, ct)
            + await db.StockOpnameHeaders.CountAsync(x => x.Status == ApprovalStatus.Submitted, ct);

        var reorder = await JoinStocks(stockQuery).Where(x => x.Quantity <= x.Product!.ReorderLevel)
            .OrderBy(x => x.Quantity).Take(10)
            .Select(x => new ReorderItem
            {
                ProductId = x.ProductId,
                Sku = x.Product!.Sku,
                ProductName = x.Product.Name,
                LocationName = x.Location!.Name,
                Quantity = x.Quantity,
                ReorderLevel = x.Product.ReorderLevel
            }).ToListAsync(ct);

        var recent = await db.StockMovements.AsNoTracking()
            .Include(x => x.Product).Include(x => x.Location)
            .OrderByDescending(x => x.CreatedAt).Take(10)
            .Select(x => new StockMovementItem
            {
                CreatedAt = x.CreatedAt,
                MovementNo = x.MovementNo,
                Type = x.Type,
                Sku = x.Product!.Sku,
                ProductName = x.Product.Name,
                LocationName = x.Location!.Name,
                Quantity = x.Quantity,
                Notes = x.Notes
            }).ToListAsync(ct);

        var belowReorder = await JoinStocks(stockQuery).CountAsync(x => x.Quantity <= x.Product!.ReorderLevel, ct);

        return new DashboardSummary
        {
            TotalProducts = totalProducts,
            TotalStockQuantity = totalQuantity,
            BelowReorderCount = belowReorder,
            PendingApprovals = pending,
            TotalLocations = totalLocations,
            ReorderItems = reorder,
            RecentMovements = recent
        };
    }

    public async Task<PagedResult<StockBalanceItem>> GetStockBalanceAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var query = JoinStocks(db.Stocks.AsNoTracking());
        query = ApplyStockFilter(query, filter);

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Product!.Sku).ThenBy(x => x.Location!.Code)
            .Skip((Math.Max(filter.Page, 1) - 1) * Math.Clamp(filter.PageSize, 1, 200))
            .Take(Math.Clamp(filter.PageSize, 1, 200))
            .Select(x => new StockBalanceItem
            {
                ProductId = x.ProductId,
                Sku = x.Product!.Sku,
                ProductName = x.Product.Name,
                UnitName = x.Product.Unit!.Name,
                LocationId = x.LocationId,
                LocationName = x.Location!.Name,
                BranchCode = x.Location.BranchCode,
                Quantity = x.Quantity,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync(ct);

        return new PagedResult<StockBalanceItem>
        {
            Items = items,
            TotalCount = total,
            Page = Math.Max(filter.Page, 1),
            PageSize = Math.Clamp(filter.PageSize, 1, 200)
        };
    }

    public async Task<IReadOnlyList<StockCardItem>> GetStockCardAsync(ReportFilter filter, CancellationToken ct = default)
    {
        if (!filter.ProductId.HasValue)
            return Array.Empty<StockCardItem>();

        var query = db.StockMovements.AsNoTracking().Include(x => x.Location)
            .Where(x => x.ProductId == filter.ProductId.Value);
        if (filter.LocationId.HasValue)
            query = query.Where(x => x.LocationId == filter.LocationId.Value);
        if (filter.From.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.From.Value.ToDateTime(TimeOnly.MinValue));
        if (filter.To.HasValue)
            query = query.Where(x => x.CreatedAt < filter.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));

        decimal opening = 0m;
        if (filter.From.HasValue)
        {
            var beforeQuery = db.StockMovements.AsNoTracking()
                .Where(x => x.ProductId == filter.ProductId.Value && x.CreatedAt < filter.From.Value.ToDateTime(TimeOnly.MinValue));
            if (filter.LocationId.HasValue)
                beforeQuery = beforeQuery.Where(x => x.LocationId == filter.LocationId.Value);
            opening = await beforeQuery.SumAsync(x => (decimal?)x.Quantity, ct) ?? 0m;
        }

        var movements = await query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Select(x => new
            {
                x.CreatedAt, x.MovementNo, x.Type, LocationName = x.Location!.Name, x.Quantity, x.Notes
            }).ToListAsync(ct);

        var result = new List<StockCardItem>();
        var running = opening;
        foreach (var movement in movements)
        {
            running += movement.Quantity;
            result.Add(new StockCardItem
            {
                CreatedAt = movement.CreatedAt,
                MovementNo = movement.MovementNo,
                Type = movement.Type,
                LocationName = movement.LocationName,
                Quantity = movement.Quantity,
                RunningBalance = running,
                Notes = movement.Notes
            });
        }
        return result;
    }

    public async Task<PagedResult<StockMovementItem>> GetMovementsAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var query = db.StockMovements.AsNoTracking().Include(x => x.Product).Include(x => x.Location).AsQueryable();
        if (filter.ProductId.HasValue)
            query = query.Where(x => x.ProductId == filter.ProductId.Value);
        if (filter.LocationId.HasValue)
            query = query.Where(x => x.LocationId == filter.LocationId.Value);
        if (!string.IsNullOrWhiteSpace(filter.BranchCode))
            query = query.Where(x => x.Location!.BranchCode == filter.BranchCode);
        if (filter.From.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.From.Value.ToDateTime(TimeOnly.MinValue));
        if (filter.To.HasValue)
            query = query.Where(x => x.CreatedAt < filter.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.CreatedAt)
            .Skip((Math.Max(filter.Page, 1) - 1) * Math.Clamp(filter.PageSize, 1, 200))
            .Take(Math.Clamp(filter.PageSize, 1, 200))
            .Select(x => new StockMovementItem
            {
                CreatedAt = x.CreatedAt,
                MovementNo = x.MovementNo,
                Type = x.Type,
                Sku = x.Product!.Sku,
                ProductName = x.Product.Name,
                LocationName = x.Location!.Name,
                Quantity = x.Quantity,
                Notes = x.Notes
            }).ToListAsync(ct);

        return new PagedResult<StockMovementItem>
        {
            Items = items,
            TotalCount = total,
            Page = Math.Max(filter.Page, 1),
            PageSize = Math.Clamp(filter.PageSize, 1, 200)
        };
    }

    public async Task<PagedResult<ReorderItem>> GetReorderAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var query = JoinStocks(db.Stocks.AsNoTracking()).Where(x => x.Quantity <= x.Product!.ReorderLevel);
        if (filter.LocationId.HasValue)
            query = query.Where(x => x.LocationId == filter.LocationId.Value);
        if (!string.IsNullOrWhiteSpace(filter.BranchCode))
            query = query.Where(x => x.Location!.BranchCode == filter.BranchCode);

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Quantity)
            .Skip((Math.Max(filter.Page, 1) - 1) * Math.Clamp(filter.PageSize, 1, 200))
            .Take(Math.Clamp(filter.PageSize, 1, 200))
            .Select(x => new ReorderItem
            {
                ProductId = x.ProductId,
                Sku = x.Product!.Sku,
                ProductName = x.Product.Name,
                LocationName = x.Location!.Name,
                Quantity = x.Quantity,
                ReorderLevel = x.Product.ReorderLevel
            }).ToListAsync(ct);

        return new PagedResult<ReorderItem>
        {
            Items = items,
            TotalCount = total,
            Page = Math.Max(filter.Page, 1),
            PageSize = Math.Clamp(filter.PageSize, 1, 200)
        };
    }

    public async Task<string> ExportCsvAsync(string report, ReportFilter filter, CancellationToken ct = default)
    {
        var builder = new StringBuilder();
        switch (report.ToLowerInvariant())
        {
            case "stock-balance":
            {
                var result = await GetStockBalanceAsync(new ReportFilter
                {
                    LocationId = filter.LocationId, BranchCode = filter.BranchCode,
                    ProductId = filter.ProductId, Page = 1, PageSize = 200
                }, ct);
                builder.AppendLine("SKU,Product,Unit,Location,BranchCode,Quantity,UpdatedAt");
                foreach (var item in result.Items)
                    builder.AppendLine($"{Csv(item.Sku)},{Csv(item.ProductName)},{Csv(item.UnitName)},{Csv(item.LocationName)},{Csv(item.BranchCode)},{item.Quantity},{item.UpdatedAt:O}");
                break;
            }
            case "reorder":
            {
                var result = await GetReorderAsync(new ReportFilter
                {
                    LocationId = filter.LocationId, BranchCode = filter.BranchCode, Page = 1, PageSize = 200
                }, ct);
                builder.AppendLine("SKU,Product,Location,Quantity,ReorderLevel");
                foreach (var item in result.Items)
                    builder.AppendLine($"{Csv(item.Sku)},{Csv(item.ProductName)},{Csv(item.LocationName)},{item.Quantity},{item.ReorderLevel}");
                break;
            }
            default:
            {
                var result = await GetMovementsAsync(new ReportFilter
                {
                    LocationId = filter.LocationId, BranchCode = filter.BranchCode, ProductId = filter.ProductId,
                    From = filter.From, To = filter.To, Page = 1, PageSize = 200
                }, ct);
                builder.AppendLine("Date,MovementNo,Type,SKU,Product,Location,Quantity,Notes");
                foreach (var item in result.Items)
                    builder.AppendLine($"{item.CreatedAt:O},{Csv(item.MovementNo)},{item.Type},{Csv(item.Sku)},{Csv(item.ProductName)},{Csv(item.LocationName)},{item.Quantity},{Csv(item.Notes)}");
                break;
            }
        }
        return builder.ToString();
    }

    private static IQueryable<Domain.Entities.Stock> JoinStocks(IQueryable<Domain.Entities.Stock> query) =>
        query.Include(x => x.Product).ThenInclude(p => p!.Unit).Include(x => x.Location);

    private static IQueryable<Domain.Entities.Stock> ApplyStockFilter(IQueryable<Domain.Entities.Stock> query, ReportFilter filter)
    {
        if (filter.LocationId.HasValue)
            query = query.Where(x => x.LocationId == filter.LocationId.Value);
        if (!string.IsNullOrWhiteSpace(filter.BranchCode))
            query = query.Where(x => x.Location!.BranchCode == filter.BranchCode);
        if (filter.ProductId.HasValue)
            query = query.Where(x => x.ProductId == filter.ProductId.Value);
        return query;
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        return value.Contains(',') || value.Contains('"')
            ? '"' + value.Replace("\"", "\"\"") + '"'
            : value;
    }
}
