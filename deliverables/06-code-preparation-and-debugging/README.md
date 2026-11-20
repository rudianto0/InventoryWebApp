# 06 - Persiapan Kode dan Debugging

## Konteks

Dokumen ini menjelaskan persiapan kode, penyiapan tools debugging, proses
kompilasi, kriteria build/eksekusi, pencatatan kesalahan, serta perbaikan dan
verifikasi pada aplikasi Inventory.

## Dokumen

`docs/` disusun mengikuti 3 langkah kerja:

| File | Butir |
|---|---|
| `docs/01-persiapan-kode-dan-tools.md` | 1. Persiapan kode dan debugging tools |
| `docs/02-lakukan-debugging.md` | 2. Kompilasi, kriteria build/eksekusi, log kesalahan |
| `docs/03-perbaiki-program.md` | 3. Rumusan perbaikan dan verifikasi |
| `06-Inventory-Code-Debugging.docx` | Gabungan Word |

## Aset

- `assets/build-success.png` — ilustrasi output build sukses.
- `assets/debug-configuration.png` — ilustrasi konfigurasi debug.

## Ringkasan Hasil

- Build: `dotnet build Inventory.sln` → sukses (0 error).
- Test: `dotnet test Inventory.sln` → 171 lulus.
- Aplikasi: ASP.NET Core MVC + PostgreSQL.
- Kendala utama: file lock, stale Razor cache, konfigurasi DocFX.
- Semua kendala memiliki langkah perbaikan dan verifikasi.
