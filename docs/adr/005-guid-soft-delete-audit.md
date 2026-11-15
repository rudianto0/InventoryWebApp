# ADR-005 - GUID, Soft Delete, dan Audit User

- **Status:** Accepted
- **Date:** 2026-11-15
- **Konteks:** Aplikasi belum pernah dijalankan dan database masih aman untuk
  perubahan schema besar.

## Keputusan

1. **GUID untuk data bisnis**
   - Master data (`Category`, `Unit`, `Product`, `Supplier`, `Location`),
     inventory (`Stock`, `StockMovement`), dan seluruh header/detail transaksi
     memakai primary key `Guid`.
   - GUID dibuat di aplikasi dengan `Guid.CreateVersion7()` agar time-ordered
     dan ramah index, serta portabel antara SQL Server dan PostgreSQL.
   - `Users`/`Roles` (ASP.NET Identity), `DocumentSequences`, dan `AuditLogs`
     tetap memakai `int`.
   - `StockMovement.ReferenceId` menjadi `Guid`.
   - `AuditLog.EntityId` menjadi `string(64)` agar bisa menampung `int` dan `Guid`.

2. **Soft delete**
   - Master data dan header/detail transaksi memiliki `IsDeleted`, `DeletedAt`,
     `DeletedById`.
   - `SaveChangesAsync` mengubah entity berstatus `Deleted` menjadi
     `IsDeleted = true` (bukan hard delete).
   - Global query filter `!IsDeleted` diterapkan otomatis.
   - `Stocks` dan `StockMovements` tidak di-soft-delete (ledger append-only).
   - Master data yang sudah direferensikan tidak boleh dihapus; gunakan
     `IsActive = false`.
   - Unique index memakai filter `IsDeleted = false` agar nilai unik bisa
     dipakai ulang setelah soft delete.

3. **Audit user**
   - Semua entity bisnis memiliki `CreatedAt/CreatedById`,
     `UpdatedAt/UpdatedById`, dan (untuk soft delete) `DeletedAt/DeletedById`.
   - Nilai diisi otomatis oleh `AppDbContext.SaveChangesAsync` dari
     `ICurrentUserService`.
   - ID user bertipe `int?` dan tidak dibuat sebagai FK fisik karena layer
     Domain tidak boleh bergantung pada Identity.

## Konsekuensi

**Positif**
- Histori data tidak hilang dan dapat diaudit.
- ID tidak mudah ditebak dan aman untuk integrasi/URL.
- Konsisten di SQL Server dan PostgreSQL.

**Negatif / Risiko**
- Query filter dapat menyembunyikan relasi jika master yang direferensikan
  ikut dihapus; karena itu delete master yang masih dipakai diblokir.
- Ukuran index sedikit lebih besar dibanding `int`.
- Semua query yang perlu melihat data terhapus harus memakai
  `IgnoreQueryFilters()` dan dibatasi ke role admin.

## Migrasi

Karena database belum dipakai, migration `InitialCreate` untuk SQL Server dan
PostgreSQL dihapus lalu dibuat ulang. Database lokal yang sudah terbentuk harus
di-drop sebelum menjalankan aplikasi.
