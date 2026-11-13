using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class AuditQueryService(IAppDbContext db, IUserDirectory users)
{
    public async Task<PagedResult<AuditListItem>> ListAsync(AuditFilter filter, CancellationToken ct = default)
    {
        var query = db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.EntityName))
            query = query.Where(x => x.EntityName == filter.EntityName);
        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(x => x.Action == filter.Action);
        if (filter.UserId.HasValue)
            query = query.Where(x => x.UserId == filter.UserId.Value);
        if (filter.From.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.From.Value.ToDateTime(TimeOnly.MinValue));
        if (filter.To.HasValue)
            query = query.Where(x => x.CreatedAt < filter.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var total = await query.CountAsync(ct);
        var logs = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((Math.Max(filter.Page, 1) - 1) * Math.Clamp(filter.PageSize, 1, 200))
            .Take(Math.Clamp(filter.PageSize, 1, 200)).ToListAsync(ct);

        var names = await users.GetDisplayNamesAsync(logs.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value), ct);
        return new PagedResult<AuditListItem>
        {
            Items = logs.Select(x => new AuditListItem
            {
                Id = x.Id,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Action = x.Action,
                UserId = x.UserId,
                UserName = x.UserId.HasValue ? names.GetValueOrDefault(x.UserId.Value) : null,
                IpAddress = x.IpAddress,
                CreatedAt = x.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page = Math.Max(filter.Page, 1),
            PageSize = Math.Clamp(filter.PageSize, 1, 200)
        };
    }

    public async Task<AuditDetailItem?> GetAsync(int id, CancellationToken ct = default)
    {
        var log = await db.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (log is null)
            return null;
        var name = log.UserId.HasValue ? await users.GetDisplayNameAsync(log.UserId.Value, ct) : null;
        return new AuditDetailItem
        {
            Id = log.Id,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            Action = log.Action,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            UserName = name,
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt
        };
    }

    public async Task<IReadOnlyList<string>> GetEntityNamesAsync(CancellationToken ct = default) =>
        await db.AuditLogs.AsNoTracking().Select(x => x.EntityName).Distinct().OrderBy(x => x).ToListAsync(ct);
}
