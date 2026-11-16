# 05 - Log Kesalahan

## 1. Klasifikasi

| Jenis | Definisi | Contoh |
|---|---|---|
| Sintaks | Kode tidak valid secara grammar | Kurung/kuot tidak lengkap |
| Semantik | Tipe/reference tidak sesuai | `CS0246`, `CS1061` |
| Logika | Kode valid tapi hasil salah | stok tidak sesuai |
| Runtime | Gagal saat aplikasi berjalan | koneksi DB, null reference |
| Build/Environment | Gagal karena proses/file | DLL terkunci |

## 2. Log Kasus

### ERR-01 — Namespace DTO hilang setelah refactor

- **Jenis:** semantik.
- **Gejala:** `CS0234: namespace 'Dtos' does not exist`.
- **Penyebab:** DTO dipindahkan ke `Inventory.Contracts`.
- **Perbaikan:** ubah `using Inventory.Application.Dtos` menjadi `using Inventory.Contracts`.
- **Status:** fixed.

### ERR-02 — Namespace enum hilang

- **Jenis:** semantik.
- **Gejala:** `CS0234: namespace 'Enums' does not exist`.
- **Penyebab:** enum dipindahkan ke `Inventory.Contracts.Enums`.
- **Perbaikan:** update `using` dan referensi.
- **Status:** fixed.

### ERR-03 — Build gagal karena file lock

- **Jenis:** environment/build.
- **Gejala:** `MSB3021/MSB3027: file is being used by another process`.
- **Penyebab:** IIS Express/JetBrains Debugger masih berjalan.
- **Perbaikan:** stop debugger/IIS Express, rebuild.
- **Status:** fixed.

### ERR-04 — Razor view merah di IDE

- **Jenis:** environment/tooling.
- **Gejala:** `Cannot resolve symbol 'ViewData'/'User'/'Model'`.
- **Penyebab:** Razor language service/cache stale setelah menambah project.
- **Perbaikan:** `dotnet clean`, rebuild, Invalidate Caches/Restart.
- **Status:** fixed (bukan error compiler).

### ERR-05 — DocFX tidak ditemukan

- **Jenis:** environment.
- **Gejala:** `dotnet-docfx does not exist`.
- **Penyebab:** `dotnet-tools.json` root dihapus/dipindahkan.
- **Perbaikan:** jalankan `dotnet tool restore` di
  `deliverables/05-code-documentation/config`.
- **Status:** fixed.

### ERR-06 — DocFX glob `../` tidak didukung

- **Jenis:** konfigurasi.
- **Gejala:** `No files are found with glob pattern ../docs/*.md`.
- **Penyebab:** DocFX tidak mendukung `../` pada content glob.
- **Perbaikan:** pindahkan `docfx.json` ke root section dan gunakan path `docs/*.md`.
- **Status:** fixed.

### ERR-07 — Migrasi tidak bisa digenerate saat app berjalan

- **Jenis:** environment.
- **Gejala:** build untuk `dotnet ef` gagal karena DLL terkunci.
- **Perbaikan:** stop app atau gunakan `--no-build` dengan binary yang ada.
- **Status:** fixed.

### ERR-08 — Data quantity desimal tidak sesuai

- **Jenis:** logika/desain.
- **Gejala:** quantity tampil `10.0000`.
- **Penyebab:** quantity `decimal` dan format `N4`.
- **Perbaikan:** ubah quantity ke `int`, format view, regenerasi migration.
- **Status:** fixed.

## 3. Format Pencatatan Baru

| Field | Keterangan |
|---|---|
| ID | `ERR-NN` |
| Tanggal | Tanggal kejadian |
| Jenis | sintaks/semantik/logika/runtime/environment |
| Modul | Module ID |
| Gejala | Pesan error |
| Penyebab | Root cause |
| Perbaikan | Tindakan |
| Verifikasi | Build/test/run |
| Status | Open/Fixed |
