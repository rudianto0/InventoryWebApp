# 06 - Perbaikan Program

## 1. Prinsip Perbaikan

1. Identifikasi **root cause**, bukan hanya gejala.
2. Perbaiki satu masalah pada satu waktu.
3. Jangan mencampur refactor besar dengan bug fix.
4. Jalankan build + test setelah perbaikan.
5. Catat perubahan di log kesalahan.

## 2. Rumusan Perbaikan per Kasus

### ERR-01/ERR-02 — Namespace setelah refactor
- **Tindakan:** perbarui `using` ke `Inventory.Contracts` dan
  `Inventory.Contracts.Enums`.
- **Verifikasi:** `dotnet build` sukses.
- **Dampak:** semua project konsisten.

### ERR-03 — File lock
- **Tindakan:** hentikan proses IIS Express/JetBrains Debugger, lalu rebuild.
- **Verifikasi:** `dotnet build Inventory.sln` sukses.
- **Pencegahan:** stop debug sebelum build.

### ERR-04 — Razor stale
- **Tindakan:** `dotnet clean`, rebuild, Invalidate Caches/Restart.
- **Verifikasi:** `dotnet build src/Inventory.Web` sukses.
- **Catatan:** error IDE bukan error compiler.

### ERR-05 — DocFX tool
- **Tindakan:** pindahkan `dotnet-tools.json` ke
  `deliverables/05-code-documentation/config` dan jalankan
  `dotnet tool restore` dari folder tersebut.
- **Verifikasi:** `dotnet docfx --version` dan build DocFX.

### ERR-06 — DocFX glob
- **Tindakan:** pindahkan `docfx.json` ke root section dan gunakan path
  `docs/*.md` tanpa `../`.
- **Verifikasi:** DocFX build 0 warning/error.

### ERR-07 — Migration generation
- **Tindakan:** stop aplikasi atau gunakan `--no-build`.
- **Verifikasi:** migration script dihasilkan.

### ERR-08 — Quantity desimal
- **Tindakan:** ubah tipe quantity ke `int`, hapus format `N4`,
  dan regenerasi migration.
- **Verifikasi:** view menampilkan bilangan bulat; test 171 passed.

## 3. Pola Perbaikan Kode

| Masalah | Pola perbaikan |
|---|---|
| Null reference | Guard/null check/required |
| Duplikasi logika | Ekstrak method/helper |
| Query lambat | Index/projection/pagination |
| Konflik stok | Transaksi + concurrency handling |
| Error validasi | `ValidationException` + pesan jelas |
| Dead code | Hapus/refactor |

## 4. Checklist Setelah Perbaikan

- [ ] Build sukses.
- [ ] Test sukses.
- [ ] Aplikasi berjalan.
- [ ] Log kesalahan diperbarui.
- [ ] Dokumentasi diperbarui.
- [ ] Tidak ada kredensial ter-commit.
