using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed record StockChange(
    MovementType Type,
    ReferenceType ReferenceType,
    int ReferenceId,
    int ProductId,
    int LocationId,
    decimal Quantity,
    int UserId,
    string? Notes);

public sealed class StockLedger(IAppDbContext db, IDocumentNumberService documents)
{
    public async Task ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct = default)
    {
        foreach (var change in changes)
        {
            if (change.Quantity == 0)
                continue;

            var stock = await db.Stocks.FirstOrDefaultAsync(
                x => x.ProductId == change.ProductId && x.LocationId == change.LocationId, ct);

            if (stock is null)
            {
                stock = new Stock { ProductId = change.ProductId, LocationId = change.LocationId, Quantity = 0 };
                db.Stocks.Add(stock);
            }

            var newQuantity = stock.Quantity + change.Quantity;
            if (newQuantity < 0)
            {
                var product = await db.Products.Where(x => x.Id == change.ProductId).Select(x => x.Name).FirstOrDefaultAsync(ct);
                var location = await db.Locations.Where(x => x.Id == change.LocationId).Select(x => x.Name).FirstOrDefaultAsync(ct);
                throw new ValidationException($"Stok tidak mencukupi untuk produk '{product}' di lokasi '{location}'.");
            }

            stock.Quantity = newQuantity;
            stock.UpdatedAt = DateTime.UtcNow;
            stock.Version++;

            db.StockMovements.Add(new StockMovement
            {
                MovementNo = await documents.NextAsync("MOV", DateTime.UtcNow, ct),
                Type = change.Type,
                ProductId = change.ProductId,
                LocationId = change.LocationId,
                Quantity = change.Quantity,
                ReferenceType = change.ReferenceType,
                ReferenceId = change.ReferenceId,
                Notes = change.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedById = change.UserId
            });
        }
    }
}

internal static class ApprovalGuard
{
    public static void EnsureOwner(int createdById, int currentUserId, bool isAdmin, string action)
    {
        if (createdById != currentUserId && !isAdmin)
            throw new ValidationException($"Anda tidak berhak {action} dokumen ini.");
    }

    public static void EnsureEditable(IApprovable header)
    {
        if (header.Status is not (ApprovalStatus.Draft or ApprovalStatus.Rejected))
            throw new ValidationException("Dokumen hanya dapat diubah saat berstatus Draft atau Rejected.");
    }

    public static async Task SubmitAsync(IApprovable header, int currentUserId, IUserDirectory users, CancellationToken ct)
    {
        var approverId = await users.GetParentIdAsync(currentUserId, ct);
        if (!approverId.HasValue)
            throw new ValidationException("Anda belum memiliki atasan (SPV). Hubungi Admin untuk mengatur parent user.");
        if (approverId.Value == currentUserId)
            throw new ValidationException("Anda tidak dapat menjadi approver dokumen sendiri.");
        if (!await users.IsActiveAsync(approverId.Value, ct))
            throw new ValidationException("Atasan (SPV) Anda tidak aktif.");

        header.ApproverId = approverId;
        header.SubmittedAt = DateTime.UtcNow;
        header.Status = ApprovalStatus.Submitted;
        header.RejectionReason = null;
    }

    public static void EnsureCanApprove(IApprovable header, int createdById, int currentUserId, bool isAdmin)
    {
        if (createdById == currentUserId)
            throw new ValidationException("Anda tidak dapat menyetujui transaksi milik sendiri.");
        if (header.Status != ApprovalStatus.Submitted)
            throw new ValidationException("Hanya dokumen berstatus Submitted yang dapat diproses.");
        if (!header.ApproverId.HasValue)
            throw new ValidationException("Dokumen belum memiliki approver.");
        if (header.ApproverId.Value == currentUserId || isAdmin)
            return;
        throw new ValidationException("Anda bukan approver dokumen ini.");
    }

    public static void Reject(IApprovable header, string reason, int currentUserId)
    {
        Guard.Required(reason, "Alasan penolakan");
        header.Status = ApprovalStatus.Rejected;
        header.RejectionReason = reason.Trim();
        header.ApprovedAt = null;
        header.ApprovedById = null;
    }
}
