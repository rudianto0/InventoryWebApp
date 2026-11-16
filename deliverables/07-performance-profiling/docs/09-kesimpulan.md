# 09 - Kesimpulan

## 1. Temuan

1. Bottleneck utama adalah **jumlah round-trip database** dan **materialisasi
   data besar**, bukan CPU.
2. Ledger 100 changes: **237 ms / 40 MB**.
3. Approval 100 pending: **15,3 ms** dengan pola N+1.
4. Stock card 10.000 movement: **30,7 ms / 5,22 MB**.
5. Stock check batch **11,8× lebih cepat** dan **6,7× lebih hemat memori**.
6. Creator lookup dictionary **3,1× lebih cepat**.

## 2. Rekomendasi

| Prioritas | Tindakan |
|---|---|
| 1 | Pagination stock card |
| 2 | Batch stock check |
| 3 | Batch creator approval |
| 4 | Batch ledger + document number |
| 5 | Cache validasi user |
| 6 | Audit satu `SaveChanges` |

## 3. Dampak yang Diharapkan

- Approval lebih responsif saat pending banyak.
- Submit transaksi 100 baris jauh lebih cepat.
- Laporan kartu stok tidak menghabiskan memori.
- Beban database turun karena query batch.
- Kapasitas pengguna/transaksi meningkat.

## 4. Keterbatasan

- Benchmark memakai SQLite in-memory dan 3 iterasi.
- Belum ada load test HTTP.
- Target optimasi ledger/approval/stock card masih estimasi.
- Perlu validasi PostgreSQL + data 100k+.

## 5. Langkah Lanjut

1. Implementasi batch stock check & approval.
2. Tambah pagination stock card.
3. Implementasi atomic document number.
4. Ukur ulang dengan PostgreSQL.
5. Tambah regression/performance test di CI.

## 6. Penutup

Rancangan optimasi tidak memerlukan perubahan arsitektur besar. Fokus pada
batch, pagination, index, dan cache memberikan peningkatan performa yang
signifikan dan terukur.
