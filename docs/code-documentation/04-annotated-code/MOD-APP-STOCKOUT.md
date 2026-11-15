# MOD-APP-STOCKOUT — Validasi Baris & Stok

| Field | Nilai |
|---|---|
| Module ID | `MOD-APP-STOCKOUT` |
| File | `src/Inventory.Application/Services/StockOutService.cs` |
| Method ID | `MTH-APP-STOCKOUT-VALIDATE-LINES` |
| Kategori | Fungsi validasi (tanpa side effect) |
| Parameter | `productIds`, `locationId`, `stockLines`, `ct` |
| Eksepsi | `ValidationException` |

```csharp
private async Task ValidateLinesAsync(
    IReadOnlyCollection<Guid> productIds,                               // daftar produk
    Guid locationId,                                                    // lokasi stok
    IReadOnlyCollection<DocumentLineInput>? stockLines,                 // null = tidak cek stok
    CancellationToken ct)                                               // token batal
{                                                                       // awal fungsi
    if (productIds.Count != productIds.Distinct().Count())              // ada duplikat?
        throw new ValidationException("Terdapat produk duplikat pada detail.");

    var activeCount = await db.Products.CountAsync(                     // hitung produk aktif
        x => productIds.Contains(x.Id) && x.IsActive, ct);
    if (activeCount != productIds.Count)                               // ada produk hilang/nonaktif?
        throw new ValidationException("Terdapat produk yang tidak ditemukan atau tidak aktif.");

    if (stockLines is null)                                            // perlu cek stok?
        return;                                                         // tidak, selesai

    foreach (var line in stockLines)                                   // per baris
    {                                                                   // buka loop
        var available = await db.Stocks                                 // ambil saldo
            .Where(x => x.ProductId == line.ProductId && x.LocationId == locationId)
            .Select(x => x.Quantity)
            .FirstOrDefaultAsync(ct);                                   // 0 jika tidak ada
        if (available < line.Quantity)                                  // stok kurang?
            throw new ValidationException($"Stok tidak mencukupi untuk salah satu produk (tersedia {available}).");
    }                                                                   // tutup loop
}                                                                       // akhir fungsi
```

## Eksepsi

- Duplikat produk.
- Produk tidak ditemukan/nonaktif.
- Stok tidak mencukupi.

## Kompleksitas

- Waktu: O(n) query count + O(n) query stok.
- Memori: O(1) tambahan.
