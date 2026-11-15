# MOD-APP-LEDGER — StockLedger.ApplyAsync

| Field | Nilai |
|---|---|
| Module ID | `MOD-APP-LEDGER` |
| File | `src/Inventory.Application/Services/StockLedger.cs` |
| Method ID | `MTH-APP-LEDGER-APPLY` |
| Kategori | Prosedur (side effect) |
| Parameter | `IEnumerable<StockChange> changes`, `CancellationToken ct` |
| Return | `Task` |
| Eksepsi | `ValidationException` |

```csharp
public async Task ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct = default)
{                                                                       // awal prosedur
    foreach (var change in changes)                                     // iterasi perubahan
    {                                                                   // buka blok loop
        if (change.Quantity == 0)                                       // qty nol?
            continue;                                                   // lewati, tidak ada mutasi

        var stock = await db.Stocks.FirstOrDefaultAsync(                // cari saldo existing
            x => x.ProductId == change.ProductId && x.LocationId == change.LocationId, ct)
            ?? db.Stocks.Local.FirstOrDefault(                          // cek entity yang belum disimpan
                x => x.ProductId == change.ProductId && x.LocationId == change.LocationId);

        if (stock is null)                                              // saldo belum ada?
        {                                                               // buat baru
            stock = new Stock { ProductId = change.ProductId, LocationId = change.LocationId, Quantity = 0 };
            db.Stocks.Add(stock);                                       // tambahkan ke context
        }                                                               // akhir if

        var newQuantity = stock.Quantity + change.Quantity;             // hitung saldo baru
        if (newQuantity < 0)                                            // cek invarian stok >= 0
        {                                                               // jika negatif
            var product = await db.Products.Where(x => x.Id == change.ProductId).Select(x => x.Name).FirstOrDefaultAsync(ct);
            var location = await db.Locations.Where(x => x.Id == change.LocationId).Select(x => x.Name).FirstOrDefaultAsync(ct);
            throw new ValidationException($"Stok tidak mencukupi untuk produk '{product}' di lokasi '{location}'.");
        }                                                               // akhir if

        stock.Quantity = newQuantity;                                   // simpan saldo baru
        stock.UpdatedAt = DateTime.UtcNow;                              // update timestamp
        stock.Version++;                                                // naikkan concurrency version

        db.StockMovements.Add(new StockMovement                          // catat ledger movement
        {                                                               // buka object initializer
            MovementNo = await documents.NextAsync("MOV", DateTime.UtcNow, ct), // nomor movement
            Type = change.Type,                                         // tipe movement
            ProductId = change.ProductId,                               // produk
            LocationId = change.LocationId,                             // lokasi
            Quantity = change.Quantity,                                 // delta qty
            ReferenceType = change.ReferenceType,                       // tipe referensi
            ReferenceId = change.ReferenceId,                           // id dokumen referensi
            Notes = change.Notes,                                       // catatan
            CreatedAt = DateTime.UtcNow,                                // waktu dibuat
            CreatedById = change.UserId                                 // user pelaku
        });                                                             // tutup initializer
    }                                                                   // akhir loop
}                                                                       // akhir prosedur
```

## Eksepsi

- `ValidationException` jika saldo baru negatif.

## Kompleksitas

- Waktu: O(k) untuk k perubahan.
- Memori: O(1) tambahan per perubahan.
