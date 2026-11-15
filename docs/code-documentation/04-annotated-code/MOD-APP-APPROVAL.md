# MOD-APP-APPROVAL — ApprovalGuard

| Field | Nilai |
|---|---|
| Module ID | `MOD-APP-APPROVAL` |
| File | `src/Inventory.Application/Services/StockLedger.cs` (kelas `ApprovalGuard`) |
| Method ID | `MTH-APP-APPROVAL-*` |
| Kategori | Fungsi/prosedur otorisasi |
| Eksepsi | `ValidationException` |

```csharp
public static void EnsureOwner(int? createdById, int currentUserId, bool isAdmin, string action)
{                                                                       // fungsi cek pemilik
    if (createdById != currentUserId && !isAdmin)                       // bukan owner & bukan admin?
        throw new ValidationException($"Anda tidak berhak {action} dokumen ini."); // tolak
}                                                                       // akhir fungsi

public static void EnsureEditable(IApprovable header)
{                                                                       // fungsi cek status
    if (header.Status is not (ApprovalStatus.Draft or ApprovalStatus.Rejected))
        throw new ValidationException("Dokumen hanya dapat diubah saat berstatus Draft atau Rejected.");
}                                                                       // akhir fungsi

public static async Task SubmitAsync(
    IApprovable header, int currentUserId, IUserDirectory users, CancellationToken ct)
{                                                                       // prosedur submit
    var approverId = await users.GetParentIdAsync(currentUserId, ct);   // cari atasan
    if (!approverId.HasValue)                                           // tidak ada atasan?
        throw new ValidationException("Anda belum memiliki atasan (SPV). Hubungi Admin untuk mengatur parent user.");
    if (approverId.Value == currentUserId)                              // atasan = diri sendiri?
        throw new ValidationException("Anda tidak dapat menjadi approver dokumen sendiri.");
    if (!await users.IsActiveAsync(approverId.Value, ct))               // atasan nonaktif?
        throw new ValidationException("Atasan (SPV) Anda tidak aktif.");

    header.ApproverId = approverId;                                     // set approver
    header.SubmittedAt = DateTime.UtcNow;                               // waktu submit
    header.Status = ApprovalStatus.Submitted;                           // status submitted
    header.RejectionReason = null;                                      // reset alasan
}                                                                       // akhir prosedur

public static void MarkApproved(IApprovable header, int currentUserId)
{                                                                       // prosedur approve
    var now = DateTime.UtcNow;                                          // waktu sekarang
    header.Status = ApprovalStatus.Approved;                            // status approved
    header.SubmittedAt ??= now;                                         // isi jika belum ada
    header.ApproverId ??= currentUserId;                                // isi approver jika kosong
    header.ApprovedAt = now;                                            // waktu approve
    header.ApprovedById = currentUserId;                                // user approve
    header.RejectionReason = null;                                      // reset alasan
}                                                                       // akhir prosedur

public static void EnsureCanApprove(IApprovable header, int? createdById, int currentUserId, bool isAdmin)
{                                                                       // fungsi otorisasi approve
    if (createdById == currentUserId)                                   // tidak boleh approve sendiri
        throw new ValidationException("Anda tidak dapat menyetujui transaksi milik sendiri.");
    if (header.Status != ApprovalStatus.Submitted)                      // harus submitted
        throw new ValidationException("Hanya dokumen berstatus Submitted yang dapat diproses.");
    if (!header.ApproverId.HasValue)                                    // harus punya approver
        throw new ValidationException("Dokumen belum memiliki approver.");
    if (header.ApproverId.Value == currentUserId || isAdmin)            // approver/admin?
        return;                                                         // izinkan
    throw new ValidationException("Anda bukan approver dokumen ini.");  // tolak
}                                                                       // akhir fungsi

public static void Reject(IApprovable header, string reason, int currentUserId)
{                                                                       // prosedur reject
    Guard.Required(reason, "Alasan penolakan");                         // alasan wajib
    header.Status = ApprovalStatus.Rejected;                            // status rejected
    header.RejectionReason = reason.Trim();                             // simpan alasan
    header.ApprovedAt = null;                                           // reset approve
    header.ApprovedById = null;                                         // reset approver
}                                                                       // akhir prosedur
```

## Eksepsi

- Bukan owner.
- Status tidak editable.
- Tidak ada atasan / atasan diri sendiri / atasan nonaktif.
- Approve sendiri / status salah / tanpa approver / bukan approver.
- Alasan reject kosong.
