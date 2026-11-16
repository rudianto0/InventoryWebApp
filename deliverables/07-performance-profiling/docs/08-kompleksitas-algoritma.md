# 08 - Kompleksitas Algoritma

## 1. Algoritma Bermasalah

| Algoritma | Lokasi | Kompleksitas | Dampak |
|---|---|---|---|
| Loop perubahan + query stok | `StockLedger.ApplyAsync` | O(k) query | I/O linear |
| Loop nomor dokumen | `DocumentNumberService.NextAsync` | O(k) round-trip | Save per nomor |
| Loop pending + query creator | `ApprovalService.GetPendingAsync` | O(n) query | N+1 |
| Loop baris + query stok | `StockOut/Transfer.ValidateLines` | O(n) query | I/O linear |
| Loop detail + query system qty | Adjustment/Opname | O(n) query | I/O linear |
| Materialisasi movement | `GetStockCardAsync` | O(m) waktu & memori | Memori linear |
| Validasi principal | `OnValidatePrincipal` | O(1) per request | Query tiap request |

## 2. Metode Pengukuran Kompleksitas

1. **Analisis statis** — hitung loop dan query di dalamnya.
2. **Query counting** — EF Core logging / profiler.
3. **Benchmark variasi n** — 10, 100, 1.000; lihat pertumbuhan waktu.
4. **Regresi** — cocokkan dengan O(n), O(n log n), O(n²).
5. **Memory profiler** — `Allocated` per operasi.

Contoh rumus:

```
waktu(n) ≈ a + b·n          (linear)
waktu(n) ≈ a + b·log(n)     (logaritmik)
memori(n) ≈ c·n             (materialisasi)
```

## 3. Analisis Kompleksitas

### Ledger
- Untuk k perubahan: O(k) query + O(k) movement.
- Ideal: O(1) query batch + O(k) pekerjaan memori.

### Approval
- n pending: O(n) query.
- Ideal: O(1) query + O(n) mapping.

### Stock check
- n baris: O(n) query.
- Batch: O(1) query + O(n) memory.
- Benchmark: 7,53 ms (O(n)) vs 0,64 ms (batch).

### Stock card
- m movement: O(m) waktu dan O(m) memori.
- Pagination: O(pageSize) memori.

### Creator lookup
- Linear: O(n).
- Dictionary: O(n) build + O(1) lookup.
- Benchmark: 3,89 µs vs 1,27 µs.

## 4. Kompleksitas yang Menurunkan Performa

| Pola | Mengapa menurunkan performa |
|---|---|
| O(n) round-trip DB | Latency jaringan × n |
| O(n) materialisasi | Memori & GC |
| Nested query dalam loop | N+1 |
| Tanpa index | Scan O(n) |
| Sort tanpa index | O(n log n) + I/O |
| Tracking entity besar | Memori & change detection |

## 5. Target Kompleksitas

| Operasi | Sekarang | Target |
|---|---|---|
| Ledger k perubahan | O(k) query | O(1) query |
| Approval n pending | O(n) query | O(1) query |
| Stock check n baris | O(n) query | O(1) query |
| Stock card m | O(m) memori | O(pageSize) |
| Auth request | O(1) query/request | O(1) per interval |

## 6. Kesimpulan

Penurunan performa terutama disebabkan **kompleksitas I/O O(n)** dan
**materialisasi O(n)**, bukan algoritma CPU yang berat. Perbaikan batch,
pagination, index, dan cache akan menurunkan kompleksitas efektif secara
signifikan.
