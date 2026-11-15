# 03 - Analisis Kompleksitas dan Beban Kerja

## 3.1 Kompleksitas Aplikasi

### Kompleksitas Fitur

| Kelompok | Kompleksitas | Alasan |
|---|---|---|
| Master data (kategori, unit, produk, supplier, lokasi) | Rendah–sedang | CRUD + validasi unik + soft delete + relasi |
| Stock In/Out | Sedang | Validasi produk/lokasi/supplier, approval, ledger, penomoran |
| Stock Transfer | Sedang | Dua lokasi, validasi saldo asal, dua movement |
| Stock Adjustment | Sedang | Snapshot system quantity, difference, approval |
| Stock Opname | Sedang–tinggi | Snapshot banyak baris, counted quantity, sesi per lokasi |
| Approval | Sedang | Satu level, otorisasi, status workflow |
| Reporting | Sedang | Join multi-tabel, filter tanggal, export CSV |
| Audit & soft delete | Sedang | Otomatis di `SaveChanges`, query filter global |
| Identity & RBAC | Sedang | User, role, parent/atasan, cookie, lockout |

Secara arsitektur, aplikasi adalah **modular monolith** dengan boundary client
yang sudah disiapkan. Kompleksitas operasional masih terkendali untuk tim kecil.

### Kompleksitas Data

- ±20 tabel bisnis + tabel Identity.
- Relasi header–detail dengan cascade pada detail transaksi.
- Soft delete pada master dan transaksi.
- Audit log mencatat create/update/delete dalam JSON.
- Global query filter pada entity soft-delete dan `Stocks`/`StockMovements`.

### Kompleksitas Query

| Pola | Contoh | Kompleksitas |
|---|---|---|
| Lookup sederhana | Get by Id, opsi dropdown | O(1) dengan index |
| List + filter + paging | Daftar transaksi, audit | O(log n + page size) |
| Detail + include | Detail transaksi + produk/unit | O(lines) |
| Ledger update | Stock in/out/transfer/adjustment/opname | O(lines) + row lock/konflik |
| Report join | Stock balance, movement, reorder | O(data terfilter) |
| Stock card | Semua movement produk | O(movement) — perlu pagination |

Tidak ada operasi algoritmik berat di aplikasi; beban utama ada di database.

## 3.2 Asumsi Beban Kerja untuk Skala CV

| Parameter | Asumsi awal | Catatan |
|---|---|---|
| Jumlah karyawan | 20–100 | Termasuk gudang dan admin |
| Pengguna aplikasi | 10–50 | Tidak semua aktif bersamaan |
| Concurrent users | 5–20 (normal), 30–50 (peak) | Jam kerja dan akhir bulan |
| Produk aktif | 500–10.000 | Bisa bertambah |
| Lokasi/gudang | 2–20 | Multi-lokasi |
| Transaksi/bulan | 1.000–10.000 | In/out/transfer/adjustment |
| Baris per transaksi | 1–20 | Rata-rata 3–5 |
| Opname | 1–4 sesi/bulan | Bisa ribuan baris per sesi |
| Retensi | 3 tahun | Transaksi dan audit |
| Jam operasional | 08.00–18.00 | Peak siang dan akhir bulan |

## 3.3 Estimasi Volume Data 3 Tahun

Rumus:

```
baris detail/tahun = transaksi/bulan × 12 × rata-rata baris
movement/tahun    = transaksi approved/bulan × 12 × rata-rata baris
audit/tahun       = jumlah create/update/delete/tahun
```

Contoh skenario menengah:

- 5.000 transaksi/bulan × 12 × 4 baris = **240.000 detail/tahun**.
- Movement mendekati jumlah detail (tergantung transfer = 2 movement).
- Audit log bisa 1–3 baris per aksi → **ratusan ribu baris/tahun**.
- Dalam 3 tahun: jutaan baris movement + audit.

Pada volume ini PostgreSQL/SQL Server dengan index yang tepat masih memadai,
asalkan laporan difilter tanggal dan tidak memindai seluruh tabel.

## 3.4 Estimasi Beban Request

Rumus sederhana:

```
RPS = (concurrent users × request per user per menit) / 60
```

Contoh:

- 20 concurrent × 10 request/menit = 200 request/menit ≈ **3,3 RPS**.
- Peak 50 concurrent × 15 request/menit = 750 request/menit ≈ **12,5 RPS**.

Beban ini ringan untuk satu instance aplikasi yang sehat. Yang lebih menantang
adalah **query DB per request** (S1) dan **laporan/stock card** (S5–S6), bukan
throughput HTTP.

## 3.5 Titik Kontensi Utama

1. `Stocks` untuk produk/lokasi populer saat transaksi bersamaan.
2. `DocumentSequences` saat banyak dokumen dibuat pada prefix/bulan sama.
3. `AuditLogs` saat banyak write dan query audit.
4. `StockMovements` saat laporan historis/stock card.

## 3.6 Kesimpulan Kompleksitas

- Untuk CV dengan puluhan pengguna dan ribuan transaksi/bulan, aplikasi
  **cukup sederhana dan sesuai**.
- Kompleksitas akan meningkat terutama karena **volume histori**, bukan jumlah
  fitur.
- Strategi yang paling berdampak: optimasi query laporan, pagination, index,
  caching ringan, dan pemisahan database dari aplikasi.
