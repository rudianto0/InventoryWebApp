# 2. Menentukan Bottleneck Performa

## 2.1 Identifikasi bottleneck performa pada program

| ID | Bottleneck | Lokasi | Bukti | Kompleksitas |
|---|---|---|---|---|
| B1 | Query stok per perubahan | `StockLedger.ApplyAsync` | 237 ms / 100 perubahan | O(k) query |
| B2 | Simpan database per nomor | `DocumentNumberService` | Dipanggil setiap pergerakan | O(k) simpan |
| B3 | Query pembuat per item | `ApprovalService` | 15,3 ms / 100 pengajuan | O(n) query |
| B4 | Query stok per baris | Stock Out / Transfer | 7,53 ms vs 0,64 ms | O(n) query |
| B5 | Query jumlah sistem per baris | Adjustment / Opname | Loop + query | O(n) query |
| B6 | Semua pergerakan dimuat | `GetStockCardAsync` | 30,7 ms / 5,22 MB | O(m) memori |
| B7 | Validasi user per request | `OnValidatePrincipal` | 1 query setiap request | O(1) per request |

**Bottleneck utama:**

1. **Mutasi stok** melakukan query berulang untuk setiap perubahan.
2. **Nomor dokumen** menyimpan database setiap kali nomor dibuat.
3. **Persetujuan** mengambil data pembuat satu per satu.
4. **Kartu stok** memuat seluruh riwayat sekaligus.

Penyebab utamanya bukan kecepatan CPU, melainkan **jumlah komunikasi ke
database** dan **banyaknya data yang dimuat**.

## 2.2 Identifikasi dampak negatif bottleneck terhadap performa

| Dampak | Penjelasan |
|---|---|
| Proses lebih lambat | Waktu bertambah sebanding jumlah baris atau data |
| Database sibuk | Terlalu banyak query berulang |
| Memori besar | Data riwayat dimuat seluruhnya |
| Risiko timeout | Halaman laporan berat saat data banyak |
| Konflik data | Nomor dokumen dan stok rawan bentrok |
| Sulit berkembang | Penambahan pengguna memperberat beban |

**Dampak ke pengguna:**

- Halaman persetujuan lambat saat antrean banyak.
- Transaksi dengan banyak barang menunggu lebih lama.
- Kartu stok barang lama bisa gagal dibuka.
- Login dan perpindahan halaman terasa lebih berat.

**Kesimpulan:** pada skala kecil aplikasi masih berjalan, tetapi kecepatan
menurun seiring bertambahnya data dan pengguna.
