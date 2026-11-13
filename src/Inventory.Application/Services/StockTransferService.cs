using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Dtos;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class StockTransferService(
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
        var query = db.StockTransferHeaders.AsNoTracking()
            .Include(x => x.FromLocation).Include(x => x.ToLocation).Include(x => x.Details).AsQueryable();
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);
        if (filter.From.HasValue)
            query = query.Where(x => x.Date >= filter.From.Value.Date);
        if (filter.To.HasValue)
            query = query.Where(x => x.Date < filter.To.Value.Date.AddDays(1));

        var total = await query.CountAsync(ct);
        var headers = await query.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id)
            .Skip((Math.Max(filter.Page, 1) - 1) * Math.Clamp(filter.PageSize, 1, 200))
            .Take(Math.Clamp(filter.PageSize, 1, 200)).ToListAsync(ct);

        var names = await users.GetDisplayNamesAsync(headers.SelectMany(x => new[] { x.CreatedById, x.ApproverId ?? 0 }), ct);
        return new PagedResult<TransactionListItem>
        {
            Items = headers.Select(x => new TransactionListItem
            {
                Id = x.Id,
                DocNo = x.DocNo,
                Date = x.Date,
                LocationName = x.FromLocation?.Name ?? string.Empty,
                SecondaryLocationName = x.ToLocation?.Name,
                Status = x.Status,
                CreatedByName = names.GetValueOrDefault(x.CreatedById),
                ApproverName = x.ApproverId.HasValue ? names.GetValueOrDefault(x.ApproverId.Value) : null,
                CreatedAt = x.CreatedAt,
                LineCount = x.Details.Count
            }).ToList(),
            TotalCount = total,
            Page = Math.Max(filter.Page, 1),
            PageSize = Math.Clamp(filter.PageSize, 1, 200)
        };
    }

    public async Task<TransactionDetailView?> GetDetailAsync(int id, CancellationToken ct = default)
    {
        var header = await db.StockTransferHeaders.AsNoTracking()
            .Include(x => x.FromLocation).Include(x => x.ToLocation)
            .Include(x => x.Details).ThenInclude(d => d.Product).ThenInclude(p => p!.Unit)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return header is null ? null : await MapDetailAsync(header, ct);
    }

    public async Task<StockTransferHeader> CreateAsync(StockTransferInput input, CancellationToken ct = default)
    {
        await ValidateInputAsync(input.FromLocationId, input.ToLocationId, input.Lines, checkStock: false, ct);
        var header = new StockTransferHeader
        {
            DocNo = await documents.NextAsync("TRF", input.Date, ct),
            FromLocationId = input.FromLocationId,
            ToLocationId = input.ToLocationId,
            Date = input.Date,
            Notes = Guard.TrimOrNull(input.Notes),
            Status = ApprovalStatus.Draft,
            CreatedById = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        foreach (var line in input.Lines)
        {
            header.Details.Add(new StockTransferDetail
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                Notes = Guard.TrimOrNull(line.Notes)
            });
        }
        db.StockTransferHeaders.Add(header);
        await db.SaveChangesAsync(ct);
        return header;
    }

    public async Task UpdateAsync(int id, StockTransferInput input, CancellationToken ct = default)
    {
        var header = await db.StockTransferHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Transfer tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "mengubah");
        ApprovalGuard.EnsureEditable(header);
        await ValidateInputAsync(input.FromLocationId, input.ToLocationId, input.Lines, checkStock: false, ct);

        header.FromLocationId = input.FromLocationId;
        header.ToLocationId = input.ToLocationId;
        header.Date = input.Date;
        header.Notes = Guard.TrimOrNull(input.Notes);
        header.Status = ApprovalStatus.Draft;
        header.RejectionReason = null;
        db.StockTransferDetails.RemoveRange(header.Details);
        header.Details = input.Lines.Select(line => new StockTransferDetail
        {
            StockTransferHeaderId = header.Id,
            ProductId = line.ProductId,
            Quantity = line.Quantity,
            Notes = Guard.TrimOrNull(line.Notes)
        }).ToList();
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var header = await db.StockTransferHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Transfer tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "menghapus");
        if (header.Status != ApprovalStatus.Draft)
            throw new ValidationException("Hanya dokumen Draft yang dapat dihapus.");
        db.StockTransferHeaders.Remove(header);
        await db.SaveChangesAsync(ct);
    }

    public async Task SubmitAsync(int id, CancellationToken ct = default)
    {
        var header = await db.StockTransferHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Transfer tidak ditemukan.");
        ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "submit");
        ApprovalGuard.EnsureEditable(header);
        await ValidateStoredAsync(header, checkStock: true, ct);
        await ApprovalGuard.SubmitAsync(header, CurrentUserId, users, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task ApproveAsync(int id, string? notes = null, CancellationToken ct = default)
    {
        var header = await db.StockTransferHeaders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Transfer tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        await ValidateStoredAsync(header, checkStock: true, ct);

        await using var transaction = await db.BeginTransactionAsync(ct);
        try
        {
            header.Status = ApprovalStatus.Approved;
            header.ApprovedAt = DateTime.UtcNow;
            header.ApprovedById = CurrentUserId;
            if (!string.IsNullOrWhiteSpace(notes))
                header.Notes = notes.Trim();

            var changes = new List<StockChange>();
            foreach (var detail in header.Details)
            {
                changes.Add(new StockChange(MovementType.TransferOut, ReferenceType.StockTransfer, header.Id,
                    detail.ProductId, header.FromLocationId, -detail.Quantity, CurrentUserId, detail.Notes));
                changes.Add(new StockChange(MovementType.TransferIn, ReferenceType.StockTransfer, header.Id,
                    detail.ProductId, header.ToLocationId, detail.Quantity, CurrentUserId, detail.Notes));
            }
            await ledger.ApplyAsync(changes, ct);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RejectAsync(int id, string reason, CancellationToken ct = default)
    {
        var header = await db.StockTransferHeaders.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Dokumen Stock Transfer tidak ditemukan.");
        ApprovalGuard.EnsureCanApprove(header, header.CreatedById, CurrentUserId, IsAdmin);
        ApprovalGuard.Reject(header, reason, CurrentUserId);
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateInputAsync(int fromId, int toId, IReadOnlyList<DocumentLineInput> lines, bool checkStock, CancellationToken ct)
    {
        if (lines.Count == 0)
            throw new ValidationException("Minimal satu baris produk harus diisi.");
        if (fromId == toId)
            throw new ValidationException("Lokasi asal dan tujuan tidak boleh sama.");
        var from = await db.Locations.FirstOrDefaultAsync(x => x.Id == fromId, ct)
            ?? throw new ValidationException("Lokasi asal tidak ditemukan.");
        var to = await db.Locations.FirstOrDefaultAsync(x => x.Id == toId, ct)
            ?? throw new ValidationException("Lokasi tujuan tidak ditemukan.");
        if (!from.IsActive || !to.IsActive)
            throw new ValidationException("Lokasi asal dan tujuan harus aktif.");

        var productIds = lines.Select(x => x.ProductId).ToList();
        if (productIds.Count != productIds.Distinct().Count())
            throw new ValidationException("Terdapat produk duplikat pada detail.");
        var activeCount = await db.Products.CountAsync(x => productIds.Contains(x.Id) && x.IsActive, ct);
        if (activeCount != productIds.Count)
            throw new ValidationException("Terdapat produk yang tidak ditemukan atau tidak aktif.");

        foreach (var line in lines)
            Guard.Positive(line.Quantity, "Quantity");

        if (checkStock)
        {
            foreach (var line in lines)
            {
                var available = await db.Stocks.Where(x => x.ProductId == line.ProductId && x.LocationId == fromId)
                    .Select(x => x.Quantity).FirstOrDefaultAsync(ct);
                if (available < line.Quantity)
                    throw new ValidationException($"Stok asal tidak mencukupi untuk salah satu produk (tersedia {available:0.####}).");
            }
        }
    }

    private async Task ValidateStoredAsync(StockTransferHeader header, bool checkStock, CancellationToken ct)
    {
        var lines = header.Details.Select(d => new DocumentLineInput { ProductId = d.ProductId, Quantity = d.Quantity }).ToList();
        await ValidateInputAsync(header.FromLocationId, header.ToLocationId, lines, checkStock, ct);
    }

    private async Task<TransactionDetailView> MapDetailAsync(StockTransferHeader header, CancellationToken ct)
    {
        var names = await users.GetDisplayNamesAsync(new[] { header.CreatedById, header.ApproverId ?? 0, header.ApprovedById ?? 0 }, ct);
        return new TransactionDetailView
        {
            Id = header.Id,
            DocNo = header.DocNo,
            Date = header.Date,
            Status = header.Status,
            FromLocationId = header.FromLocationId,
            ToLocationId = header.ToLocationId,
            LocationId = header.FromLocationId,
            LocationName = header.FromLocation?.Name ?? string.Empty,
            SecondaryLocationName = header.ToLocation?.Name,
            Notes = header.Notes,
            CreatedByName = names.GetValueOrDefault(header.CreatedById),
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
