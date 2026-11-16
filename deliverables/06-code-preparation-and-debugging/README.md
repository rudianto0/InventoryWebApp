# 06 - Persiapan Kode dan Debugging

## Konteks

Dokumen ini menjelaskan persiapan kode, tools debugging, proses kompilasi,
kriteria build/eksekusi, pencatatan kesalahan, serta perbaikan dan verifikasi
pada aplikasi Inventory.

## Dokumen

| File | Isi |
|---|---|
| `docs/00-ringkasan.md` | Ringkasan hasil |
| `docs/01-persiapan-kode-program.md` | Standar kode & struktur proyek |
| `docs/02-debugging-tools.md` | Tools & konfigurasi debugging |
| `docs/03-kompilasi-dan-analisis-build.md` | Kompilasi & kriteria build |
| `docs/04-analisis-kriteria-eksekusi.md` | Kriteria aplikasi berjalan |
| `docs/05-log-kesalahan.md` | Log kesalahan sintaks/semantik/logika |
| `docs/06-perbaikan-program.md` | Rumusan perbaikan |
| `docs/07-verifikasi-perbaikan.md` | Verifikasi hasil perbaikan |
| `Inventory-Code-Debugging.docx` | Gabungan Word |

## Aset

- `assets/build-success.png` — ilustrasi output build sukses.
- `assets/debug-configuration.png` — ilustrasi konfigurasi debug.

## Ringkasan Hasil

- Build: `dotnet build Inventory.sln` → sukses.
- Test: `dotnet test Inventory.sln` → 171 passed.
- Aplikasi: ASP.NET Core MVC + PostgreSQL.
- Kendala utama: file lock IIS Express, stale Razor cache, konfigurasi DocFX.
- Semua kendala memiliki langkah perbaikan dan verifikasi.
