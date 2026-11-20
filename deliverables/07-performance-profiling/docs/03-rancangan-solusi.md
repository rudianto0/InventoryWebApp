# 3. Merancang Solusi untuk Mengurangi atau Menghilangkan Bottleneck

## 3.1 Rancangan metode

**Prinsip:** kurangi jumlah query, batasi data yang dimuat, gunakan index, dan
cache data yang jarang berubah.

| Bottleneck | Rancangan solusi |
|---|---|
| B1 Mutasi stok | Ambil stok sekali untuk semua barang, lalu proses di memori |
| B2 Nomor dokumen | Naikkan nomor secara otomatis dalam satu perintah database |
| B3 Persetujuan | Ambil data pembuat sekaligus dengan join/batch |
| B4–B5 Cek stok | Ambil semua stok dengan satu query `IN (...)` |
| B6 Kartu stok | Tampilkan per halaman dan batasi rentang tanggal |
| B7 Validasi user | Simpan sementara data user selama 30–60 detik |
| B8 Audit | Simpan audit pada proses simpan yang sama |

**Contoh solusi nomor dokumen:**

```sql
INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
VALUES ('IN', 2026, 11, 1)
ON CONFLICT ("Prefix", "Year", "Month")
DO UPDATE SET "LastNumber" = "DocumentSequences"."LastNumber" + 1
RETURNING "LastNumber";
```

**Urutan prioritas:**

| Prioritas | Solusi | Risiko |
|---|---|---|
| 1 | Halaman pada kartu stok | Rendah |
| 2 | Cek stok sekaligus | Rendah |
| 3 | Data pembuat sekaligus | Rendah |
| 4 | Mutasi stok dan nomor dokumen | Sedang |
| 5 | Cache data user | Sedang |

## 3.2 Tunjukkan peningkatan performa rancangan metode

**Hasil terukur (prototipe):**

| Skenario | Sebelum | Sesudah | Peningkatan |
|---|---:|---:|---|
| Cek stok 100 produk | 7,53 ms / 1.158 KB | **0,64 ms / 173 KB** | **11,8× lebih cepat** |
| Cari pembuat 100 data | 3,89 µs / 8,59 KB | **1,27 µs / 3,05 KB** | **3,1× lebih cepat** |

**Estimasi setelah perbaikan:**

| Skenario | Sebelum | Target | Dasar |
|---|---:|---:|---|
| Mutasi 100 perubahan | 237 ms / 40 MB | ±50 ms / 10 MB | Stok diambil sekaligus |
| Persetujuan 100 data | 15,3 ms | ±3 ms | Data pembuat digabung |
| Kartu stok 10.000 baris | 30,7 ms / 5,22 MB | ±5 ms / 0,5 MB | Halaman dan filter tanggal |
| Validasi user | 1 query/request | 1 query/60 detik | Cache singkat |

![Grafik Performa](assets/performance-chart.png)

**Target keberhasilan:**

| Metrik | Target |
|---|---|
| Query per transaksi 100 baris | ≤ 5 |
| Query persetujuan 100 data | ≤ 5 |
| Memori kartu stok | ≤ 1 MB |
| Waktu persetujuan (p95) | < 500 ms |
| Waktu laporan (p95) | < 2 detik |

Angka target adalah estimasi dan perlu diuji ulang dengan PostgreSQL serta data
yang lebih besar.
