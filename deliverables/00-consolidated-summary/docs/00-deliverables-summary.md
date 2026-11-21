# Rangkuman Terpadu 10 Deliverable + Bukti Implementasi

Dokumen ini merangkum **10 deliverable** aplikasi Inventory menjadi satu
penjelasan, lalu menunjukkan **function, code, query, dan objek database** yang
benar-benar ada di implementasi beserta lokasinya.

## 1. Ringkasan Satu Halaman

| No | Deliverable | Inti | Bukti utama | Status |
|---|---|---|---|---|
| 01 | Scalability Analysis | Analisis beban, lingkungan, arsitektur | `DependencyInjection.cs:19-28`, `Program.cs:29` | Sesuai arsitektur |
| 02 | Database Implementation | DDL, seed, index, view, SP, function, trigger | `deliverables/02/sql/**` | Terverifikasi |
| 03 | Database Access & Security | EF Core, koneksi, role, performa | `AppDbContext.cs:265`, `DependencyInjection.cs:74` | Terverifikasi sebagian |
| 04 | Programming Fundamentals | Sorting/searching, flowchart, kompleksitas | Latihan terpisah, bukan kode app | Tidak di app |
| 05 | Code Documentation | Modul, method, annotated code, DocFX | `deliverables/05/docfx.json`, `api-metadata/` | Terverifikasi |
| 06 | Code Preparation & Debugging | Build, debug, error log, perbaikan | `.editorconfig`, `ApprovalGuard.cs:9` | Terverifikasi |
| 07 | Performance Profiling | Waktu, memori, bottleneck, benchmark | `tests/Inventory.Benchmarks/Program.cs` | Terverifikasi dengan catatan |
| 08 | Code Guidelines Compliance | Kesesuaian aturan, perbaikan, pengecualian | `.editorconfig`, `AppControllerBase.cs:18` | Terverifikasi |
| 09 | Software Testing | Skenario, data, hasil, coverage | `tests/Inventory.UnitTests/**` | Terverifikasi |
| 10 | Integration Testing | Uji integrasi modul + lembar hasil + third party | `sheets/result-sheet.csv`, `sheets/result-sheet-third-party.csv` | Terverifikasi dengan catatan |

## 2. Peta Arsitektur

```text
Browser (Razor View)
   |
   v
Controller (Inventory.Web)
   |
   v
Inventory.Client (in-process)
   |
   v
Application Service (Inventory.Application)
   |
   v
IAppDbContext / EF Core
   |
   v
PostgreSQL atau SQL Server
```

Provider database dipilih lewat konfigurasi `Database:Provider`:
- `PostgreSql` -> Npgsql
- `SqlServer` -> Microsoft.EntityFrameworkCore.SqlServer

Kutipan implementasi (`src/Inventory.Infrastructure/DependencyInjection.cs:19-29`):

```csharp
var provider = configuration["Database:Provider"] ?? "SqlServer";
var connection = configuration.GetConnectionString("Default")
    ?? configuration["Database:ConnectionString"]
    ?? throw new InvalidOperationException("Connection string 'Default' belum dikonfigurasi.");

services.AddDbContext<AppDbContext>(options =>
{
    if (string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(connection, b => b.MigrationsAssembly("Inventory.Migrations.PostgreSql"));
    else
        options.UseSqlServer(connection, b => b.MigrationsAssembly("Inventory.Migrations.SqlServer"));
});
```

## 3. Bukti per Deliverable

### 01 — Scalability Analysis

Isi: analisis lingkup, lingkungan operasi, masalah skalabilitas, beban kerja,
perangkat keras, dan hasil analisis.

Bukti implementasi:
- Routing MVC: `src/Inventory.Web/Program.cs:29`.
- Pemilihan provider DB: `DependencyInjection.cs:19-28` (kutipan di atas).
- Modul/controller: `src/Inventory.Web/Controllers/*`.
- Use case/service: `src/Inventory.Application/Services/*`.
- Entity/domain: `src/Inventory.Domain/Entities/*`.
- Infrastruktur: `src/Inventory.Infrastructure/Persistence/AppDbContext.cs`.

Catatan: angka pengguna/transaksi di deliverable 01 adalah estimasi, bukan hasil
load test. Yang terverifikasi adalah arsitektur dan komponen yang dijelaskan.

### 02 — Database Implementation

Isi: PDM/DDL, seed data, indeks, view, stored procedure, function, trigger, dan
commit/rollback.

Jumlah objek yang benar-benar ada:
- 27 tabel di `sql/01-migration/01-schema.sql`
- 7 view di `sql/03-index-and-view/04-views.sql`
- 5 stored procedure di `sql/05-stored-procedure/06-stored-procedures.sql`
- 5 function di `sql/06-function/07-functions.sql`
- 8 trigger di `sql/07-trigger/08-triggers.sql`
- transaksi di `sql/08-transaction/09-transactions.sql`

Contoh tabel stok (`sql/01-migration/01-schema.sql:539-552`):

```sql
CREATE TABLE "Stocks" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "LocationId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "Version" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedById" integer,
    "UpdatedById" integer,
    CONSTRAINT "PK_Stocks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Stocks_Locations_LocationId" FOREIGN KEY ("LocationId")
        REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Stocks_Products_ProductId" FOREIGN KEY ("ProductId")
        REFERENCES "Products" ("Id") ON DELETE RESTRICT
);
```

Contoh view saldo stok (`sql/03-index-and-view/04-views.sql:13-32`):

```sql
CREATE OR REPLACE VIEW "v_stock_balance" AS
SELECT
    s."Id"           AS "StockId",
    p."Id"           AS "ProductId",
    p."Sku"          AS "Sku",
    p."Name"         AS "ProductName",
    u."Name"         AS "UnitName",
    l."Id"           AS "LocationId",
    l."Code"         AS "LocationCode",
    l."Name"         AS "LocationName",
    l."BranchCode"   AS "BranchCode",
    s."Quantity"     AS "Quantity",
    p."ReorderLevel" AS "ReorderLevel",
    s."UpdatedAt"    AS "UpdatedAt"
FROM "Stocks" s
JOIN "Products" p ON p."Id" = s."ProductId"
JOIN "Units" u ON u."Id" = p."UnitId"
JOIN "Locations" l ON l."Id" = s."LocationId"
WHERE p."IsDeleted" = false
  AND l."IsDeleted" = false;
```

Contoh function nomor dokumen atomic (`sql/06-function/07-functions.sql:23-43`):

```sql
CREATE OR REPLACE FUNCTION fn_next_document_number(
    p_prefix text,
    p_date date DEFAULT CURRENT_DATE
) RETURNS text
LANGUAGE plpgsql
AS $$
DECLARE
    v_prefix text := upper(trim(p_prefix));
    v_year integer := EXTRACT(YEAR FROM p_date)::int;
    v_month integer := EXTRACT(MONTH FROM p_date)::int;
    v_num integer;
BEGIN
    INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
    VALUES (v_prefix, v_year, v_month, 1)
    ON CONFLICT ("Prefix", "Year", "Month")
    DO UPDATE SET "LastNumber" = "DocumentSequences"."LastNumber" + 1
    RETURNING "LastNumber" INTO v_num;

    RETURN v_prefix || '-' || to_char(p_date, 'YYYYMM') || '-' || lpad(v_num::text, 4, '0');
END;
$$;
```

Contoh trigger stok tidak negatif (`sql/07-trigger/08-triggers.sql:35-51`):

```sql
CREATE OR REPLACE FUNCTION fn_stocks_nonnegative()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."Quantity" < 0 THEN
        RAISE EXCEPTION 'Saldo stok tidak boleh negatif (ProductId=%, LocationId=%)',
            NEW."ProductId", NEW."LocationId";
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_stocks_nonnegative
BEFORE INSERT OR UPDATE OF "Quantity" ON "Stocks"
FOR EACH ROW EXECUTE FUNCTION fn_stocks_nonnegative();
```

Contoh commit dan rollback (`sql/08-transaction/09-transactions.sql:9-31`):

```sql
BEGIN;
INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "CreatedAt", "IsDeleted")
VALUES ('30000000-0000-0000-0000-000000000101', 'SUP-TRX-COMMIT', 'Supplier Commit', true, now(), false)
ON CONFLICT ("Id") DO NOTHING;
COMMIT;

BEGIN;
INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "CreatedAt", "IsDeleted")
VALUES ('30000000-0000-0000-0000-000000000102', 'SUP-TRX-ROLLBACK', 'Supplier Rollback', true, now(), false);
ROLLBACK;
```

### 03 — Database Access & Security

Isi: akses data dari aplikasi, koneksi, keamanan, hak akses, dan pengujian
performa.

Yang dipakai aplikasi:
- EF Core + LINQ, bukan raw SQL untuk alur utama.
- `SaveChangesAsync` untuk simpan + audit.
- `CancellationToken` dan `AsNoTracking` pada query baca.
- Pagination `Skip`/`Take`.

Audit dan soft delete (`src/Inventory.Infrastructure/Persistence/AppDbContext.cs:265-301`):

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    if (_auditing)
        return await base.SaveChangesAsync(cancellationToken);

    var snapshots = CaptureAuditSnapshots();
    ApplyAuditAndSoftDelete();
    var result = await base.SaveChangesAsync(cancellationToken);

    if (snapshots.Count > 0)
    {
        _auditing = true;
        try
        {
            foreach (var snapshot in snapshots)
            {
                var entityId = snapshot.EntityId ?? ResolveEntityId(snapshot.Entry);
                AuditLogs.Add(new AuditLog
                {
                    EntityName = snapshot.EntityName,
                    EntityId = entityId,
                    Action = snapshot.Action,
                    OldValues = snapshot.OldValues,
                    NewValues = snapshot.NewValues,
                    UserId = _currentUser.UserId,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _auditing = false;
        }
    }
    return result;
}
```

Validasi principal per request (`DependencyInjection.cs:74-83`):

```csharp
options.Events.OnValidatePrincipal = async context =>
{
    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
    var user = await userManager.GetUserAsync(context.Principal!);
    if (user is null || !user.IsActive)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
};
```

Contoh query EF Core (`src/Inventory.Application/Services/CatalogService.cs:143-165`):

```csharp
var query = db.Products.AsNoTracking()
    .Include(x => x.Category)
    .Include(x => x.Unit)
    .AsQueryable();

if (!string.IsNullOrWhiteSpace(search))
{
    var term = search.Trim();
    query = query.Where(x => x.Sku.Contains(term) || x.Name.Contains(term));
}
if (categoryId.HasValue)
    query = query.Where(x => x.CategoryId == categoryId.Value);
if (activeOnly == true)
    query = query.Where(x => x.IsActive);

return await query.OrderBy(x => x.Sku).Select(x => new ProductListItem
{
    Id = x.Id,
    Sku = x.Sku,
    Name = x.Name,
    CategoryName = x.Category!.Name,
    UnitName = x.Unit!.Name,
    ReorderLevel = x.ReorderLevel,
    IsActive = x.IsActive
}).ToListAsync(ct);
```

Contoh query stok per baris (`src/Inventory.Application/Services/StockOutService.cs:228-246`):

```csharp
private async Task ValidateLinesAsync(
    IReadOnlyCollection<Guid> productIds,
    Guid locationId,
    IReadOnlyCollection<DocumentLineInput>? stockLines,
    CancellationToken ct)
{
    if (productIds.Count != productIds.Distinct().Count())
        throw new ValidationException("Terdapat produk duplikat pada detail.");

    var activeCount = await db.Products.CountAsync(
        x => productIds.Contains(x.Id) && x.IsActive, ct);
    if (activeCount != productIds.Count)
        throw new ValidationException("Terdapat produk yang tidak ditemukan atau tidak aktif.");

    if (stockLines is null)
        return;

    foreach (var line in stockLines)
    {
        var available = await db.Stocks
            .Where(x => x.ProductId == line.ProductId && x.LocationId == locationId)
            .Select(x => x.Quantity).FirstOrDefaultAsync(ct);
        if (available < line.Quantity)
            throw new ValidationException($"Stok tidak mencukupi untuk salah satu produk (tersedia {available}).");
    }
}
```

Role database dan performance test tersedia di:
- `sql/01-roles-and-grants/10-roles-and-grants.sql`
- `sql/02-performance-tests/11-performance-tests.sql`

Catatan: view/function/procedure tersedia di database, tetapi belum dipanggil
langsung oleh kode aplikasi. Aplikasi memakai LINQ/EF Core. Role `inventory_app`
baru tersedia di script, belum dipakai konfigurasi aplikasi saat ini.

### 04 — Programming Fundamentals

Isi 04 adalah latihan terpisah (program menu angka, Bubble Sort, Linear Search).
Kode tersebut **tidak ada** di `src` aplikasi Inventory.

Yang benar-benar diimplementasikan di aplikasi adalah algoritma bisnis:

Mutasi stok (`src/Inventory.Application/Services/StockLedger.cs:22-66`):

```csharp
public async Task ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct = default)
{
    foreach (var change in changes)
    {
        if (change.Quantity == 0)
            continue;

        var stock = await db.Stocks.FirstOrDefaultAsync(
            x => x.ProductId == change.ProductId && x.LocationId == change.LocationId, ct)
            ?? db.Stocks.Local.FirstOrDefault(
                x => x.ProductId == change.ProductId && x.LocationId == change.LocationId);

        if (stock is null)
        {
            stock = new Stock { ProductId = change.ProductId, LocationId = change.LocationId, Quantity = 0 };
            db.Stocks.Add(stock);
        }

        var newQuantity = stock.Quantity + change.Quantity;
        if (newQuantity < 0)
        {
            var product = await db.Products
                .Where(x => x.Id == change.ProductId)
                .Select(x => x.Name).FirstOrDefaultAsync(ct);
            var location = await db.Locations
                .Where(x => x.Id == change.LocationId)
                .Select(x => x.Name).FirstOrDefaultAsync(ct);
            throw new ValidationException(
                $"Stok tidak mencukupi untuk produk '{product}' di lokasi '{location}'.");
        }

        stock.Quantity = newQuantity;
        stock.UpdatedAt = DateTime.UtcNow;
        stock.Version++;

        db.StockMovements.Add(new StockMovement
        {
            MovementNo = await documents.NextAsync("MOV", DateTime.UtcNow, ct),
            Type = change.Type,
            ProductId = change.ProductId,
            LocationId = change.LocationId,
            Quantity = change.Quantity,
            ReferenceType = change.ReferenceType,
            ReferenceId = change.ReferenceId,
            Notes = change.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedById = change.UserId
        });
    }
}
```

Saldo berjalan kartu stok (`src/Inventory.Application/Services/ReportService.cs:123-145`):

```csharp
var movements = await query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
    .Select(x => new
    {
        x.CreatedAt, x.MovementNo, x.Type,
        LocationName = x.Location!.Name, x.Quantity, x.Notes
    }).ToListAsync(ct);

var result = new List<StockCardItem>();
var running = opening;
foreach (var movement in movements)
{
    running += movement.Quantity;
    result.Add(new StockCardItem
    {
        CreatedAt = movement.CreatedAt,
        MovementNo = movement.MovementNo,
        Type = movement.Type,
        LocationName = movement.LocationName,
        Quantity = movement.Quantity,
        RunningBalance = running,
        Notes = movement.Notes
    });
}
```

Deteksi siklus lokasi (`src/Inventory.Application/Services/LocationService.cs:118-131`):

```csharp
private async Task<bool> CreatesCycleAsync(Guid currentId, Guid parentId, CancellationToken ct)
{
    var visited = new HashSet<Guid>();
    var cursor = (Guid?)parentId;
    while (cursor.HasValue)
    {
        if (cursor.Value == currentId)
            return true;
        if (!visited.Add(cursor.Value))
            return true;
        cursor = await db.Locations
            .Where(x => x.Id == cursor.Value)
            .Select(x => x.ParentLocationId)
            .FirstOrDefaultAsync(ct);
    }
    return false;
}
```

### 05 — Code Documentation

Isi: dokumentasi modul, method, parameter, eksepsi, annotated code, dan DocFX.

Konfigurasi DocFX (`deliverables/05-code-documentation/docfx.json`):

```json
{
  "metadata": [
    {
      "src": [ { "src": "../../src", "files": [ "**/*.csproj" ] } ],
      "dest": "api-metadata"
    }
  ],
  "build": {
    "content": [
      { "files": [ "docs/*.md", "docs/*.yml",
                   "docs/04-annotated-code/*.md",
                   "docs/04-annotated-code/*.yml" ] }
    ],
    "dest": "api-site"
  }
}
```

Contoh annotated code (`deliverables/05/docs/04-annotated-code/MOD-APP-LEDGER.md:14-32`):

```csharp
public async Task ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct = default)
{                                                       // awal prosedur
    foreach (var change in changes)                     // iterasi perubahan
    {
        if (change.Quantity == 0)                       // qty nol?
            continue;                                   // lewati, tidak ada mutasi

        var stock = await db.Stocks.FirstOrDefaultAsync( // cari saldo existing
            x => x.ProductId == change.ProductId && x.LocationId == change.LocationId, ct)
            ?? db.Stocks.Local.FirstOrDefault(           // cek entity yang belum disimpan
                x => x.ProductId == change.ProductId && x.LocationId == change.LocationId);

        if (stock is null)                              // saldo belum ada?
        {
            stock = new Stock { ProductId = change.ProductId, LocationId = change.LocationId, Quantity = 0 };
            db.Stocks.Add(stock);                       // tambahkan ke context
        }

        var newQuantity = stock.Quantity + change.Quantity; // hitung saldo baru
        if (newQuantity < 0)                            // cek invarian stok >= 0
            throw new ValidationException("Stok tidak mencukupi.");
    }
}
```

Modul dan method yang didokumentasikan sesuai service yang ada:
`CatalogService`, `SupplierService`, `LocationService`, `StockLedger`,
`StockInService`, `StockOutService`, `StockTransferService`,
`StockAdjustmentService`, `StockOpnameService`, `ApprovalService`,
`ReportService`, dan `AuditQueryService`.

### 06 — Code Preparation & Debugging

Isi: persiapan kode, tools debugging, build, kriteria eksekusi, log kesalahan,
perbaikan, dan verifikasi.

Bukti perbaikan:
- `.editorconfig` ada di root.
- `Class1.cs` sudah tidak ada.
- `ApprovalGuard` dipisah ke file sendiri.
- Komentar pengecualian ada di kode.

`src/Inventory.Application/Services/ApprovalGuard.cs:9-11`:

```csharp
// EX-05: Internal workflow helper. It is intentionally excluded from the public
// API surface and therefore does not require public XML documentation.
internal static class ApprovalGuard
```

`src/Inventory.Web/Controllers/AppControllerBase.cs:18-22`:

```csharp
// EX-06: Temporary Web -> EF dependency until Inventory.Api owns
// transaction/concurrency handling (see ADR 006).
if (ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
    return "Data telah berubah oleh pengguna lain. Muat ulang halaman dan coba lagi.";
```

Hasil verifikasi saat ini:
- Build Release: 0 error, 0 warning.
- Unit test: 171 lulus.

### 07 — Performance Profiling

Isi: pengukuran waktu/memori, identifikasi bottleneck, rancangan solusi, dan
kompleksitas algoritma.

Benchmark yang memanggil service asli (`tests/Inventory.Benchmarks/Program.cs`):

```csharp
[Benchmark]
public async Task Ledger_Apply_100()
{
    var ledger = new StockLedger(_db, new FakeDocumentNumbers());
    var changes = Enumerable.Range(0, 100)
        .Select(_ => new StockChange(
            MovementType.In, ReferenceType.StockIn, Guid.NewGuid(),
            _productId, _locationId, 1, 1, null)).ToList();
    await ledger.ApplyAsync(changes);
    await _db.SaveChangesAsync();
}

[Benchmark]
public async Task Approval_Pending_100()
{
    var service = new ApprovalService(_db, new FakeCurrentUser { UserId = 20 }, new FakeUserDirectory());
    await service.GetPendingAsync();
}

[Benchmark]
public async Task StockCard_10000()
{
    var service = new ReportService(_db);
    await service.GetStockCardAsync(new ReportFilter { ProductId = _productId });
}
```

Benchmark prototipe (bukan method service aplikasi):
- `StockCheck_Naive_100` dan `StockCheck_Batch_100`
- `CreatorLookup_Naive_100` dan `CreatorLookup_Batch_100`

Pola N+1 di aplikasi (`src/Inventory.Application/Services/ApprovalService.cs:74-88`):

```csharp
var creators = new List<int>();
foreach (var item in items)
{
    var createdBy = item.Type switch
    {
        "StockIn" => await db.StockInHeaders
            .Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
        "StockOut" => await db.StockOutHeaders
            .Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
        "StockTransfer" => await db.StockTransferHeaders
            .Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
        "StockAdjustment" => await db.StockAdjustmentHeaders
            .Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct),
        _ => await db.StockOpnameHeaders
            .Where(x => x.Id == item.Id).Select(x => x.CreatedById).FirstOrDefaultAsync(ct)
    };
    creators.Add(createdBy ?? 0);
}
```

Data pengukuran tersedia di:
- `measurements/baseline.csv`
- `measurements/after-optimization.csv`
- `measurements/memory.csv`

### 08 — Code Guidelines Compliance

Isi: evaluasi kesesuaian, perbaikan, dan pengecualian aturan kode.

Contoh aturan yang diterapkan (`.editorconfig:1-17`):

```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

[*.{cs,csx}]
indent_size = 4
dotnet_sort_system_directives_first = true
csharp_style_namespace_declarations = file_scoped:suggestion
csharp_using_directive_placement = outside_namespace:warning
dotnet_diagnostic.IDE0005.severity = suggestion
```

Perbaikan yang sudah diterapkan:
- `Class1.cs` dihapus.
- `ApprovalGuard.cs` dipisah.
- `.editorconfig` ditambahkan.
- Komentar `EX-05` dan `EX-06` ditambahkan.

Temuan yang masih terbuka dan sesuai kondisi kode:
- N+1 pada approval.
- Query stok per baris di StockOut/Transfer.
- Kredensial masih ada di `appsettings.json` lokal.
- Sebagian public API belum memiliki XML comment.

### 09 — Software Testing

Isi: kebutuhan uji, tools, data uji, skenario, pelaksanaan, dan evaluasi.

Perintah uji:

```powershell
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj `
  --logger "trx;LogFileName=test-results.trx" `
  --collect:"XPlat Code Coverage"
```

Hasil aktual:

```text
Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
Line coverage   : 74,69%
Branch coverage : 60,17%
```

Bukti:
- `deliverables/09-software-testing/results/test-results.csv`
- `deliverables/09-software-testing/results/coverage-summary.txt`
- `tests/Inventory.UnitTests/**`

### 10 — Integration Testing

Isi: peralatan, data uji integrasi, pelaksanaan, analisis, laporan, dan arsip.

Skenario INT-01 s.d. INT-10 ada di
`deliverables/10-integration-testing/sheets/result-sheet.csv`. Hasil uji
integrasi third party (EXT-11 s.d. EXT-15, HTTP stub lokal) dicatat terpisah di
`deliverables/10-integration-testing/sheets/result-sheet-third-party.csv`.

Contoh evidence yang sudah cocok dengan test aktual:
- `StockTransactionApprovalTests.ApproveStockIn_AsAssignedApprover_UpdatesStock`
- `StockOutServiceTests.Approve_DecreasesStock`
- `StockTransferServiceTests.Approve_MovesStockAndCreatesTwoMovements`
- `StockAdjustmentServiceTests.Approve_AppliesNegativeDifference`
- `StockOpnameServiceTests.Approve_AppliesDifference`
- `StockTransactionApprovalTests.ApproveStockIn_ByNonApprover_IsRejected`
- `SoftDeleteTests.Delete_MasterData_IsSoftDeletedAndHiddenFromQueries`

Hasil: 171 test lulus. Pengujian integrasi dilakukan pada level service +
`AppDbContext` dengan SQLite in-memory.

Gap yang belum diuji:
- PostgreSQL nyata.
- API/controller.
- UI.
- CI otomatis.

## 4. Kesimpulan

- **8 dari 10 deliverable** terverifikasi langsung ke kode/SQL/test:
  02, 03, 05, 06, 07, 08, 09, dan 10.
- **01** adalah analisis arsitektur; komponen yang dijelaskan memang ada.
- **04** adalah latihan algoritma terpisah dan bukan implementasi aplikasi.
- **03, 07, dan 10** memiliki catatan cakupan:
  - view/SP/function belum dipanggil langsung oleh aplikasi,
  - sebagian benchmark adalah prototipe,
  - integrasi belum memakai PostgreSQL/API/UI nyata.
