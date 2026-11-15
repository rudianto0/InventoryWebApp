using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class StockOpnameService(
    IAppDbContext db,
    ICurrentUserService currentUser,
    IUserDirectory users,
    StockLedger ledger,
    IDocumentNumberService documents)
{
    private int CurrentUserId => currentUser.UserId ?? throw new ValidationException("Sesi pengguna tidak valid.");
    private bool IsAdmin => currentUser.IsInRole("Admin");

    public async Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default)
    {
        var query = db.StockOpnameHeaders.AsNoTracking().Include(x => x.Location).Include(x => x.Details).AsQueryable();
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);
        if (filter.From.HasValue)
            query = query.Where(x => x.Date >= filter.From.Value.Date);
        if (filter.To.HasValue)
            query = query.Where(x => x.Date < filter.To.Value.Date.AddDays(1));

        var total = await query.CountAsync(ct);
        var headers = await query.OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAt)
            .Skip((Math.Max(filter.Page, 1) - 1) * Math.Clamp(filter.PageSize, 1, 200))
            .Take(Math.Clamp(filter.PageSize, 1, 200)).ToListAsync(ct);

        var names = await users.GetDisplayNamesAsync(headers.SelectMany(x => new[] { x.CreatedById ?? 0, x.ApproverId ?? 0 }), ct);
        return new PagedResult<TransactionListItem>
        {
            Items = headers.Select(x => new TransactionListItem
            {
                Id = x.Id,
                DocNo = x.DocNo,
                Date = x.Date,
                LocationName = x.Location?.Name ?? string.Empty,
                Status = x.Status,
                CreatedByName = names.GetValueOrDefault(x.CreatedById ?? 0),
                ApproverName = x.ApproverId.HasValue ? names.GetValueOrDefault(x.ApproverId.Value) : null,
                CreatedAt = x.CreatedAt,
                LineCount = x.Details.Count
            }).ToList(),
            TotalCount = total,
            Page = Math.Max(filter.Page, 1),
            PageSize = Math.Clamp(filter.PageSize, 1, 200)
        };
    }

    public async Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockOpnameHeaders.AsNoTracking()
            .Include(x => x.Location).Include(x => x.Details).ThenInclude(d => d.Product).ThenInclude(p => p!.Unit)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return header is null ? null : await MapDetailAsync(header, ct);
    }

    public async Task<TransactionCreatedResponse> CreateAsync(StockOpnameInput input, CancellationToken ct = default)
    {
        await EnsureLocationActiveAsync(input.LocationId, ct);
        var stocks = await db.Stocks.AsNoTracking()
            .Where(x => x.LocationId == input.LocationId)
            .Select(x => new { x.ProductId, x.Quantity })
            .ToListAsync(ct);

        if (stocks.Count == 0)
            throw new ValidationException("Belum ada stok pada lokasi ini. Pilih lokasi lain yang memiliki stok.");

        var header = new StockOpnameHeader
        {
            DocNo = await documents.NextAsync("OPN", input.Date, ct),
            LocationId = input.LocationId,
            Date = input.Date,
            Notes = Guard.TrimOrNull(input.Notes),
            Status = ApprovalStatus.Draft,
            CreatedById = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        foreach (var stock in stocks)
        {
            header.Details.Add(new StockOpnameDetail
            {
                ProductId = stock.ProductId,
                SystemQuantity = stock.Quantity,
                CountedQuantity = null,
                Difference = 0
            });
        }
        db.StockOpnameHeaders.Add(header);
        await db.SaveChangesAsync(ct);
        return new TransactionCreatedResponse
        {
            Id = header.Id,
            DocNo = header.DocNo,
            Status = header.Status,
            CreatedById = header.CreatedById,
            CreatedAt = header.CreatedAt,
            LocationId = header.LocationId,
            Notes = header.Notes,
            Details = header.Details.Select(d => new DocumentLineView
            {
                DetailId = d.Id,
                ProductId = d.ProductId,
                SystemQuantity = d.SystemQuantity,
                CountedQuantity = d.CountedQuantity,
                Difference = d.Difference,
                Notes = d.Notes
            }).ToList()
        };
    }

    public async Task UpdateAsync(Guid id, IReadOnlyList<OpnameLineInput> lines, string? notes, CancellationToken ct = default)
    {
        var header = await db.StockOpnameHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Opname tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "mengubah");
        ApprovalGuard.EnsureEditable(header);

        var map = lines.ToDictionary(x => x.DetailId);
        foreach (var detail in header.Details)
        {
            if (!map.TryGetValue(detail.Id, out var line))
                continue;
            if (line.CountedQuantity.HasValue)
            {
                Guard.NotNegative(line.CountedQuantity.Value, "Counted quantity");
                detail.CountedQuantity = line.CountedQuantity;
                detail.Difference = line.CountedQuantity.Value - detail.SystemQuantity;
            }
            detail.Notes = Guard.TrimOrNull(line.Notes);
        }
        header.Notes = Guard.TrimOrNull(notes);
        header.Status = ApprovalStatus.Draft;
        header.RejectionReason = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockOpnameHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Opname tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "menghapus");
        if (header.Status != ApprovalStatus.Draft)
            throw new ValidationException("Hanya dokumen Draft yang dapat dihapus.");
        db.StockOpnameHeaders.Remove(header);
        await db.SaveChangesAsync(ct);
    }

    public async Task SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockOpnameHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Opname tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "submit");
        ApprovalGuard.EnsureEditable(header);
        await EnsureLocationActiveAsync(header.LocationId, ct);
        if (await db.StockOpnameHeaders.AnyAsync(x => x.Id != id && x.LocationId == header.LocationId && x.Status == ApprovalStatus.Submitted, ct))
            throw new ValidationException("Sudah ada sesi opname berstatus Submitted untuk lokasi ini.");

        if (header.Details.Any(x => !x.CountedQuantity.HasValue))
            throw new ValidationException("Semua baris harus diisi counted quantity sebelum submit.");
        foreach (var detail in header.Details)
        {
            detail.SystemQuantity = await GetSystemQuantityAsync(header.LocationId, detail.ProductId, ct);
            detail.Difference = detail.CountedQuantity!.Value - detail.SystemQuantity;
        }
        if (IsAdmin)
        {
            await ApplyApprovalAsync(header, null, ct);
            return;
        }
        await ApprovalGuard.SubmitAsync(header, CurrentUserId, users, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default)
    {
        var header = await db.StockOpnameHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Opname tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        await EnsureLocationActiveAsync(header.LocationId, ct);
        if (header.Details.Any(x => !x.CountedQuantity.HasValue))
            throw new ValidationException("Terdapat baris yang belum memiliki counted quantity.");

        await ApplyApprovalAsync(header, notes, ct);
    }

    private async Task ApplyApprovalAsync(StockOpnameHeader header, string? notes, CancellationToken ct)
    {
        await using var transaction = await db.BeginTransactionAsync(ct);
        try
        {
            foreach (var detail in header.Details)
            {
                detail.SystemQuantity = await GetSystemQuantityAsync(header.LocationId, detail.ProductId, ct);
                detail.Difference = detail.CountedQuantity!.Value - detail.SystemQuantity;
            }

            ApprovalGuard.MarkApproved(header, CurrentUserId);
            if (!string.IsNullOrWhiteSpace(notes))
                header.Notes = notes.Trim();

            await ledger.ApplyAsync(header.Details.Select(d => new StockChange(
                MovementType.Opname, ReferenceType.StockOpname, header.Id, d.ProductId, header.LocationId,
                d.Difference, CurrentUserId, d.Notes)), ct);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RejectAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var header = await db.StockOpnameHeaders.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Opname tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        ApprovalGuard.Reject(header, reason, CurrentUserId);
        await db.SaveChangesAsync(ct);
    }

    private Task<decimal> GetSystemQuantityAsync(Guid locationId, Guid productId, CancellationToken ct) =>
        db.Stocks.Where(x => x.LocationId == locationId && x.ProductId == productId)
            .Select(x => x.Quantity).FirstOrDefaultAsync(ct);

    private async Task EnsureLocationActiveAsync(Guid locationId, CancellationToken ct)
    {
        var location = await db.Locations.FirstOrDefaultAsync(x => x.Id == locationId, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        if (!location.IsActive)
            throw new ValidationException("Lokasi tidak aktif.");
    }

    private async Task<TransactionDetailView> MapDetailAsync(StockOpnameHeader header, CancellationToken ct)
    {
        var names = await users.GetDisplayNamesAsync(new[] { header.CreatedById ?? 0, header.ApproverId ?? 0, header.ApprovedById ?? 0 }, ct);
        return new TransactionDetailView
        {
            Id = header.Id,
            DocNo = header.DocNo,
            Date = header.Date,
            Status = header.Status,
            LocationId = header.LocationId,
            LocationName = header.Location?.Name ?? string.Empty,
            Notes = header.Notes,
            CreatedByName = names.GetValueOrDefault(header.CreatedById ?? 0),
            ApproverName = header.ApproverId.HasValue ? names.GetValueOrDefault(header.ApproverId.Value) : null,
            CreatedAt = header.CreatedAt,
            SubmittedAt = header.SubmittedAt,
            ApprovedAt = header.ApprovedAt,
            RejectionReason = header.RejectionReason,
            Lines = header.Details.Select(d => new DocumentLineView
            {
                DetailId = d.Id,
                ProductId = d.ProductId,
                Sku = d.Product?.Sku ?? string.Empty,
                ProductName = d.Product?.Name ?? string.Empty,
                UnitName = d.Product?.Unit?.Name ?? string.Empty,
                Quantity = d.CountedQuantity ?? 0,
                SystemQuantity = d.SystemQuantity,
                CountedQuantity = d.CountedQuantity,
                Difference = d.Difference,
                Notes = d.Notes
            }).ToList()
        };
    }
}
