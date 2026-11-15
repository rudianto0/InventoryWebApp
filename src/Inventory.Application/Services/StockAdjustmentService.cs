using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Dtos;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class StockAdjustmentService(
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
        var query = db.StockAdjustmentHeaders.AsNoTracking().Include(x => x.Location).Include(x => x.Details).AsQueryable();
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
                PartnerName = x.Reason,
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
        var header = await db.StockAdjustmentHeaders.AsNoTracking()
            .Include(x => x.Location).Include(x => x.Details).ThenInclude(d => d.Product).ThenInclude(p => p!.Unit)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return header is null ? null : await MapDetailAsync(header, ct);
    }

    public async Task<StockAdjustmentHeader> CreateAsync(StockAdjustmentInput input, CancellationToken ct = default)
    {
        await ValidateInputAsync(input, ct);
        var header = new StockAdjustmentHeader
        {
            DocNo = await documents.NextAsync("ADJ", input.Date, ct),
            LocationId = input.LocationId,
            Date = input.Date,
            Reason = input.Reason.Trim(),
            Notes = Guard.TrimOrNull(input.Notes),
            Status = ApprovalStatus.Draft,
            CreatedById = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        foreach (var line in input.Lines)
        {
            var system = await GetSystemQuantityAsync(input.LocationId, line.ProductId, ct);
            header.Details.Add(new StockAdjustmentDetail
            {
                ProductId = line.ProductId,
                SystemQuantity = system,
                CountedQuantity = line.CountedQuantity,
                Difference = line.CountedQuantity - system,
                Notes = Guard.TrimOrNull(line.Notes)
            });
        }
        db.StockAdjustmentHeaders.Add(header);
        await db.SaveChangesAsync(ct);
        return header;
    }

    public async Task UpdateAsync(Guid id, StockAdjustmentInput input, CancellationToken ct = default)
    {
        var header = await db.StockAdjustmentHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Adjustment tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "mengubah");
        ApprovalGuard.EnsureEditable(header);
        await ValidateInputAsync(input, ct);

        header.LocationId = input.LocationId;
        header.Date = input.Date;
        header.Reason = input.Reason.Trim();
        header.Notes = Guard.TrimOrNull(input.Notes);
        header.Status = ApprovalStatus.Draft;
        header.RejectionReason = null;
        db.StockAdjustmentDetails.RemoveRange(header.Details);
        var details = new List<StockAdjustmentDetail>();
        foreach (var line in input.Lines)
        {
            var system = await GetSystemQuantityAsync(input.LocationId, line.ProductId, ct);
            details.Add(new StockAdjustmentDetail
            {
                StockAdjustmentHeaderId = header.Id,
                ProductId = line.ProductId,
                SystemQuantity = system,
                CountedQuantity = line.CountedQuantity,
                Difference = line.CountedQuantity - system,
                Notes = Guard.TrimOrNull(line.Notes)
            });
        }
        header.Details = details;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockAdjustmentHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Adjustment tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "menghapus");
        if (header.Status != ApprovalStatus.Draft)
            throw new ValidationException("Hanya dokumen Draft yang dapat dihapus.");
        db.StockAdjustmentHeaders.Remove(header);
        await db.SaveChangesAsync(ct);
    }

    public async Task SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockAdjustmentHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Adjustment tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "submit");
        ApprovalGuard.EnsureEditable(header);
        if (header.Details.Count == 0)
            throw new ValidationException("Dokumen tidak memiliki detail.");
        Guard.Required(header.Reason, "Alasan");
        await EnsureLocationActiveAsync(header.LocationId, ct);
        await RefreshSystemQuantitiesAsync(header, ct);
        await ApprovalGuard.SubmitAsync(header, CurrentUserId, users, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default)
    {
        var header = await db.StockAdjustmentHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Adjustment tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        await EnsureLocationActiveAsync(header.LocationId, ct);
        await RefreshSystemQuantitiesAsync(header, ct);

        await using var transaction = await db.BeginTransactionAsync(ct);
        try
        {
            header.Status = ApprovalStatus.Approved;
            header.ApprovedAt = DateTime.UtcNow;
            header.ApprovedById = CurrentUserId;
            if (!string.IsNullOrWhiteSpace(notes))
                header.Notes = notes.Trim();

            await ledger.ApplyAsync(header.Details.Select(d => new StockChange(
                MovementType.Adjustment, ReferenceType.StockAdjustment, header.Id, d.ProductId, header.LocationId,
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
        var header = await db.StockAdjustmentHeaders.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Adjustment tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        ApprovalGuard.Reject(header, reason, CurrentUserId);
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateInputAsync(StockAdjustmentInput input, CancellationToken ct)
    {
        Guard.Required(input.Reason, "Alasan");
        if (input.Lines.Count == 0)
            throw new ValidationException("Minimal satu baris produk harus diisi.");
        await EnsureLocationActiveAsync(input.LocationId, ct);
        var productIds = input.Lines.Select(x => x.ProductId).ToList();
        if (productIds.Count != productIds.Distinct().Count())
            throw new ValidationException("Terdapat produk duplikat pada detail.");
        var activeCount = await db.Products.CountAsync(x => productIds.Contains(x.Id) && x.IsActive, ct);
        if (activeCount != productIds.Count)
            throw new ValidationException("Terdapat produk yang tidak ditemukan atau tidak aktif.");
        foreach (var line in input.Lines)
            Guard.NotNegative(line.CountedQuantity, "Counted quantity");
    }

    private async Task RefreshSystemQuantitiesAsync(StockAdjustmentHeader header, CancellationToken ct)
    {
        foreach (var detail in header.Details)
        {
            detail.SystemQuantity = await GetSystemQuantityAsync(header.LocationId, detail.ProductId, ct);
            detail.Difference = detail.CountedQuantity - detail.SystemQuantity;
            if (detail.CountedQuantity < 0)
                throw new ValidationException("Counted quantity tidak boleh negatif.");
        }
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

    private async Task<TransactionDetailView> MapDetailAsync(StockAdjustmentHeader header, CancellationToken ct)
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
            Reason = header.Reason,
            Notes = header.Notes,
            CreatedByName = names.GetValueOrDefault(header.CreatedById ?? 0),
            ApproverName = header.ApproverId.HasValue ? names.GetValueOrDefault(header.ApproverId.Value) : null,
            CreatedAt = header.CreatedAt,
            SubmittedAt = header.SubmittedAt,
            ApprovedAt = header.ApprovedAt,
            RejectionReason = header.RejectionReason,
            Lines = header.Details.Select(d => new DocumentLineView
            {
                ProductId = d.ProductId,
                Sku = d.Product?.Sku ?? string.Empty,
                ProductName = d.Product?.Name ?? string.Empty,
                UnitName = d.Product?.Unit?.Name ?? string.Empty,
                Quantity = d.CountedQuantity,
                SystemQuantity = d.SystemQuantity,
                CountedQuantity = d.CountedQuantity,
                Difference = d.Difference,
                Notes = d.Notes
            }).ToList()
        };
    }
}
