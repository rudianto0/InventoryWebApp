# 4. Uji Program Basis Data

## a. Siapkan skenario pengujian.

| No | Skenario | Hasil yang diharapkan |
|---|---|---|
| 1 | Login/logout | Berhasil, menu sesuai peran |
| 2 | CRUD master | Data tersimpan dan berubah |
| 3 | Stock in/out | Saldo naik/turun, pergerakan tercatat |
| 4 | Opname/adjustment | Selisih benar, saldo sesuai |
| 5 | Total stok | Angka sesuai data transaksi |
| 6 | Hak akses | Pengguna tidak berhak ditolak |
| 7 | Koneksi aman | Koneksi terenkripsi |
| 8 | Performa | Halaman utama < 2 detik |

## b. Baca logika pemrograman mengacu pada kinerja statement akses data.

- Aktifkan log query untuk melihat perintah yang dijalankan.
- Hitung jumlah query saat membuka satu halaman.
- Jika jumlah query bertambah mengikuti jumlah baris (N+1), gabungkan query.
- Beri penanda pada query penting agar mudah dicari.
- Gunakan `EXPLAIN (ANALYZE, BUFFERS)` untuk melihat apakah indeks dipakai.

```sql
SELECT query, calls, mean_exec_time
FROM pg_stat_statements
ORDER BY total_exec_time DESC
LIMIT 10;
```

## c. Uji performansi mengacu pada kinerja statement akses data yang akan dibaca.

**Data uji:** 10.000 produk, 20.000 baris stok, 100.000 pergerakan stok,
20.000 header transaksi, dan 100.000 audit.

**Langkah pengujian:**

1. Catat waktu query sebelum indeks ditambahkan.
2. Tambahkan indeks yang diperlukan.
3. Jalankan query yang sama dan bandingkan hasilnya.
4. Uji beberapa pengguna mengakses laporan bersamaan.

| Metrik | Target awal |
|---|---|
| Daftar data (p95) | < 500 ms |
| Laporan (p95) | < 2 detik |
| Tingkat error | < 1% |
| Penggunaan CPU database | < 70% |

**Kesimpulan yang diharapkan:** query laporan dan kartu stok menjadi lebih
cepat setelah indeks dan filter tanggal diterapkan, serta jumlah query tidak
meningkat tajam saat data bertambah.
