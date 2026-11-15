# 05 - Kompleksitas Algoritma

## 5.1 Kompleksitas Waktu

### Definisi

- **O(1)**: waktu konstan, tidak bergantung jumlah data.
- **O(log n)**: membagi data (binary search, index B-tree).
- **O(n)**: satu kali traversal.
- **O(n log n)**: sorting efisien.
- **O(n²)**: nested loop/sorting sederhana.
- **O(n) query DB (N+1)**: jumlah round-trip bertambah per data.

### Penerapan di aplikasi

| Operasi | Kompleksitas | Bukti |
|---|---|---|
| Cari stok by `(ProductId, LocationId)` | O(1) / O(log n) index | `Stocks` unique index |
| Cari produk by `Sku` | O(log n) | `Products.Sku` unique index |
| List transaksi + paging | O(log n + pageSize) | index `Date`, `Status` |
| Detail transaksi | O(lines) | `StockInDetails` |
| `StockLedger.ApplyAsync` | O(k) | iterasi changes |
| Validasi baris | O(n) | loop + query count |
| Sorting produk | O(n log n) | LINQ `OrderBy` |
| Bubble/Selection Sort | O(n²) | nested loop |
| Binary search | O(log n) | data terurut |
| `ApprovalService` | O(n) query DB | N+1 creator lookup |
| Stock card | O(m) | seluruh movement produk |

### Pseudocode analisis

```
FUNCTION AnalyzeLedger(changes):
    total ← 0
    FOR EACH change IN changes DO       // O(k)
        total ← total + change.Quantity
    END FOR
    RETURN total                        // O(k)
END FUNCTION
```

### Contoh N+1

```
FUNCTION GetPendingApprovals():
    items ← QUERY all pending             // 1 query
    FOR EACH item IN items DO
        creator ← QUERY user by item.Id   // n query
    END FOR
    RETURN items
END FUNCTION
```

Kompleksitas round-trip: O(n). Perbaikan: join/batch → O(1)–O(k).

### Perbandingan sorting

| Algoritma | Best | Average | Worst | Stabil | Memori |
|---|---|---|---|---|---|
| Bubble | O(n) | O(n²) | O(n²) | Ya | O(1) |
| Selection | O(n²) | O(n²) | O(n²) | Tidak | O(1) |
| Merge | O(n log n) | O(n log n) | O(n log n) | Ya | O(n) |
| Quick | O(n log n) | O(n log n) | O(n²) | Tidak | O(log n) |
| LINQ OrderBy | O(n log n) | O(n log n) | O(n log n) | Ya | O(n) |

## 5.2 Kompleksitas Memori

### Definisi

- **O(1)**: memori tambahan konstan.
- **O(log n)**: stack rekursi.
- **O(n)**: menyimpan salinan data.

### Penerapan

| Operasi | Memori | Catatan |
|---|---|---|
| Bubble/Selection Sort | O(1) | in-place |
| Quick Sort | O(log n) | stack rekursi |
| Merge Sort | O(n) | array bantu |
| LINQ `ToList` | O(n) | materialisasi |
| EF tracking | O(n) entity | change tracker |
| `AsNoTracking` | lebih hemat | read-only |
| Pagination | O(pageSize) | membatasi memori |
| Stock card tanpa paging | O(m) | berisiko untuk histori besar |

### Contoh

```csharp
var items = await db.StockMovements
    .AsNoTracking()
    .Where(x => x.ProductId == productId)
    .OrderByDescending(x => x.CreatedAt)
    .Take(100)          // O(100) memori
    .ToListAsync(ct);
```

Tanpa `.Take(100)`, materialisasi menjadi O(n) dan bisa membebani memori.

### Pseudocode

```
FUNCTION LoadStockCard(productId, limit):
    rows ← QUERY movements WHERE productId ORDER BY date DESC LIMIT limit
    RETURN rows                 // O(limit) memori
END FUNCTION
```

## 5.3 Analisis Kode Aplikasi

### StockLedger
- Waktu: O(k) untuk k perubahan.
- Memori: O(1) tambahan per perubahan (entity di-track).
- Bottleneck: lock/konflik pada baris `Stocks` yang sama.

### ApprovalService
- Waktu: O(n) query karena lookup creator per item.
- Memori: O(n) item.
- Rekomendasi: batch query → O(1) query tambahan.

### ReportService
- Stock balance: O(log n + pageSize) dengan index.
- Stock card: O(m) movement; perlu pagination.
- Dashboard: beberapa agregasi; pastikan index dan filter.

### CatalogService
- Search `Contains` pada `Sku`/`Name`: bisa O(n) scan jika tanpa index.
- Pagination membatasi hasil, tetapi query tetap memindai sesuai predicate.

## 5.4 Ringkasan

- Kompleksitas waktu dan memori harus dipertimbangkan bersama.
- Untuk data besar, index database dan pagination lebih penting daripada
  optimasi mikro di C#.
- Sorting efisien O(n log n); hindari O(n²) untuk data besar.
- N+1 adalah masalah umum yang mengubah O(1) query menjadi O(n).
- `AsNoTracking`, projection, dan `Take/Skip` membantu menekan memori.

## 5.5 Tabel Keputusan

| Kebutuhan | Pilihan |
|---|---|
| Cari satu record | index + `FirstOrDefault` |
| Cari banyak dengan filter | query + index + paging |
| Urutkan data besar | `OrderBy` (O(n log n)) |
| Data sudah terurut | binary search |
| Histori panjang | pagination + filter tanggal |
| Banyak lookup user | batch/join |
