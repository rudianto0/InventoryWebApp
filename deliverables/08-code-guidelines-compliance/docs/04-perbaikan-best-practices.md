# 04 - Perbaikan Best Practices

## 1. Best Practice yang Sudah Diterapkan

| Praktik | Bukti |
|---|---|
| Async/await end-to-end | Service & EF Core |
| `CancellationToken` | Parameter service/controller |
| DTO/response di boundary | `Inventory.Contracts` |
| Validasi terpusat | `Guard` |
| Transaksi multi-tabel | `BeginTransactionAsync` |
| Audit & soft delete | `AppDbContext` |
| Pagination | `PagedResult`, `Skip/Take` |
| `AsNoTracking` | Query baca |
| Unit test | 171 test |
| XML doc generation | `GenerateDocumentationFile` |
| `.editorconfig` | Root |

## 2. Rekomendasi Best Practice

### Keamanan
- Pindahkan connection string ke User Secrets/env var.
- Aktifkan TLS PostgreSQL.
- Gunakan role DB minimal (`inventory_app`).

### Performa
- Batch query stok/approval.
- Pagination stock card.
- Atomic document number.
- Cache validasi user.

### Maintainability
- Constants untuk role/prefix/pagination.
- Pisahkan class besar.
- XML comments public API.
- Analyzer/CI style check.

### Observability
- Health check.
- Structured logging.
- Metrics/tracing.
- Query tag EF Core.

## 3. Contoh Penerapan

### Query read-only
```csharp
var items = await db.Products.AsNoTracking()
    .Where(x => x.IsActive)
    .OrderBy(x => x.Sku)
    .Select(x => new ProductListItem { ... })
    .ToListAsync(ct);
```

### Validasi
```csharp
Guard.Positive(line.Quantity, "Quantity");
```

### Transaksi
```csharp
await using var transaction = await db.BeginTransactionAsync(ct);
try { ...; await transaction.CommitAsync(ct); }
catch { await transaction.RollbackAsync(ct); throw; }
```

## 4. Checklist Review

- [x] Async untuk I/O.
- [x] Nullable aktif.
- [x] Tidak ada secret di source code/dokumen.
- [x] Build & test hijau.
- [x] DTO tidak membocorkan entity.
- [x] Exception bisnis memakai `ValidationException`.
- [ ] Semua public API ber-XML comment.
- [ ] Semua query kritis berbatch.
- [ ] Stock card berpagination.
