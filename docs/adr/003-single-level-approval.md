# ADR 003 - Single-Level Approval (SPV)

- **Status:** Accepted
- **Date:** 2026-11-06

## Context

Transaksi stok (in/out/transfer/adjustment/opname) memerlukan persetujuan,
tetapi tidak boleh kompleks. Hanya ada **satu level approval**.

## Decision

1. Setiap **User/Person** memiliki `ParentId` (self-reference) yang menunjuk
   ke SPV-nya. SPV adalah approver default.
2. Transaksi yang dapat diapprove memiliki field status:
   `Draft → Submitted → Approved | Rejected`.
3. Saat submit, sistem menetapkan `ApproverId = CurrentUser.ParentId`.
   Jika user tidak punya parent (tidak ada SPV), submit ditolak dengan pesan
   validasi.
4. Hanya user dengan `Id == ApproverId` (atau role Admin) yang boleh
   approve/reject.
5. Reject wajib menyertakan `RejectionReason`. Transaksi dapat diperbaiki lalu
   di-submit ulang (kembali ke `Submitted`).
6. Efek stok hanya dijalankan ketika status menjadi **Approved** (bukan saat
   submit), dalam satu transaksi database dengan update `Stock`.

## Shared Contract

```csharp
public interface IApprovable
{
    ApprovalStatus Status { get; set; }
    int? ApproverId { get; set; }
    DateTime? SubmittedAt { get; set; }
    DateTime? ApprovedAt { get; set; }
    int? ApprovedById { get; set; }
    string? RejectionReason { get; set; }
}

public enum ApprovalStatus { Draft, Submitted, Approved, Rejected, Cancelled }
```

## Consequences

- **Positif:** sederhana, mudah diaudit, tidak perlu workflow engine.
- **Negatif:** tidak mendukung approval berjenjang; jika nanti perlu,
  status enum dapat diperluas tanpa mengubah model data transaksi.
- SPV tidak boleh menyetujui transaksi miliknya sendiri (validasi tambahan).
