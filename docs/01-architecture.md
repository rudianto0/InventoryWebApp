# 01 - Architecture & Solution Structure

## 1. Solution Layout (Clean Architecture, pragmatic)

```
Inventory.sln
├─ src/
│  ├─ Inventory.Domain/          # Entities, enums, domain rules (no dependency)
│  ├─ Inventory.Application/     # Services/use cases, DTOs, validators, interfaces
│  ├─ Inventory.Infrastructure/  # EF Core DbContext, configs, repositories, provider factory
│  └─ Inventory.Web/             # ASP.NET Core MVC, Razor Views, Identity, middleware
├─ migrations/
│  ├─ Inventory.Migrations.SqlServer/
│  └─ Inventory.Migrations.PostgreSql/
└─ tests/
   ├─ Inventory.UnitTests/
   └─ Inventory.IntegrationTests/
```

Dependency direction: `Web → Application → Domain`, `Infrastructure → Application/Domain`.
Domain tidak bergantung pada proyek lain.

## 2. Layer Responsibilities

| Layer | Isi |
|-------|-----|
| Domain | Entity, enum (`ApprovalStatus`, `MovementType`), invariant & business rule murni. |
| Application | Service per fitur, DTO request/response, validator, interface repository & `IUnitOfWork`. |
| Infrastructure | `AppDbContext`, `IEntityTypeConfiguration<T>`, implementasi repository, provider factory, seeding. |
| Web | Controller, ViewModel, Razor View, filter otorisasi, layout Bootstrap. |

## 3. Database Provider Selection

`Infrastructure/DependencyInjection.cs`:

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration config)
{
    var provider = config["Database:Provider"];         // "SqlServer" | "PostgreSql"
    var connection = config.GetConnectionString("Default");

    services.AddDbContext<AppDbContext>(opt =>
    {
        if (provider == "PostgreSql")
            opt.UseNpgsql(connection, b => b.MigrationsAssembly("Inventory.Migrations.PostgreSql"));
        else
            opt.UseSqlServer(connection, b => b.MigrationsAssembly("Inventory.Migrations.SqlServer"));
    });
    return services;
}
```

Aturan: layer Application/Domain **tidak** boleh referensi namespace provider.

## 4. Request Pipeline (Web)

```
Request → Exception Middleware → Auth (Cookie) → Authorization → Controller
        → Application Service → Repository → EF Core → DB
```

- Error: `ProblemDetails`-style halaman error + logging.
- PRG pattern untuk semua POST.
- Anti-forgery token pada semua form.

## 5. Cross-Cutting Concerns

| Concern | Pendekatan |
|---------|-----------|
| Auth | ASP.NET Core Identity, cookie, role: `Admin, SPV, Staff, Manager, Auditor`. |
| Authorization | `[Authorize(Roles=...)]` + policy per aksi (approve hanya SPV/Admin). |
| Validation | DataAnnotations di ViewModel + FluentValidation di Application service. |
| Audit | `AppDbContext.SaveChangesAsync` mengisi field audit otomatis + menulis `AuditLog`. |
| Soft delete | `AppDbContext.SaveChangesAsync` mengubah `Deleted` menjadi `IsDeleted = true`; global query filter menyembunyikan data. |
| Concurrency | Optimistic concurrency pada `Stock` (quantity update atomik). |
| Time | Semua `DateTime` disimpan UTC, ditampilkan lokal di view. |
| Logging | Serilog / built-in ILogger. |

### 5.1 Audit & Soft Delete Pipeline

Urutan pada `AppDbContext.SaveChangesAsync`:

1. **Capture snapshot audit** dari `ChangeTracker` (sebelum mutasi tambahan) agar
   aksi `Delete` tetap tercatat sebagai `Delete`.
2. **Auto-fill audit**:
   - `Added` → `CreatedAt = UtcNow`, `CreatedById = currentUser.UserId`.
   - `Modified` → `UpdatedAt = UtcNow`, `UpdatedById = currentUser.UserId`.
   - `Deleted` + `ISoftDeletable` → `IsDeleted = true`, `DeletedAt = UtcNow`,
     `DeletedById = currentUser.UserId`, `IsActive = false` (jika ada), lalu
     state diubah menjadi `Modified`.
3. Simpan perubahan entity.
4. Tulis `AuditLog` (termasuk `EntityId` sebagai string untuk mendukung `int` dan `Guid`).

Global query filter `!IsDeleted` diterapkan otomatis untuk semua entity
`ISoftDeletable`. Untuk kebutuhan restore/admin, gunakan `IgnoreQueryFilters()`.

### 5.2 GUID & Waktu

- Data bisnis (master, inventory, transaksi) memakai PK `Guid` yang dibuat
  aplikasi dengan `Guid.CreateVersion7()` (time-ordered).
- `Users`, `Roles` (Identity), `DocumentSequences`, dan `AuditLogs` tetap `int`.
- Field referensi user tetap `int?` dan bukan FK fisik lintas layer.
- Unique index memakai filter `IsDeleted = false` agar nilai bisa dipakai ulang
  setelah soft delete.

## 6. Stock Mutation Rule (core invariant)

- `Stock` adalah agregat saldo per `(ProductId, LocationId)`.
- Setiap perubahan saldo **wajib** menghasilkan satu `StockMovement`.
- Mutasi hanya terjadi saat transaksi berstatus **Approved**.
- Operasi mutasi dijalankan dalam satu DB transaction; gunakan `SELECT ... FOR UPDATE`
  (PostgreSQL) / `UPDLOCK` (SQL Server) melalui repository agar saldo tidak race.
- Stok tidak boleh negatif pada Stock Out/Transfer (dapat dikonfigurasi).

## 7. Testing Strategy

- **Unit:** business rule (approval, perhitungan saldo, validasi).
- **Integration:** repository + provider (minimal satu provider di CI).
- **Smoke:** jalankan aplikasi dengan provider kedua sebelum rilis.

## 8. Naming & Conventions

- Tabel: PascalCase plural (`Products`, `StockMovements`).
- Primary key: `Id` (`Guid` untuk data bisnis, `int` untuk Identity/system).
- Nomor dokumen: `{PREFIX}-{yyyyMM}-{sequence}` (contoh `IN-202611-0001`).
- Endpoint MVC: `/products`, `/stock-ins`, `/approvals`.
