# 1. Tentukan Kebutuhan Uji Coba dalam Pengembangan

Contoh pada skenario menyebut modul pelanggan dan tarif listrik. Standar
pengujian yang sama diterapkan pada aplikasi **Inventory**, khususnya modul
master (supplier, produk, lokasi) dan transaksi stok.

## a. Identifikasi prosedur uji coba sesuai software development life cycle

| Fase | Kegiatan | Hasil |
|---|---|---|
| Perencanaan | Menentukan kebutuhan dan aturan bisnis | Skenario uji |
| Desain | Meninjau data masukan dan alur persetujuan | Rencana uji |
| Implementasi | Membuat unit test per modul | 171 test |
| Pengujian | Menjalankan unit test dan regresi | Hasil uji |
| Deployment | Uji cepat setelah migrasi | Checklist |
| Perawatan | Mengulang test saat kode berubah | Laporan bug |

**Tingkat pengujian:**

| Tingkat | Cakupan | Status |
|---|---|---|
| Unit | Service, ledger, validasi | Ada (171 test) |
| Integration | Database PostgreSQL | Belum |
| API | Controller dan endpoint | Belum |
| UI | Halaman web | Belum |
| Performance | Kecepatan program | Ada di bagian 07 |

## b. Tentukan tools uji coba

| Tool | Fungsi |
|---|---|
| xUnit | Kerangka unit test |
| FluentAssertions | Memeriksa hasil uji |
| EF Core SQLite in-memory | Database uji sementara |
| coverlet | Mengukur cakupan kode |
| `dotnet test` | Menjalankan test dan menyimpan hasil |
| Rider / Visual Studio | Menjalankan dan debug test |

## c. Identifikasi standar dan kondisi uji coba

**Standar penulisan test:**

- Pola **AAA**: siapkan data, jalankan aksi, periksa hasil.
- Nama test: `Method_Scenario_Expected`.
- Satu test memeriksa satu perilaku.
- Setiap test memakai database baru agar terisolasi.
- Hasil harus dapat diulang (deterministik).
- Tidak memakai data produksi.

**Kondisi pengujian:**

| Aspek | Kondisi |
|---|---|
| Framework | .NET 10 |
| Sistem operasi | Windows 11 |
| Database uji | SQLite in-memory |
| Konfigurasi | Debug |
| Test runner | xUnit 2.9.3 |
| Pengukuran cakupan | coverlet |

**Kriteria selesai:**

- Tidak ada test gagal.
- Tidak ada test dilewati.
- Build tanpa error.
- Cakupan kode dilaporkan.
- Tidak ada bug utama yang tersisa.
