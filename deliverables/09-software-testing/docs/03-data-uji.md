# 03 - Data Uji

## 1. Identifikasi Data Uji Unit

| Sumber | Isi |
|---|---|
| `TestDatabase.SeedBasicsAsync` | category, unit, product, HQ/WH1/WH2, supplier |
| `SeedStockAsync` | saldo per produk/lokasi |
| `SeedLocationAsync` | lokasi aktif/nonaktif + parent |
| `SeedProductAsync` | produk aktif/nonaktif |
| `IdentityFixture` | user, role, password |
| `FakeCurrentUser` | user login + role |
| `FakeUserDirectory` | parent/active/display name |
| `FakeDocumentNumbers` | nomor dokumen deterministik |

## 2. Data Dasar

```text
Category: General
Unit    : PCS
Product : SKU-1 / Product 1
Branch  : HQ
Warehouse: WH1, WH2
Supplier: SUP
```

## 3. Variasi Data

### Valid
- Quantity 5, product aktif, lokasi aktif, supplier aktif.
- User Staff dengan parent SPV aktif.

### Boundary
- Quantity 1.
- Page size 1 dan 200.
- Stok tepat sama dengan quantity.

### Invalid
- Empty lines.
- Quantity 0 / negatif.
- SKU/kode duplikat.
- Supplier/lokasi/produk nonaktif.
- Parent user tidak ada / nonaktif / diri sendiri.
- Counted quantity negatif.

### State
- Produk soft-deleted.
- Lokasi nonaktif.
- Dokumen Draft/Submitted/Approved/Rejected.

### Authorization
- Staff, SPV, Admin.
- Owner vs non-owner.
- Approver vs non-approver.

### Concurrency
- Dua perubahan stok pada `(ProductId, LocationId)` sama.

## 4. Generate Data

- Setiap test membuat `SqliteConnection("DataSource=:memory:")`.
- `EnsureCreated` membuat schema.
- Seed via helper (bukan file SQL).
- ID tetap untuk data yang perlu referensi.
- `FakeDocumentNumbers` menghindari ketergantungan sequence DB.

## 5. Aturan Data

- Tidak memakai data produksi.
- Tidak menyimpan PII.
- Data dibersihkan saat fixture dispose.
- Nilai waktu memakai UTC.

## 6. Contoh Test Data

| Test | Data |
|---|---|
| Stock Out insufficient | stok 2, request 5 |
| Approval non-approver | creator 10, approver 20, actor 99 |
| Duplicate SKU | `SKU-1` dua kali |
| Soft delete product | product + stock, lalu IsDeleted |
| Page size 500 | clamp menjadi 200 |

## 7. Gap Data

- Belum ada data volume besar (100k movement) di unit test.
- Belum ada data PostgreSQL nyata.
- Belum ada data multi-cabang skala besar.
