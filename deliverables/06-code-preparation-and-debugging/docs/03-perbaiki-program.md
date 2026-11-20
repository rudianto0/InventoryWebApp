# 3. Perbaiki Program

## a. Rumuskan perbaikan terhadap kesalahan kompilasi maupun build

**Prinsip perbaikan:**

1. Cari penyebab utama, bukan hanya gejala.
2. Perbaiki satu masalah dalam satu waktu.
3. Jangan mencampur perbaikan bug dengan perubahan besar.
4. Jalankan build dan test setelah perbaikan.
5. Catat hasil perbaikan pada log kesalahan.

**Rumusan perbaikan per kasus:**

| ID | Masalah | Perbaikan | Verifikasi |
|---|---|---|---|
| ERR-01 | Namespace DTO berubah | Ubah `using` ke `Inventory.Contracts` | Build sukses |
| ERR-02 | Namespace enum berubah | Ubah referensi ke `Contracts.Enums` | Build sukses |
| ERR-03 | DLL terkunci | Hentikan debugger/IIS Express, rebuild | Rebuild sukses |
| ERR-04 | Razor merah di IDE | Clean, rebuild, invalidate cache | Build Razor sukses |
| ERR-05 | DocFX tidak ditemukan | Pulihkan tool manifest dan `dotnet tool restore` | `docfx --version` |
| ERR-06 | Quantity desimal | Ubah tipe menjadi `int`, perbarui view | Test 171 lulus |

**Pola perbaikan umum:**

| Masalah | Perbaikan |
|---|---|
| Null reference | Tambahkan pemeriksaan null |
| Logika berulang | Jadikan method atau helper |
| Query lambat | Tambahkan index atau pagination |
| Konflik stok | Gunakan transaksi dan concurrency |
| Pesan error tidak jelas | Gunakan `ValidationException` |

## b. Lakukan perbaikan dan simpan hasil perbaikan

**Langkah:**

1. Terapkan perbaikan pada kode.
2. Simpan perubahan.
3. Jalankan `dotnet build Inventory.sln`.
4. Jalankan `dotnet test Inventory.sln`.
5. Jalankan aplikasi dan uji alur utama.
6. Perbarui log kesalahan dan dokumentasi.

**Hasil verifikasi:**

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)

Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
```

**Checklist setelah perbaikan:**

- [x] Build sukses.
- [x] Test sukses (171 lulus).
- [x] Aplikasi berjalan.
- [x] Login berhasil.
- [x] Transaksi stok dapat dibuat dan disetujui.
- [x] Log kesalahan diperbarui.
- [x] Dokumentasi diperbarui.

Dengan langkah ini, kesalahan kompilasi, build, dan logika sudah diperbaiki,
disimpan, dan diverifikasi sehingga program terbebas dari bug utama.
