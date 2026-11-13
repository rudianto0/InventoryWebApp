using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Dtos;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class ApprovalService(
    IAppDbContext db,
    ICurrentUserService currentUser,
    IUserDirectory users)
{
    private int CurrentUserId => currentUser.UserId ?? throw new ValidationException("Sesi pengguna tidak valid.");
    private bool IsAdmin => currentUser.IsInRole("Admin");

    public async Task<IReadOnlyList<ApprovalItem>> GetPendingAsync(CancellationToken ct = default)
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

        var items = await permissions.Concat(outs).Concat(transfers).Concat(adjustments).Concat(opnames)
            .OrderBy(x => x.SubmittedAt).ThenBy(x => x.DocNo).ToListAsync(ct);

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
            creators.Add(createdBy);
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
}
