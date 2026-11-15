# 04 - Prosedur dan Fungsi

## 4.1 Konsep Penggunaan Ulang

Prinsip **DRY (Don't Repeat Yourself)**: logika yang sama tidak ditulis berulang.

Contoh reuse di aplikasi:

| Komponen | Dipakai oleh | Tujuan |
|---|---|---|
| `Guard.Required` | semua service | validasi field wajib |
| `Guard.Positive` | Stock In/Out/Transfer | quantity > 0 |
| `Guard.NotNegative` | Adjustment/UnitCost | nilai tidak negatif |
| `StockLedger.ApplyAsync` | 5 service transaksi | satu jalur mutasi stok |
| `ApprovalGuard` | 5 header transaksi | aturan approval konsisten |
| `DocumentNumberService` | semua create dokumen | format nomor konsisten |
| `IUserDirectory` | approval/list | lookup user |
| `AppDbContext.SaveChangesAsync` | semua service | audit + soft delete |

Manfaat:

- Mengurangi duplikasi.
- Perubahan aturan hanya di satu tempat.
- Lebih mudah diuji.
- Konsistensi perilaku.

## 4.2 Prosedur

Definisi: rutin yang melakukan **aksi/efek samping**, boleh mengubah state.

### C# — `StockLedger.ApplyAsync`

```csharp
public async Task ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct = default)
{
    foreach (var change in changes)
    {
        if (change.Quantity == 0) continue;

        var stock = await db.Stocks.FirstOrDefaultAsync(...)
            ?? new Stock { ... };

        var newQuantity = stock.Quantity + change.Quantity;
        if (newQuantity < 0)
            throw new ValidationException("Stok tidak mencukupi.");

        stock.Quantity = newQuantity;
        stock.Version++;

        db.StockMovements.Add(new StockMovement { ... });
    }
}
```

Ciri prosedur:

- Mengubah `Stocks` dan `StockMovements`.
- Tidak mengembalikan nilai domain (hanya `Task`).
- Biasanya dipanggil dalam transaksi.

### Pseudocode

```
PROCEDURE ApplyStockChanges(changes):
    FOR EACH change IN changes DO
        IF change.Quantity = 0 THEN CONTINUE
        stock ← FIND_OR_CREATE(change.ProductId, change.LocationId)
        newQty ← stock.Quantity + change.Quantity
        IF newQty < 0 THEN THROW "Stok tidak mencukupi"
        stock.Quantity ← newQty
        stock.Version ← stock.Version + 1
        INSERT StockMovement(change)
    END FOR
END PROCEDURE
```

### SQL — stored procedure

```sql
CALL sp_receive_stock(
    NULL,
    '<supplier-id>',
    '<location-id>',
    '<product-id>',
    5,
    1
);
```

`sp_receive_stock` melakukan insert header/detail, update stok, dan movement.

## 4.3 Fungsi

Definisi: rutin yang **mengembalikan nilai**, idealnya tanpa efek samping.

### C# — `Guard`

```csharp
public static void Positive(int value, string field)
{
    if (value <= 0)
        throw new ValidationException($"{field} harus lebih besar dari 0.");
}
```

Walaupun melempar exception, fungsi ini tidak mengubah state.

### C# — `ReportService.GetStockBalanceAsync`

```csharp
public async Task<PagedResult<StockBalanceItem>> GetStockBalanceAsync(
    ReportFilter filter, CancellationToken ct = default)
{
    var query = JoinStocks(db.Stocks.AsNoTracking());
    // ... filter ...
    return new PagedResult<StockBalanceItem> { ... };
}
```

### SQL — function

```sql
SELECT fn_available_stock('<product-id>', '<location-id>') AS "Available";

SELECT fn_next_document_number('IN', CURRENT_DATE) AS "DocNo";

SELECT * FROM fn_stock_balance('<product-id>', '<location-id>');
```

### Pseudocode

```
FUNCTION AvailableStock(productId, locationId):
    stock ← FIND Stock BY (productId, locationId)
    IF stock = NULL THEN RETURN 0
    RETURN stock.Quantity
END FUNCTION

FUNCTION NextDocumentNumber(prefix, date):
    sequence ← INCREMENT DocumentSequences(prefix, year, month)
    RETURN prefix + '-' + FORMAT(date,'YYYYMM') + '-' + PAD(sequence, 4)
END FUNCTION
```

## 4.4 Perbandingan Prosedur vs Fungsi

| Aspek | Prosedur | Fungsi |
|---|---|---|
| Return | Tidak wajib | Wajib |
| Efek samping | Boleh | Sebaiknya tidak |
| Contoh C# | `ApplyAsync`, `ApproveAsync` | `Guard.Positive`, `NextAsync` |
| Contoh SQL | `sp_receive_stock` | `fn_available_stock` |
| Pemanggilan | `CALL` | `SELECT` |

## 4.5 Kapan Memakai

- **Prosedur**: alur bisnis yang mengubah beberapa tabel/state.
- **Fungsi**: perhitungan, validasi, lookup, query read-only.
- **Hindari** prosedur besar yang melakukan terlalu banyak hal; pecah menjadi
  fungsi/prosedur kecil yang dapat diuji.
