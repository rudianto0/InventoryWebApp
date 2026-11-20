# 2. Lakukan Debugging

## a. Kompilasi kode program menggunakan menu debug

| Tempat | Menu |
|---|---|
| Rider | Build → Build Solution / Rebuild Solution |
| Visual Studio | Build → Build Solution / Rebuild Solution |
| Terminal | `dotnet build`, `dotnet clean` |

```powershell
dotnet clean Inventory.sln
dotnet build Inventory.sln -c Debug
dotnet build Inventory.sln -c Release
```

![Build Sukses](assets/build-success.png)

## b. Analisis kriteria kode program yang lolos proses build

| Kriteria | Keterangan |
|---|---|
| Semua project berhasil dikompilasi | Wajib |
| Tidak ada error | Wajib |
| Warning tidak kritis | Wajib |
| Razor view terkompilasi | Wajib |
| File dokumentasi XML terbentuk | Wajib |
| Referensi antar project valid | Wajib |
| Project migration ikut ter-build | Wajib |

**Build dianggap sukses jika** compiler selesai tanpa error dan semua
dependency berhasil dipulihkan. Contoh output:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Penyebab build gagal yang umum:**

| Gejala | Penyebab | Solusi |
|---|---|---|
| File DLL terkunci | Aplikasi masih berjalan | Hentikan debugger lalu build ulang |
| `CS0246` | Namespace atau package hilang | Perbaiki `using`/referensi |
| `CS1061` | Method atau properti tidak ada | Sesuaikan dengan API yang benar |
| Error Razor | Model dan view tidak cocok | Perbaiki model atau `_ViewImports` |

## c. Analisis kriteria aplikasi yang berhasil dieksekusi

Aplikasi dinyatakan berhasil berjalan jika:

| No | Kriteria | Cara memeriksa |
|---|---|---|
| 1 | Proses start tanpa exception | Console atau log |
| 2 | Server mendengarkan port | Log URL |
| 3 | Halaman login terbuka | Browser |
| 4 | Login berhasil | Akun hasil seeding |
| 5 | Database terhubung | Dashboard atau daftar data |
| 6 | Migrasi dan seeding selesai | Log "Database initialized" |
| 7 | CRUD master berjalan | Tambah, ubah, hapus |
| 8 | Transaksi stok berjalan | Stock In/Out dan approve |
| 9 | Laporan menampilkan data | Stock Balance |

```powershell
dotnet run --project src/Inventory.Web
```

**Gejala gagal eksekusi:**

| Gejala | Kemungkinan | Solusi |
|---|---|---|
| Port sudah dipakai | Proses lain berjalan | Hentikan proses atau ganti port |
| Database ditolak | PostgreSQL mati atau salah host | Periksa service dan konfigurasi |
| Login gagal | User belum dibuat | Jalankan seeding |
| Error 500 | Kesalahan saat program berjalan | Lihat log dan stack trace |

## d. Pencatatan kesalahan pada kode program

Kesalahan dicatat berdasarkan jenisnya.

| Jenis | Arti | Contoh |
|---|---|---|
| Sintaks | Penulisan kode tidak valid | Kurung atau tanda kutip kurang |
| Semantik | Tipe atau referensi tidak sesuai | `CS0246`, `CS1061` |
| Logika | Kode valid tetapi hasil salah | Saldo stok tidak sesuai |
| Runtime | Gagal saat aplikasi berjalan | Koneksi database gagal |
| Environment | Gagal karena proses atau file | DLL terkunci |

**Catatan kasus:**

| ID | Jenis | Gejala | Penyebab | Perbaikan | Status |
|---|---|---|---|---|---|
| ERR-01 | Semantik | `CS0234` namespace tidak ada | DTO dipindah ke `Inventory.Contracts` | Perbarui `using` | Fixed |
| ERR-02 | Semantik | `CS0234` enum tidak ada | Enum dipindah ke `Contracts.Enums` | Perbarui referensi | Fixed |
| ERR-03 | Build | DLL tidak bisa ditimpa | Debugger/IIS Express masih berjalan | Hentikan lalu rebuild | Fixed |
| ERR-04 | Tooling | Razor merah di IDE | Cache IDE usang | Clean, rebuild, restart | Fixed |
| ERR-05 | Environment | DocFX tidak ditemukan | Tool manifest salah lokasi | `dotnet tool restore` | Fixed |
| ERR-06 | Logika | Quantity tampil `10.0000` | Tipe data desimal | Ubah ke `int` | Fixed |

**Format pencatatan baru:** ID, tanggal, jenis, modul, gejala, penyebab,
perbaikan, verifikasi, dan status.
