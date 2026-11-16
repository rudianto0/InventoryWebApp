# 04 - Operasi Basis Data

## 4a. Simpan dan Ubah Data

### Melalui aplikasi (EF Core)

```csharp
var entity = new Product { Sku = "SKU-010", Name = "Notebook", ... };
db.Products.Add(entity);
await db.SaveChangesAsync(ct);
```

- `SaveChangesAsync` menulis perubahan dalam satu transaksi.
- Audit otomatis mengisi `CreatedAt`, `CreatedById`, `UpdatedAt`, `UpdatedById`.
- Soft delete mengubah `Deleted` menjadi `IsDeleted = true`.
- `AuditLogs` ditulis setelah perubahan utama.

### Melalui SQL (DML)

Contoh ekuivalen:

```sql
INSERT INTO "Products" ("Id", "Sku", "Name", "CategoryId", "UnitId",
                        "ReorderLevel", "IsActive", "CreatedAt", "IsDeleted")
VALUES (gen_random_uuid(), 'SKU-010', 'Notebook',
        '<category-id>', '<unit-id>', 5, true, now(), false);

UPDATE "Products" SET "Name" = 'Notebook A5' WHERE "Sku" = 'SKU-010';

DELETE FROM "Products" WHERE "Sku" = 'SKU-010';
```

Script lengkap: `../../02-database-implementation/sql/02-seed-data/02-seed-data.sql`,
`05-relational-dml.sql`.

## 4b. Menghasilkan Informasi dengan Query

### Query aplikasi (LINQ)

```csharp
var items = await db.Stocks.AsNoTracking()
    .Where(s => s.LocationId == locationId)
    .Join(db.Products, s => s.ProductId, p => p.Id, (s, p) => new { s, p })
    .Select(x => new StockBalanceItem { ... })
    .OrderBy(x => x.Sku)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(ct);
```

### Query SQL

```sql
-- Saldo per lokasi
SELECT p."Sku", p."Name", u."Name" AS "Unit",
       l."Name" AS "Location", s."Quantity", p."ReorderLevel"
FROM "Stocks" s
JOIN "Products" p ON p."Id" = s."ProductId"
JOIN "Units" u ON u."Id" = p."UnitId"
JOIN "Locations" l ON l."Id" = s."LocationId"
WHERE l."Code" = 'HQ-WH1'
ORDER BY p."Sku";

-- Pending approval
SELECT * FROM "v_pending_approvals" ORDER BY "SubmittedAt";

-- Kartu stok
SELECT * FROM fn_stock_card('<product-id>', CURRENT_DATE - 30, CURRENT_DATE);
```

## 4c. Penggunaan Indeks

### Index yang tersedia

- Unique: `Products.Sku`, `Suppliers.Code`, `Locations.Code`,
  `Stocks(ProductId, LocationId)`, `DocNo` header.
- Filter: unique index dengan `IsDeleted = false`.
- Laporan: `StockMovements(ProductId, LocationId, CreatedAt)`.
- Tambahan: lihat `../../02-database-implementation/sql/03-index-and-view/03-indexes.sql`.

### Melihat penggunaan index

```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT *
FROM "StockMovements"
WHERE "ProductId" = '<product-id>'
ORDER BY "CreatedAt" DESC
LIMIT 50;
```

Yang diperhatikan:

- `Index Scan`/`Index Only Scan` → index dipakai.
- `Seq Scan` pada tabel besar → kandidat index.
- `Buffers: shared hit` tinggi → data dari cache.
- `Sort` besar → pertimbangkan index urutan.

### Kapan index tidak dipakai

- Filter tidak selektif.
- Tabel sangat kecil.
- Fungsi pada kolom (`lower("Sku")`) tanpa expression index.
- Statistik usang; jalankan `ANALYZE`.

## 4d. Ringkasan Perintah

| Operasi | Aplikasi | SQL |
|---|---|---|
| Insert | `DbSet.Add` + `SaveChanges` | `INSERT` |
| Update | ubah entity + `SaveChanges` | `UPDATE` |
| Delete | `Remove` (soft delete) | `UPDATE IsDeleted` / `DELETE` |
| Read | LINQ async | `SELECT` |
| Aggregasi | `GroupBy`, `Sum` | `GROUP BY`, `SUM` |
| Pagination | `Skip/Take` | `LIMIT/OFFSET` |
| Index | migration/config | `CREATE INDEX` |
