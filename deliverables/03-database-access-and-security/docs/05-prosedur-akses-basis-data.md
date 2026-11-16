# 05 - Prosedur Akses Basis Data

## 5a. Library Akses Basis Data

### EF Core + Npgsql (utama)

- `AppDbContext` mewarisi `IdentityDbContext<AppUser, AppRole, int>`.
- `IAppDbContext` mengekspos `DbSet` untuk Application layer.
- Provider dipilih melalui `Database:Provider`.
- Migration assembly dipilih otomatis (PostgreSQL/SQL Server).

Contoh query:

```csharp
var products = await db.Products.AsNoTracking()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Sku)
    .Select(p => new ProductListItem { ... })
    .ToListAsync(ct);
```

### ADO.NET Npgsql (untuk kasus khusus)

Dipakai bila butuh kontrol penuh, bulk operation, atau query PostgreSQL-specific.

```csharp
await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync(ct);
await using var command = new NpgsqlCommand(
    "SELECT fn_available_stock(@product, @location)", connection);
command.Parameters.AddWithValue("product", productId);
command.Parameters.AddWithValue("location", locationId);
var qty = (int)await command.ExecuteScalarAsync(ct);
```

### Dapper (opsional)

Belum dipakai. Cocok untuk query read-only kompleks, tetapi menambah dependency
dan mapping manual.

## 5b. Perintah Akses Data yang Relevan

| Kebutuhan | Perintah |
|---|---|
| CRUD | LINQ `Add/Update/Remove` + `SaveChangesAsync` |
| Query view | `FromSqlInterpolated` / `SqlQuery<T>` |
| Function/procedure | `SELECT fn_...`, `CALL sp_...` |
| Batch DML | `ExecuteSqlRawAsync` / `ExecuteSqlInterpolatedAsync` |
| Transaksi | `BeginTransactionAsync` |
| Pagination | `Skip/Take` |
| Aggregasi | `GroupBy`, `SumAsync`, `CountAsync` |

### Query ke view

```csharp
var pending = await db.Database
    .SqlQuery<ApprovalItem>($"SELECT * FROM \"v_pending_approvals\"")
    .ToListAsync(ct);
```

### Memanggil function

```csharp
var available = await db.Database
    .SqlQuery<int>($"SELECT fn_available_stock({productId}, {locationId})")
    .SingleAsync(ct);
```

### Memanggil procedure

```csharp
await db.Database.ExecuteSqlInterpolatedAsync(
    $"CALL sp_issue_stock(NULL, {locationId}, {productId}, {qty}, {userId})", ct);
```

## 5c. Tipe Data PostgreSQL

| Tipe | Penggunaan |
|---|---|
| `uuid` | ID bisnis |
| `integer` | quantity, reorder level, user ID |
| `numeric(18,4)` | unit cost |
| `timestamptz` | timestamp |
| `jsonb` | kandidat untuk `AuditLogs.OldValues/NewValues` |

Catatan: saat ini `AuditLogs.OldValues/NewValues` bertipe `text` berisi JSON.
Jika query JSON diperlukan, pertimbangkan migrasi ke `jsonb` dan index GIN.

## 5d. Praktik Aman

- Selalu gunakan parameter/interpolasi aman, jangan string concat.
- Batasi `CommandTimeout` untuk laporan berat.
- Gunakan `AsNoTracking` untuk read-only.
- Gunakan `CancellationToken`.
- Hindari `SELECT *` pada tabel besar.
- Untuk export/streaming, proses per batch.
