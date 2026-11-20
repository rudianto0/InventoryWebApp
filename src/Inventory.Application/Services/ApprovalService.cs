using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class ApprovalService(
    IAppDbContext db,
    ICurrentUserService currentUser,
    IUserDirectory users)
{
    private int CurrentUserId => currentUser.UserId ?? throw new ValidationException("Sesi pengguna tidak valid.");
    private bool IsAdmin => currentUser.IsInRole("Admin");

    public async Task<IReadOnlyList<ApprovalItem>> GetPendingAsync(SortSpec? sort = null, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        var permissions = db.StockInHeaders.AsNoTracking()
            .Where(x => x.Status == ApprovalStatus.Submitted)
            .Where(x => IsAdmin || x.ApproverId == userId)
            .Include(x => x.Location)
            .Select(x => new ApprovalItem
            {
                Id = x.Id, Type = "StockIn", DocNo = x.DocNo, Date = x.Date,
                LocationName = x.Location!.Name, RequesterName = null, SubmittedAt = x.SubmittedAt
            });

        var outs = db.StockOutHeaders.AsNoTracking()
            .Where(x => x.Status == ApprovalStatus.Submitted)
            .Where(x => IsAdmin || x.ApproverId == userId)
            .Include(x => x.Location)
            .Select(x => new ApprovalItem
            {
                Id = x.Id, Type = "StockOut", DocNo = x.DocNo, Date = x.Date,
                LocationName = x.Location!.Name, RequesterName = null, SubmittedAt = x.SubmittedAt
            });

        var transfers = db.StockTransferHeaders.AsNoTracking()
            .Where(x => x.Status == ApprovalStatus.Submitted)
            .Where(x => IsAdmin || x.ApproverId == userId)
            .Include(x => x.FromLocation)
            .Select(x => new ApprovalItem
            {
                Id = x.Id, Type = "StockTransfer", DocNo = x.DocNo, Date = x.Date,
                LocationName = x.FromLocation!.Name, RequesterName = null, SubmittedAt = x.SubmittedAt
            });

        var adjustments = db.StockAdjustmentHeaders.AsNoTracking()
            .Where(x => x.Status == ApprovalStatus.Submitted)
            .Where(x => IsAdmin || x.ApproverId == userId)
            .Include(x => x.Location)
            .Select(x => new ApprovalItem
            {
                Id = x.Id, Type = "StockAdjustment", DocNo = x.DocNo, Date = x.Date,
                LocationName = x.Location!.Name, RequesterName = null, SubmittedAt = x.SubmittedAt
            });

        var opnames = db.StockOpnameHeaders.AsNoTracking()
            .Where(x => x.Status == ApprovalStatus.Submitted)
            .Where(x => IsAdmin || x.ApproverId == userId)
            .Include(x => x.Location)
            .Select(x => new ApprovalItem
            {
                Id = x.Id, Type = "StockOpname", DocNo = x.DocNo, Date = x.Date,
                LocationName = x.Location!.Name, RequesterName = null, SubmittedAt = x.SubmittedAt
            });

        var items = await SortPending(
                permissions.Concat(outs).Concat(transfers).Concat(adjustments).Concat(opnames),
                Sorting.NormalizeKey(sort?.SortBy),
                sort?.SortDir ?? SortDirection.Asc)
            .ToListAsync(ct);

        var creators = new List<int>();
        foreach (var item in items)
        {
            var createdBy = item.Type switch
            {
                "StockIn" => await db.StockInHeaders.Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
                "StockOut" => await db.StockOutHeaders.Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
                "StockTransfer" => await db.StockTransferHeaders.Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
                "StockAdjustment" => await db.StockAdjustmentHeaders.Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
                _ => await db.StockOpnameHeaders.Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct)
            };
            creators.Add(createdBy ?? 0);
        }

        var names = await users.GetDisplayNamesAsync(creators, ct);
        return items.Select((item, index) => new ApprovalItem
        {
            Id = item.Id,
            Type = item.Type,
            DocNo = item.DocNo,
            Date = item.Date,
            LocationName = item.LocationName,
            RequesterName = names.GetValueOrDefault(creators[index]),
            SubmittedAt = item.SubmittedAt
        }).ToList();
    }

    private static IOrderedQueryable<ApprovalItem> SortPending(IQueryable<ApprovalItem> query, string? sortBy, SortDirection direction)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(x => x.SubmittedAt).ThenBy(x => x.DocNo);

        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<ApprovalItem> ordered = (sortBy, desc) switch
        {
            ("type", false) => query.OrderBy(x => x.Type.ToLower()),
            ("type", true) => query.OrderByDescending(x => x.Type.ToLower()),
            ("docno", false) => query.OrderBy(x => x.DocNo.ToLower()),
            ("docno", true) => query.OrderByDescending(x => x.DocNo.ToLower()),
            ("date", false) => query.OrderBy(x => x.Date),
            ("date", true) => query.OrderByDescending(x => x.Date),
            ("location", false) => query.OrderBy(x => x.LocationName.ToLower()),
            ("location", true) => query.OrderByDescending(x => x.LocationName.ToLower()),
            ("submittedat", false) => query.OrderBy(x => x.SubmittedAt == null).ThenBy(x => x.SubmittedAt),
            ("submittedat", true) => query.OrderBy(x => x.SubmittedAt == null).ThenByDescending(x => x.SubmittedAt),
            _ => query.OrderBy(x => x.SubmittedAt)
        };
        return ordered.ThenBy(x => x.DocNo);
    }
}
