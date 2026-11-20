using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class StockInService(
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
        var query = db.StockInHeaders.AsNoTracking()
            .Include(x => x.Location).Include(x => x.Supplier).Include(x => x.Details).AsQueryable();
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);
        if (filter.From.HasValue)
            query = query.Where(x => x.Date >= filter.From.Value.Date);
        if (filter.To.HasValue)
            query = query.Where(x => x.Date < filter.To.Value.Date.AddDays(1));

        var total = await query.CountAsync(ct);
        var headers = await SortHeaders(query, Sorting.NormalizeKey(filter.SortBy), filter.SortDir)
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
                PartnerName = x.Supplier?.Name,
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

    private static IOrderedQueryable<StockInHeader> SortHeaders(IQueryable<StockInHeader> query, string? sortBy, SortDirection direction)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id);

        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<StockInHeader> ordered = (sortBy, desc) switch
        {
            ("docno", false) => query.OrderBy(x => x.DocNo.ToLower()),
            ("docno", true) => query.OrderByDescending(x => x.DocNo.ToLower()),
            ("date", false) => query.OrderBy(x => x.Date),
            ("date", true) => query.OrderByDescending(x => x.Date),
            ("location", false) => query.OrderBy(x => x.Location!.Name.ToLower()),
            ("location", true) => query.OrderByDescending(x => x.Location!.Name.ToLower()),
            ("status", false) => query.OrderBy(x => x.Status),
            ("status", true) => query.OrderByDescending(x => x.Status),
            ("createdat", false) => query.OrderBy(x => x.CreatedAt),
            ("createdat", true) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.Date)
        };
        return ordered.ThenBy(x => x.Id);
    }

    public async Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockInHeaders.AsNoTracking()
            .Include(x => x.Location).Include(x => x.Supplier).Include(x => x.Details).ThenInclude(d => d.Product).ThenInclude(p => p!.Unit)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return header is null ? null : await MapDetailAsync(header, ct);
    }

    public async Task<TransactionCreatedResponse> CreateAsync(StockInInput input, CancellationToken ct = default)
    {
        await ValidateInputAsync(input, ct);
        var header = new StockInHeader
        {
            DocNo = await documents.NextAsync("IN", input.Date, ct),
            SupplierId = input.SupplierId,
            LocationId = input.LocationId,
            Date = input.Date,
            Notes = Guard.TrimOrNull(input.Notes),
            Status = ApprovalStatus.Draft,
            CreatedById = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        foreach (var line in input.Lines)
        {
            header.Details.Add(new StockInDetail
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                Notes = Guard.TrimOrNull(line.Notes)
            });
        }
        db.StockInHeaders.Add(header);
        await db.SaveChangesAsync(ct);
        return new TransactionCreatedResponse
        {
            Id = header.Id,
            DocNo = header.DocNo,
            Status = header.Status,
            CreatedById = header.CreatedById,
            CreatedAt = header.CreatedAt,
            SupplierId = header.SupplierId,
            LocationId = header.LocationId,
            Notes = header.Notes,
            Details = header.Details.Select(d => new DocumentLineView
            {
                ProductId = d.ProductId, Quantity = d.Quantity, UnitCost = d.UnitCost, Notes = d.Notes
            }).ToList()
        };
    }

    public async Task UpdateAsync(Guid id, StockInInput input, CancellationToken ct = default)
    {
        var header = await db.StockInHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock In tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "mengubah");
        ApprovalGuard.EnsureEditable(header);
        await ValidateInputAsync(input, ct);

        header.SupplierId = input.SupplierId;
        header.LocationId = input.LocationId;
        header.Date = input.Date;
        header.Notes = Guard.TrimOrNull(input.Notes);
        header.Status = ApprovalStatus.Draft;
        header.RejectionReason = null;
        db.StockInDetails.RemoveRange(header.Details);
        var details = input.Lines.Select(line => new StockInDetail
        {
            StockInHeaderId = header.Id,
            ProductId = line.ProductId,
            Quantity = line.Quantity,
            UnitCost = line.UnitCost,
            Notes = Guard.TrimOrNull(line.Notes)
        }).ToList();
        db.StockInDetails.AddRange(details);
        header.Details = details;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockInHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock In tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "menghapus");
        if (header.Status != ApprovalStatus.Draft)
            throw new ValidationException("Hanya dokumen Draft yang dapat dihapus.");
        db.StockInHeaders.Remove(header);
        await db.SaveChangesAsync(ct);
    }

    public async Task SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockInHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock In tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "submit");
        ApprovalGuard.EnsureEditable(header);
        await ValidateStoredAsync(header, ct);
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
        var header = await db.StockInHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock In tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        await ValidateStoredAsync(header, ct);
        await ApplyApprovalAsync(header, notes, ct);
    }

    private async Task ApplyApprovalAsync(StockInHeader header, string? notes, CancellationToken ct)
    {
        await using var transaction = await db.BeginTransactionAsync(ct);
        try
        {
            ApprovalGuard.MarkApproved(header, CurrentUserId);
            if (!string.IsNullOrWhiteSpace(notes))
                header.Notes = notes.Trim();

            await ledger.ApplyAsync(header.Details.Select(d => new StockChange(
                MovementType.In, ReferenceType.StockIn, header.Id, d.ProductId, header.LocationId,
                d.Quantity, CurrentUserId, d.Notes)), ct);
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
        var header = await db.StockInHeaders.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock In tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        ApprovalGuard.Reject(header, reason, CurrentUserId);
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateInputAsync(StockInInput input, CancellationToken ct)
    {
        if (input.Lines.Count == 0)
            throw new ValidationException("Minimal satu baris produk harus diisi.");
        var supplier = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == input.SupplierId, ct)
            ?? throw new ValidationException("Supplier tidak ditemukan.");
        if (!supplier.IsActive)
            throw new ValidationException("Supplier tidak aktif.");
        await EnsureLocationActiveAsync(input.LocationId, ct);
        await ValidateLinesAsync(input.Lines.Select(x => x.ProductId).ToList(), ct);
        foreach (var line in input.Lines)
        {
            Guard.Positive(line.Quantity, "Quantity");
            if (line.UnitCost.HasValue)
                Guard.NotNegative(line.UnitCost.Value, "Unit cost");
        }
    }

    private async Task ValidateStoredAsync(StockInHeader header, CancellationToken ct)
    {
        if (header.Details.Count == 0)
            throw new ValidationException("Dokumen tidak memiliki detail.");
        var supplier = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == header.SupplierId, ct)
            ?? throw new ValidationException("Supplier tidak ditemukan.");
        if (!supplier.IsActive)
            throw new ValidationException("Supplier tidak aktif.");
        await EnsureLocationActiveAsync(header.LocationId, ct);
        await ValidateLinesAsync(header.Details.Select(x => x.ProductId).ToList(), ct);
        foreach (var detail in header.Details)
        {
            if (detail.UnitCost.HasValue)
                Guard.NotNegative(detail.UnitCost.Value, "Unit cost");
        }
    }

    private async Task EnsureLocationActiveAsync(Guid locationId, CancellationToken ct)
    {
        var location = await db.Locations.FirstOrDefaultAsync(x => x.Id == locationId, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        if (!location.IsActive)
            throw new ValidationException("Lokasi tidak aktif.");
    }

    private async Task ValidateLinesAsync(IReadOnlyCollection<Guid> productIds, CancellationToken ct)
    {
        if (productIds.Count != productIds.Distinct().Count())
            throw new ValidationException("Terdapat produk duplikat pada detail.");
        var activeCount = await db.Products.CountAsync(x => productIds.Contains(x.Id) && x.IsActive, ct);
        if (activeCount != productIds.Count)
            throw new ValidationException("Terdapat produk yang tidak ditemukan atau tidak aktif.");
    }

    private async Task<TransactionDetailView> MapDetailAsync(StockInHeader header, CancellationToken ct)
    {
        var names = await users.GetDisplayNamesAsync(new[] { header.CreatedById ?? 0, header.ApproverId ?? 0, header.ApprovedById ?? 0 }, ct);
        return new TransactionDetailView
        {
            Id = header.Id,
            DocNo = header.DocNo,
            Date = header.Date,
            Status = header.Status,
            SupplierId = header.SupplierId,
            LocationId = header.LocationId,
            LocationName = header.Location?.Name ?? string.Empty,
            PartnerName = header.Supplier?.Name,
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
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                Notes = d.Notes
            }).ToList()
        };
    }
}
