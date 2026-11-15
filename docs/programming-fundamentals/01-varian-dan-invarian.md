# 01 - Varian dan Invarian

## 1.1 Tipe Data yang Sesuai Kaidah

### Prinsip

- Pilih tipe yang **merepresentasikan domain** dan **mencegah nilai tidak valid**.
- Gunakan tipe terkecil yang aman, tetapi jangan mengorbankan presisi.
- Gunakan enum untuk himpunan nilai tetap.
- Gunakan nullable hanya jika data memang boleh kosong.
- Gunakan UTC untuk waktu.

### Tipe data di aplikasi

| Tipe | Contoh field | Alasan |
|---|---|---|
| `int` | `Stock.Quantity`, `ReorderLevel`, `PageSize` | Bilangan bulat, hemat memori |
| `decimal` | `StockInDetail.UnitCost` | Presisi uang, hindari floating error |
| `Guid` | `Product.Id`, `Stock.Id` | Unik, time-ordered |
| `DateTime` | `CreatedAt`, `ApprovedAt` | Waktu UTC |
| `bool` | `IsActive`, `IsDeleted` | Status biner |
| `string` | `Sku`, `Name` | Teks |
| `enum` | `ApprovalStatus`, `MovementType`, `LocationType` | Nilai terbatas |
| Nullable | `int? CreatedById`, `DateTime? ApprovedAt` | Opsional |
| Koleksi | `IReadOnlyList<T>`, `PagedResult<T>` | Kontrak read-only/paging |

### Contoh C#

```csharp
public class Stock
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public int Quantity { get; set; }
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class StockInDetail
{
    public int Quantity { get; set; }          // > 0
    public decimal? UnitCost { get; set; }     // presisi uang
}

public enum ApprovalStatus { Draft, Submitted, Approved, Rejected, Cancelled }
```

### Pseudocode

```
CONSTANT MAX_PAGE_SIZE = 200
TYPE Stock:
  Id: UUID
  ProductId: UUID
  LocationId: UUID
  Quantity: INTEGER
  Version: INTEGER
END TYPE

TYPE StockInDetail:
  Quantity: INTEGER      // harus > 0
  UnitCost: DECIMAL?     // boleh kosong
END TYPE
```

### Kesalahan yang Dihindari

| Kesalahan | Dampak | Perbaikan |
|---|---|---|
| `double` untuk uang | Pembulatan tidak akurat | `decimal` |
| `string` untuk status | Nilai liar | `enum` |
| `int` untuk ID global | Bentrok | `Guid` |
| Nullable berlebihan | Null reference | Wajibkan nilai |

## 1.2 Variabel yang Sesuai Kaidah

### Aturan Penamaan dan Scope

- Lokal: `camelCase` (`newQuantity`, `available`, `pageSize`).
- Properti/kelas: `PascalCase` (`Quantity`, `StockLedger`).
- Parameter: `camelCase` bermakna (`productId`, `locationId`).
- Hindari singkatan tidak jelas (`x`, `tmp`, `data1`).
- Deklarasikan sedekat mungkin dengan penggunaan.
- Gunakan `readonly`/`init` bila tidak berubah.

### Contoh C#

```csharp
var stock = await db.Stocks.FirstOrDefaultAsync(
    x => x.ProductId == change.ProductId && x.LocationId == change.LocationId, ct);

if (stock is null)
    stock = new Stock { ProductId = change.ProductId, LocationId = change.LocationId };

var newQuantity = stock.Quantity + change.Quantity;
if (newQuantity < 0)
    throw new ValidationException("Stok tidak mencukupi.");

stock.Quantity = newQuantity;
```

### Pseudocode

```
READ change
stock ← FIND Stock BY (change.ProductId, change.LocationId)
IF stock = NULL THEN
    stock ← NEW Stock(productId, locationId, quantity=0)
END IF

newQuantity ← stock.Quantity + change.Quantity
IF newQuantity < 0 THEN
    THROW ValidationException("Stok tidak mencukupi")
END IF

stock.Quantity ← newQuantity
```

### Variabel di kode lain

| Variabel | Lokasi | Fungsi |
|---|---|---|
| `pageSize` | Service/Report | ukuran halaman, di-clamp 1–200 |
| `available` | StockOut/Transfer | saldo tersedia |
| `opening` | ReportService | saldo awal kartu stok |
| `running` | ReportService | saldo berjalan |
| `approverId` | ApprovalGuard | ID atasan |
| `term` | Catalog search | kata kunci yang di-trim |

## 1.3 Konstanta yang Sesuai Kaidah

### Jenis Konstanta

| Jenis | Kapan dipakai |
|---|---|
| `const` | Nilai compile-time: string/number primitive |
| `static readonly` | Objek yang tidak berubah setelah inisialisasi |
| Enum | Himpunan nilai tetap |

### Kandidat konstanta di aplikasi

| Nilai | Saat ini | Rekomendasi |
|---|---|---|
| `"Admin"`, `"SPV"`, `"Staff"`, `"Manager"`, `"Auditor"` | Literal string | `RoleNames` constants |
| `"IN"`, `"OUT"`, `"TRF"`, `"ADJ"`, `"OPN"` | Literal | `DocumentPrefixes` |
| `1..200` page size | Literal | `Paging.Min/Max` |
| `8` karakter password | Literal | `UserRules.MinPasswordLength` |
| `500` max notes | Konfigurasi EF | `FieldLimits.Notes` |

### Contoh C#

```csharp
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Spv = "SPV";
    public const string Staff = "Staff";
    public const string Manager = "Manager";
    public const string Auditor = "Auditor";
}

public static class Paging
{
    public const int MinPageSize = 1;
    public const int MaxPageSize = 200;
    public const int DefaultPageSize = 20;
}

var pageSize = Math.Clamp(filter.PageSize, Paging.MinPageSize, Paging.MaxPageSize);
```

### Pseudocode

```
CONSTANT MIN_PAGE_SIZE ← 1
CONSTANT MAX_PAGE_SIZE ← 200

FUNCTION ClampPageSize(size):
    IF size < MIN_PAGE_SIZE THEN RETURN MIN_PAGE_SIZE
    IF size > MAX_PAGE_SIZE THEN RETURN MAX_PAGE_SIZE
    RETURN size
END FUNCTION
```

## Invarian Sistem

Invarian adalah kondisi yang **selalu benar** sebelum dan sesudah operasi.

| Invarian | Dijaga oleh |
|---|---|
| `Stock.Quantity >= 0` | `StockLedger`, trigger DB |
| `Detail.Quantity > 0` | `Guard.Positive`, trigger DB |
| SKU/kode unik (aktif) | Unique filtered index + validasi service |
| Status approval valid | Enum + `ApprovalGuard` |
| `ApprovedAt` terisi saat Approved | `ApprovalGuard.MarkApproved` |
| Nomor dokumen unik per prefix/bulan | Unique index + `DocumentSequences` |
| Audit user terisi saat create/update/delete | `AppDbContext.SaveChangesAsync` |
| Soft delete tidak muncul di query | Global query filter |

### Contoh penjagaan invarian

```csharp
Guard.Positive(line.Quantity, "Quantity");        // invarian quantity > 0
if (newQuantity < 0)                              // invarian stok >= 0
    throw new ValidationException("Stok tidak mencukupi.");
```

### Pseudocode invarian

```
PROCEDURE ApplyStockChange(change):
    ASSERT change.Quantity <> 0
    stock ← FIND_OR_CREATE(change.ProductId, change.LocationId)
    newQty ← stock.Quantity + change.Quantity
    ASSERT newQty >= 0
    stock.Quantity ← newQty
    CREATE Movement(...)
END PROCEDURE
```

## Ringkasan

- Tipe data harus mewakili domain dan mencegah nilai tidak valid.
- Variabel diberi nama jelas, scope minimal, dan sedapat mungkin immutable.
- Konstanta menghilangkan magic value dan memudahkan perubahan.
- Invarian dijaga berlapis: validasi aplikasi + constraint/index database.
