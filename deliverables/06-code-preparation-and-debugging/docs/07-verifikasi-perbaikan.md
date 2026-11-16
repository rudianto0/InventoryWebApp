# 07 - Verifikasi Perbaikan

## 1. Verifikasi Build

```powershell
dotnet clean Inventory.sln
dotnet build Inventory.sln -c Debug
```

Hasil yang diharapkan:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## 2. Verifikasi Test

```powershell
dotnet test Inventory.sln
```

Hasil terakhir:

```text
Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
```

## 3. Verifikasi Eksekusi

```powershell
dotnet run --project src/Inventory.Web
```

Checklist:

- [x] Build sukses.
- [x] Test sukses.
- [x] Aplikasi start.
- [x] Login page tampil.
- [x] Login berhasil.
- [x] Database terhubung.
- [x] Dashboard tampil.
- [x] Stock Balance dapat dibuka.
- [x] Transaksi stok dapat dibuat & di-approve.

## 4. Verifikasi Database

```sql
SELECT COUNT(*) FROM "__EFMigrationsHistory";
SELECT COUNT(*) FROM "Products";
SELECT COUNT(*) FROM "Stocks";
SELECT COUNT(*) FROM "StockMovements";
```

## 5. Verifikasi Dokumentasi

```powershell
Set-Location deliverables/05-code-documentation/config
dotnet tool restore
dotnet docfx ../docfx.json
```

Hasil:

```text
Build succeeded.
    0 warning(s)
    0 error(s)
```

## 6. Status Perbaikan

| ID | Status | Verifikasi |
|---|---|---|
| ERR-01 | Fixed | Build sukses |
| ERR-02 | Fixed | Build sukses |
| ERR-03 | Fixed | Rebuild sukses |
| ERR-04 | Fixed | Build Razor sukses |
| ERR-05 | Fixed | `docfx --version` |
| ERR-06 | Fixed | DocFX build 0 warning |
| ERR-07 | Fixed | Migration script |
| ERR-08 | Fixed | Test 171 passed |

## 7. Kesimpulan

Seluruh kendala build dan eksekusi utama sudah diperbaiki dan diverifikasi.
Aplikasi dapat dibangun, diuji, dijalankan, dan dokumentasi dapat digenerate.
