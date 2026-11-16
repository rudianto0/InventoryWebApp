# 05 - Dampak Negatif Bottleneck

## 1. Dampak Teknis

| Bottleneck | Dampak |
|---|---|
| B1 Ledger | Waktu approval panjang; transaksi besar memperlambat user lain |
| B2 Document number | Round-trip & lock contention; risiko retry/duplikasi |
| B3 Approval N+1 | Halaman approval lambat saat pending banyak |
| B4/B5 Per-line query | Submit/approve lambat; beban DB naik linear |
| B6 Stock card | Memori & response besar; risiko timeout |
| B7 Auth | Query DB setiap request; beban naik dengan traffic |
| B8 Audit save | Dua kali round-trip per operasi |
| B9 Sequence | Konflik saat concurrent submit |

## 2. Dampak ke Pengguna

- Halaman approval terasa lambat saat banyak dokumen.
- Submit transaksi dengan banyak baris menunggu lama.
- Kartu stok produk lama bisa gagal/timeout.
- Login dan navigasi lebih berat saat banyak pengguna.

## 3. Dampak ke Database

- Jumlah query tinggi (round-trip).
- Lock pada `Stocks`/`DocumentSequences` meningkat.
- Connection pool lebih cepat penuh.
- Buffer cache kurang efektif karena query berulang.

## 4. Dampak ke Skalabilitas

- Beban naik **linear** terhadap jumlah baris/pending/request.
- Penambahan user/transaksi memperbesar latency.
- Multi-instance akan memperparah contention database.

## 5. Dampak Bisnis

| Aspek | Risiko |
|---|---|
| Produktivitas | Staff menunggu approval/transaksi |
| Akurasi | Retry/duplikasi nomor dokumen |
| Pengalaman | Timeout pada laporan besar |
| Biaya | Kebutuhan upgrade DB lebih cepat |

## 6. Kesimpulan

Bottleneck tidak menyebabkan kegagalan total pada skala CV, tetapi menghasilkan
**latency yang meningkat linear** dan **penggunaan memori besar** pada histori.
Perbaikan batch/pagination/cache akan memberi dampak langsung pada pengalaman
pengguna dan kapasitas.
