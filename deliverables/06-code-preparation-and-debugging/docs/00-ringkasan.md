# 00 - Ringkasan

## Tujuan

Memastikan kode program disiapkan sesuai standar, dapat dikompilasi, dapat
dijalankan, serta kesalahan dapat dicatat dan diperbaiki secara terukur.

## Lingkungan

| Komponen | Nilai |
|---|---|
| Framework | .NET 10 |
| Aplikasi | ASP.NET Core MVC |
| Database | PostgreSQL |
| IDE | JetBrains Rider / Visual Studio |
| CLI | `dotnet` SDK 10.0.404 |
| Test runner | xUnit |
| Dokumentasi | DocFX 2.81.0 |

## Hasil Utama

| Aspek | Hasil |
|---|---|
| Build | Sukses (0 error) |
| Unit test | 171 passed |
| Aplikasi | Berjalan dengan database PostgreSQL |
| Migrasi | Idempotent, dijalankan saat startup |
| Kendala | File lock, stale Razor cache, konfigurasi DocFX |
| Perbaikan | Clean/rebuild, stop debugger, invalidate cache, sesuaikan config |

## Pelajaran

1. Error IDE belum tentu error compiler; verifikasi dengan `dotnet build`.
2. File yang sedang dipakai debugger tidak bisa ditimpa build.
3. Tool lokal membutuhkan manifest di lokasi yang dapat ditemukan.
4. Path relatif tool/documentation harus diuji dari working directory yang benar.
5. Setiap perbaikan harus diverifikasi dengan build + test + eksekusi.
