using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
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
        var logs = await SortLogs(query, Sorting.NormalizeKey(filter.SortBy), filter.SortDir)
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

    private static IOrderedQueryable<Domain.Entities.AuditLog> SortLogs(IQueryable<Domain.Entities.AuditLog> query, string? sortBy, SortDirection direction)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id);

        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<Domain.Entities.AuditLog> ordered = (sortBy, desc) switch
        {
            ("entity", false) => query.OrderBy(x => x.EntityName.ToLower()),
            ("entity", true) => query.OrderByDescending(x => x.EntityName.ToLower()),
            ("action", false) => query.OrderBy(x => x.Action.ToLower()),
            ("action", true) => query.OrderByDescending(x => x.Action.ToLower()),
            ("userid", false) => query.OrderBy(x => x.UserId == null).ThenBy(x => x.UserId),
            ("userid", true) => query.OrderBy(x => x.UserId == null).ThenByDescending(x => x.UserId),
            ("ip", false) => query.OrderBy(x => x.IpAddress == null).ThenBy(x => x.IpAddress!.ToLower()),
            ("ip", true) => query.OrderBy(x => x.IpAddress == null).ThenByDescending(x => x.IpAddress!.ToLower()),
            ("createdat", false) => query.OrderBy(x => x.CreatedAt),
            ("createdat", true) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
        return ordered.ThenByDescending(x => x.Id);
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
