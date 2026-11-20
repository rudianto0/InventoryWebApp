# 4. Menentukan Kompleksitas Algoritma

## 4.1 Identifikasi algoritma pada program yang terindikasi bermasalah

| Algoritma | Lokasi | Kompleksitas | Dampak |
|---|---|---|---|
| Loop perubahan + query stok | `StockLedger.ApplyAsync` | O(k) query | Beban database naik linear |
| Loop nomor dokumen | `DocumentNumberService` | O(k) simpan | Simpan berulang |
| Loop pengajuan + query pembuat | `ApprovalService` | O(n) query | N+1 |
| Loop baris + query stok | Stock Out / Transfer | O(n) query | Query berulang |
| Loop detail + query jumlah sistem | Adjustment / Opname | O(n) query | Query berulang |
| Muat semua pergerakan | `GetStockCardAsync` | O(m) waktu dan memori | Memori linear |
| Validasi user per request | `OnValidatePrincipal` | O(1) per request | Query setiap request |

## 4.2 Tentukan metode untuk mengukur kompleksitas algoritma

1. **Analisis kode** — hitung perulangan dan query di dalamnya.
2. **Hitung jumlah query** — melalui log EF Core atau profiler.
3. **Benchmark variasi data** — 10, 100, dan 1.000 data; lihat pertumbuhan waktu.
4. **Bandingkan pola** — apakah naik linear O(n) atau kuadratik O(n²).
5. **Ukur memori** — lihat jumlah memori yang dialokasikan.

Rumus sederhana:

```
waktu(n)  ≈ a + b·n        (linear)
waktu(n)  ≈ a + b·log(n)   (logaritmik)
memori(n) ≈ c·n            (memuat semua data)
```

## 4.3 Identifikasi kompleksitas algoritma yang berdampak penurunan performa

| Pola | Mengapa memperlambat |
|---|---|
| O(n) query database | Waktu tunggu database dikali jumlah data |
| O(n) memuat data | Memori dan pembersihan memori meningkat |
| Query di dalam loop | Terjadi N+1 |
| Tanpa index | Database memindai semua baris |
| Urut tanpa index | Waktu urut dan baca data bertambah |
| Terlalu banyak tracking | Memori dan pemeriksaan perubahan berat |

**Target kompleksitas:**

| Operasi | Sekarang | Target |
|---|---|---|
| Mutasi k perubahan | O(k) query | O(1) query |
| Persetujuan n data | O(n) query | O(1) query |
| Cek stok n baris | O(n) query | O(1) query |
| Kartu stok m data | O(m) memori | O(ukuran halaman) |
| Validasi user | O(1) per request | O(1) per interval |

**Kesimpulan:** penurunan performa terutama disebabkan oleh **kompleksitas
komunikasi database O(n)** dan **pemuatan data O(n)**, bukan perhitungan CPU
yang berat. Dengan batch, halaman, index, dan cache, kompleksitas efektif dapat
diturunkan secara signifikan.
