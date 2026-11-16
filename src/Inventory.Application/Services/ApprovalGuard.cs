using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;

namespace Inventory.Application.Services;

// EX-05: Internal workflow helper. It is intentionally excluded from the public
// API surface and therefore does not require public XML documentation.
internal static class ApprovalGuard
{
    public static void EnsureOwner(int? createdById, int currentUserId, bool isAdmin, string action)
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

    public static void MarkApproved(IApprovable header, int currentUserId)
    {
        var now = DateTime.UtcNow;
        header.Status = ApprovalStatus.Approved;
        header.SubmittedAt ??= now;
        header.ApproverId ??= currentUserId;
        header.ApprovedAt = now;
        header.ApprovedById = currentUserId;
        header.RejectionReason = null;
    }

    public static void EnsureCanApprove(IApprovable header, int? createdById, int currentUserId, bool isAdmin)
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
