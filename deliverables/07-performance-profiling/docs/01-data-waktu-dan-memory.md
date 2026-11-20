# 1. Mengumpulkan Data Waktu Eksekusi Komponen-Komponen

Pengukuran dilakukan dengan **BenchmarkDotNet** pada .NET 10 menggunakan
database SQLite in-memory agar hasil mudah diulang.

> Catatan: `Ledger_Apply_100`, `Approval_Pending_100`, dan `StockCard_10000`
> memanggil service aplikasi. `StockCheck_*` dan `CreatorLookup_*` adalah
> **prototipe di project benchmark**, bukan method service aplikasi; hasilnya
> menggambarkan pola query, bukan pengukuran langsung method produksi.

## 1.1 Kumpulkan informasi waktu eksekusi function, procedure, atau method

| Benchmark | Skenario | Waktu rata-rata | Perkiraan query |
|---|---|---:|---:|
| `Ledger_Apply_100` | 100 perubahan stok | **237,4 ms** | ±100 query |
| `Approval_Pending_100` | 100 pengajuan | **15,3 ms** | ±105 query |
| `StockCard_10000` | 10.000 pergerakan | **30,7 ms** | 1 query besar |
| `StockCheck_Naive_100` | 100 produk, per baris | **7,53 ms** | 100 query |
| `StockCheck_Batch_100` | 100 produk, satu query | **0,64 ms** | 1 query |
| `CreatorLookup_Naive_100` | pencarian linear | **3,89 µs** | tanpa query |
| `CreatorLookup_Batch_100` | pencarian dictionary | **1,27 µs** | tanpa query |

**Arti sederhana:** semakin banyak query dan semakin besar data yang dimuat,
semakin lama waktu eksekusi.

## 1.2 Kumpulkan informasi penggunaan memory

| Benchmark | Memori terpakai | Gen0 per 1000 | Catatan |
|---|---:|---:|---|
| `Ledger_Apply_100` | **40.348 KB** | 17.000 | Banyak objek dan pergerakan |
| `Approval_Pending_100` | **1.279 KB** | 625 | Daftar item dan query |
| `StockCard_10000` | **5.221 KB** | 906 | Semua data dimuat sekaligus |
| `StockCheck_Naive_100` | **1.158 KB** | 562 | 100 query terpisah |
| `StockCheck_Batch_100` | **173 KB** | 84 | Satu query |
| `CreatorLookup_Naive_100` | **8,59 KB** | 4,2 | Pencarian linear |
| `CreatorLookup_Batch_100` | **3,05 KB** | 1,5 | Dictionary |

**Arti sederhana:** memuat banyak data sekaligus membuat memori dan
pembersihan (GC) meningkat.

## 1.3 Identifikasi modul yang terindikasi bermasalah

| Module ID | Modul | Indikasi | Prioritas |
|---|---|---|---|
| `MOD-APP-LEDGER` | Mutasi stok | Query berulang per perubahan | Tinggi |
| `MOD-INF-DOCNUM` | Nomor dokumen | Simpan database setiap nomor | Tinggi |
| `MOD-APP-APPROVAL` | Persetujuan | Query data pembuat per item | Tinggi |
| `MOD-APP-STOCKOUT` | Barang keluar | Query stok per baris | Tinggi |
| `MOD-APP-TRANSFER` | Transfer | Query stok per baris | Tinggi |
| `MOD-APP-OPNAME` | Stok opname | Query jumlah sistem per baris | Tinggi |
| `MOD-APP-REPORT` | Laporan | Kartu stok tanpa halaman | Tinggi |
| `MOD-WEB-UI` | Login | Validasi user setiap request | Sedang |

**Kesimpulan:** masalah utama ada pada **alur transaksi stok** dan **pembacaan
riwayat**, bukan pada CRUD master data.
