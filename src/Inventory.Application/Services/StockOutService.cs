using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class StockOutService(
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
        var query = db.StockOutHeaders.AsNoTracking()
            .Include(x => x.Location).Include(x => x.Details).AsQueryable();
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
                PartnerName = x.Purpose,
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

    private static IOrderedQueryable<StockOutHeader> SortHeaders(IQueryable<StockOutHeader> query, string? sortBy, SortDirection direction)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id);

        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<StockOutHeader> ordered = (sortBy, desc) switch
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
        var header = await db.StockOutHeaders.AsNoTracking()
            .Include(x => x.Location).Include(x => x.Details).ThenInclude(d => d.Product).ThenInclude(p => p!.Unit)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return header is null ? null : await MapDetailAsync(header, ct);
    }

    public async Task<TransactionCreatedResponse> CreateAsync(StockOutInput input, CancellationToken ct = default)
    {
        await ValidateInputAsync(input, checkStock: false, ct);
        var header = new StockOutHeader
        {
            DocNo = await documents.NextAsync("OUT", input.Date, ct),
            LocationId = input.LocationId,
            Date = input.Date,
            Purpose = Guard.TrimOrNull(input.Purpose),
            Notes = Guard.TrimOrNull(input.Notes),
            Status = ApprovalStatus.Draft,
            CreatedById = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        foreach (var line in input.Lines)
        {
            header.Details.Add(new StockOutDetail
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                Notes = Guard.TrimOrNull(line.Notes)
            });
        }
        db.StockOutHeaders.Add(header);
        await db.SaveChangesAsync(ct);
        return new TransactionCreatedResponse
        {
            Id = header.Id,
            DocNo = header.DocNo,
            Status = header.Status,
            CreatedById = header.CreatedById,
            CreatedAt = header.CreatedAt,
            LocationId = header.LocationId,
            Purpose = header.Purpose,
            Notes = header.Notes,
            Details = header.Details.Select(d => new DocumentLineView
            {
                ProductId = d.ProductId, Quantity = d.Quantity, Notes = d.Notes
            }).ToList()
        };
    }

    public async Task UpdateAsync(Guid id, StockOutInput input, CancellationToken ct = default)
    {
        var header = await db.StockOutHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Out tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "mengubah");
        ApprovalGuard.EnsureEditable(header);
        await ValidateInputAsync(input, checkStock: false, ct);

        header.LocationId = input.LocationId;
        header.Date = input.Date;
        header.Purpose = Guard.TrimOrNull(input.Purpose);
        header.Notes = Guard.TrimOrNull(input.Notes);
        header.Status = ApprovalStatus.Draft;
        header.RejectionReason = null;
        db.StockOutDetails.RemoveRange(header.Details);
        var details = input.Lines.Select(line => new StockOutDetail
        {
            StockOutHeaderId = header.Id,
            ProductId = line.ProductId,
            Quantity = line.Quantity,
            Notes = Guard.TrimOrNull(line.Notes)
        }).ToList();
        db.StockOutDetails.AddRange(details);
        header.Details = details;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockOutHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Out tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "menghapus");
        if (header.Status != ApprovalStatus.Draft)
            throw new ValidationException("Hanya dokumen Draft yang dapat dihapus.");
        db.StockOutHeaders.Remove(header);
        await db.SaveChangesAsync(ct);
    }

    public async Task SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var header = await db.StockOutHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Out tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "submit");
        ApprovalGuard.EnsureEditable(header);
        await ValidateStoredAsync(header, checkStock: true, ct);
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
        var header = await db.StockOutHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Out tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        await ValidateStoredAsync(header, checkStock: true, ct);
        await ApplyApprovalAsync(header, notes, ct);
    }

    private async Task ApplyApprovalAsync(StockOutHeader header, string? notes, CancellationToken ct)
    {
        await using var transaction = await db.BeginTransactionAsync(ct);
        try
        {
            ApprovalGuard.MarkApproved(header, CurrentUserId);
            if (!string.IsNullOrWhiteSpace(notes))
                header.Notes = notes.Trim();

            await ledger.ApplyAsync(header.Details.Select(d => new StockChange(
                MovementType.Out, ReferenceType.StockOut, header.Id, d.ProductId, header.LocationId,
                -d.Quantity, CurrentUserId, d.Notes)), ct);
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
        var header = await db.StockOutHeaders.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Out tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        ApprovalGuard.Reject(header, reason, CurrentUserId);
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateInputAsync(StockOutInput input, bool checkStock, CancellationToken ct)
    {
        if (input.Lines.Count == 0)
            throw new ValidationException("Minimal satu baris produk harus diisi.");
        var location = await db.Locations.FirstOrDefaultAsync(x => x.Id == input.LocationId, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        if (!location.IsActive)
            throw new ValidationException("Lokasi tidak aktif.");
        await ValidateLinesAsync(input.Lines.Select(x => x.ProductId).ToList(), input.LocationId, checkStock ? input.Lines : null, ct);
        foreach (var line in input.Lines)
            Guard.Positive(line.Quantity, "Quantity");
    }

    private async Task ValidateStoredAsync(StockOutHeader header, bool checkStock, CancellationToken ct)
    {
        if (header.Details.Count == 0)
            throw new ValidationException("Dokumen tidak memiliki detail.");
        var location = await db.Locations.FirstOrDefaultAsync(x => x.Id == header.LocationId, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        if (!location.IsActive)
            throw new ValidationException("Lokasi tidak aktif.");
        var lines = header.Details.Select(d => new DocumentLineInput { ProductId = d.ProductId, Quantity = d.Quantity }).ToList();
        await ValidateLinesAsync(lines.Select(x => x.ProductId).ToList(), header.LocationId, checkStock ? lines : null, ct);
    }

    private async Task ValidateLinesAsync(IReadOnlyCollection<Guid> productIds, Guid locationId, IReadOnlyCollection<DocumentLineInput>? stockLines, CancellationToken ct)
    {
        if (productIds.Count != productIds.Distinct().Count())
            throw new ValidationException("Terdapat produk duplikat pada detail.");
        var activeCount = await db.Products.CountAsync(x => productIds.Contains(x.Id) && x.IsActive, ct);
        if (activeCount != productIds.Count)
            throw new ValidationException("Terdapat produk yang tidak ditemukan atau tidak aktif.");

        if (stockLines is null)
            return;

        foreach (var line in stockLines)
        {
            var available = await db.Stocks.Where(x => x.ProductId == line.ProductId && x.LocationId == locationId)
                .Select(x => x.Quantity).FirstOrDefaultAsync(ct);
            if (available < line.Quantity)
                throw new ValidationException($"Stok tidak mencukupi untuk salah satu produk (tersedia {available}).");
        }
    }

    private async Task<TransactionDetailView> MapDetailAsync(StockOutHeader header, CancellationToken ct)
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
            Purpose = header.Purpose,
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
                Notes = d.Notes
            }).ToList()
        };
    }
}
