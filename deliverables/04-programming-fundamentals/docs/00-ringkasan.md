# 00 - Ringkasan

## Tujuan

Menghubungkan konsep dasar pemrograman dengan implementasi nyata aplikasi
Inventory, sehingga dapat dijelaskan secara akademis dan praktis.

## Peta Materi

| No | Materi | Penerapan di aplikasi |
|---|---|---|
| 1 | Tipe data | `int` quantity, `decimal` unit cost, `Guid` ID, enum status |
| 1 | Variabel | `stock`, `newQuantity`, `available`, `pageSize` |
| 1 | Konstanta | role, prefix dokumen, batas pagination |
| 1 | Invarian | stok ≥ 0, quantity > 0, dokumen unik |
| 2 | Flowchart | Stock In approval, Stock Out, sorting/searching |
| 3 | Sorting | `OrderBy` LINQ + Bubble/Merge/Quick |
| 3 | Searching | linear, binary, `FirstOrDefault`, index DB |
| 4 | Prosedur | `StockLedger.ApplyAsync`, `sp_receive_stock` |
| 4 | Fungsi | `Guard.Positive`, `fn_available_stock` |
| 5 | Kompleksitas waktu | O(1), O(log n), O(n), O(n log n), O(n²) |
| 5 | Kompleksitas memori | in-place vs auxiliary, `AsNoTracking`, pagination |

## Kesimpulan Singkat

- Aplikasi memakai tipe data yang sesuai domain: `int` untuk quantity,
  `decimal` untuk uang, `Guid` untuk ID, enum untuk status.
- Invarian utama dijaga di service/ledger dan database constraint.
- Algoritma sorting/searching tersedia melalui LINQ, tetapi pemahaman
  algoritma dasar penting untuk memilih solusi efisien.
- Prosedur dan fungsi dipakai untuk reuse logika (`Guard`, `StockLedger`,
  `DocumentNumberService`) dan di database (`sp_*`, `fn_*`).
- Kompleksitas terbesar ada pada query database dan volume histori, bukan pada
  operasi aritmetika di memori.
