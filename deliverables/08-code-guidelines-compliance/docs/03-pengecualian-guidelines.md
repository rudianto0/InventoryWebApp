# 3. Membuat Pengecualian Penulisan Kode terhadap Coding Guidelines

Tidak semua kode harus mengikuti aturan yang sama. Pengecualian harus memiliki
alasan teknis, dicatat, dan diberi komentar bila berada di dalam kode.

## 3.1 Identifikasi kode yang memang sebaiknya tidak perlu sesuai coding-guideline

| ID | File / Area | Aturan yang dikecualikan | Alasan |
|---|---|---|---|
| EX-01 | File migration EF | Jangan mengedit kode generated | Dibuat otomatis oleh EF Core |
| EX-02 | `api-metadata/*.yml` | Gaya penulisan YAML | Hasil generate DocFX |
| EX-03 | `Program.cs` | Gaya class penuh | Gaya minimal ASP.NET Core |
| EX-04 | View Razor | Nullability `!` | Keterbatasan analisis Razor |
| EX-05 | `ApprovalGuard` | Komentar API publik | Kelas internal, bukan kontrak publik |
| EX-06 | `AppControllerBase` | Web tidak boleh memakai EF | Transisi sampai API dipisah |
| EX-07 | Script SQL | Aturan penamaan C# | SQL memakai aturan berbeda |
| EX-08 | Folder test | Gaya penamaan umum | Data bantu pengujian |

**Kriteria pengecualian yang diperbolehkan:**

- Kode dibuat otomatis oleh tool.
- Batasan framework atau bahasa.
- Alasan performa atau keamanan yang jelas.
- Kode internal yang tidak dipakai pihak luar.
- Masa transisi yang memiliki rencana perbaikan.

**Pengecualian yang tidak diperbolehkan:**

- Menyembunyikan error dengan `catch { }` tanpa log.
- Mematikan pengecekan nullable secara global.
- Menyimpan kata sandi di kode.
- Menonaktifkan test.
- Mengabaikan warning penting tanpa alasan.

## 3.2 Tulis komentar yang menjelaskan kode pengecualian

**Aturan penulisan komentar:**

1. Tulis ID pengecualian (`EX-*`).
2. Jelaskan alasan teknis.
3. Jelaskan sampai kapan pengecualian berlaku.
4. Cantumkan rencana perbaikan bila sementara.
5. Jangan memakai komentar untuk menyembunyikan error.

**Contoh komentar yang benar:**

```csharp
// EX-05: Internal workflow helper. Excluded from public API,
// so public XML documentation is not required.
internal static class ApprovalGuard
```

```csharp
// EX-06: Temporary Web -> EF dependency until Inventory.Api
// owns transaction/concurrency handling (ADR 006).
if (ex is DbUpdateConcurrencyException)
```

```cshtml
@* EX-04: Razor nullable analysis limitation; model validated by controller. *@
@Model.Title!
```

```sql
-- EX-07: PostgreSQL script; C# naming/style rules do not apply.
```

**Contoh komentar yang harus dihindari:**

```csharp
// BURUK: tanpa alasan
#pragma warning disable

// BURUK: menyembunyikan kegagalan
catch { }
```

**Verifikasi:**

- [x] EX-05 dan EX-06 diberi komentar di kode.
- [x] EX-01, EX-02, EX-04, dan EX-07 dijelaskan di dokumen.
- [x] Tidak ada penonaktifan warning tanpa alasan.

Pengecualian ditinjau kembali setiap ada perubahan besar. Jika alasannya sudah
tidak berlaku, pengecualian dihapus.
