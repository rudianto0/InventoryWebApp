# 2. Persiapkan Data Uji Coba

## 2.1 Identifikasi data uji integrasi program

**Data dasar:**

| Data | Nilai |
|---|---|
| Kategori | General |
| Satuan | PCS |
| Produk | SKU-1 / Product 1 |
| Lokasi | HQ (cabang), WH1 dan WH2 (gudang) |
| Supplier | SUP |
| Stok awal | WH1 = 10, WH2 = 0 |
| Pengguna | Staff = 10, SPV = 20, Admin = 1 |

**Data per skenario:**

| Test ID | Masukan | Hasil yang diharapkan |
|---|---|---|
| INT-01 | Jumlah 7, penyetuju 20 | Stok 7, status Approved |
| INT-02 | Stok 10, jumlah 4 | Stok 6, pergerakan keluar 4 |
| INT-03 | Stok 2, jumlah 5 | Ditolak, stok tetap 2 |
| INT-04 | WH1 10, jumlah 4 | WH1 6, WH2 4 |
| INT-05 | Sistem 10, hitung 7 | Selisih -3, stok 7 |
| INT-06 | Sistem 10, hitung 12 | Stok 12 |
| INT-07 | Pembuat 10, aktor 99 | Ditolak, stok tetap |
| INT-08 | Admin, jumlah 7 | Disetujui otomatis |
| INT-09 | Produk dan stok | Terhapus sementara, tidak tampil |
| INT-10 | Tambah/ubah/hapus kategori | Tiga catatan audit |

## 2.2 Bangkitkan data uji integrasi program

**Cara membangkitkan data:**

1. Setiap test membuat database sementara `SQLite in-memory`.
2. Tabel dibuat dengan `EnsureCreated`.
3. Data dasar diisi melalui `SeedBasicsAsync`.
4. Saldo stok diisi melalui `SeedStockAsync`.
5. Pengguna dan peran dibuat melalui `IdentityFixture`.
6. Nomor dokumen dan atasan dibuat palsu agar hasil tetap sama.
7. Database dihapus setelah test selesai.

**Variasi kondisi:**

| Kondisi | Contoh |
|---|---|
| Berhasil | Stok cukup, penyetuju valid |
| Gagal validasi | Stok kurang, jumlah negatif |
| Hak akses | Bukan penyetuju, menyetujui sendiri |
| Status dokumen | Draft, Submitted, Approved |
| Data terhapus | Produk atau lokasi soft delete |
| Admin | Persetujuan otomatis |

**Aturan data:** tidak memakai data produksi, setiap test terisolasi, data
dibersihkan setelah pengujian, dan waktu memakai UTC.
