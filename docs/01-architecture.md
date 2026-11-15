# 01 - Architecture & Solution Structure

## 1. Solution Layout (Clean Architecture, API-first ready)

```
Inventory.sln
├─ src/
│  ├─ Inventory.Domain/             # Entities, enums (melalui Contracts), domain rules
│  ├─ Inventory.Contracts/          # DTO request/response, enum publik, PagedResult
│  ├─ Inventory.Application/        # Use cases, validasi, abstraksi
│  ├─ Inventory.Client/             # Interface client + implementasi in-process
│  ├─ Inventory.Infrastructure/     # EF Core DbContext, Identity, provider
│  ├─ Inventory.Migrations.SqlServer/
│  ├─ Inventory.Migrations.PostgreSql/
│  └─ Inventory.Web/                # Razor MVC UI
├─ tests/
│  └─ Inventory.UnitTests/
└─ docs/
```

Target jangka menengah menambahkan:

```
├─ Inventory.Api/                   # REST API /api/v1, auth, OpenAPI
└─ Inventory.Api.IntegrationTests/
```

## 2. Dependency Direction

```
Inventory.Web        ──▶ Inventory.Client ──▶ Inventory.Contracts
      │                        │
      │                        └──▶ Inventory.Application ──▶ Inventory.Domain ──▶ Inventory.Contracts
      │
      └──▶ Inventory.Infrastructure (hanya komposisi startup saat ini)

Inventory.Infrastructure ──▶ Inventory.Application / Inventory.Domain
```

Aturan yang dijaga:

1. `Inventory.Web` tidak boleh memakai `Inventory.Domain.Entities` atau service
   Application secara langsung untuk use case bisnis. Pengecualian transisi:
   komposisi startup, Identity/Account, dan penanganan concurrency masih berada
   di Web sampai `Inventory.Api` selesai.
2. `Inventory.Client` tidak boleh memakai `Inventory.Infrastructure`.
3. `Inventory.Contracts` tidak boleh memakai EF Core atau entity Domain.
4. `Inventory.Domain` dan `Inventory.Application` tidak boleh memakai namespace
   provider database.
5. `Inventory.Api` (nanti) adalah satu-satunya host yang memakai Infrastructure
   secara langsung.

## 3. Layer Responsibilities

| Layer | Isi |
|-------|-----|
| Domain | Entity, invariant, business rule murni. Enum publik berasal dari Contracts. |
| Contracts | Kontrak stabil lintas boundary: request/response DTO, `PagedResult<T>`, enum (`ApprovalStatus`, `MovementType`, `LocationType`, `ReferenceType`), `*Response` master data. |
| Application | Use case per fitur, validasi, mapping entity → Contracts, abstraksi (`IAppDbContext`, `IUserDirectory`, `IDocumentNumberService`, `IUserService`). |
| Client | Interface per area (`ICatalogClient`, `ISupplierClient`, `IStockInClient`, dst.) dan implementasi `InProcess*Client` yang membungkus service Application. |
| Infrastructure | `AppDbContext`, konfigurasi EF, Identity (`AppUser`, `AppRole`), `UserService`, `DocumentNumberService`, `UserDirectory`, seeding/migration bootstrap. |
| Web | Razor view, controller UI, ViewModel, otorisasi tampilan, layout. Memakai Client. |
| Api (target) | REST controller/minimal API, ProblemDetails, OpenAPI, auth cookie + JWT. |

## 4. Contract Rules

- Controller/Client **tidak** menerima atau mengembalikan entity Domain.
- Master data mengembalikan `CategoryResponse`, `UnitResponse`, `ProductResponse`,
  `SupplierResponse`, `LocationResponse`.
- Create transaksi mengembalikan `TransactionCreatedResponse` (`Id`, `DocNo`,
  `Status`, detail ringkas).
- Detail transaksi memakai `TransactionDetailView` / `DocumentLineView`.
- Semua endpoint API memakai versioning `/api/v1`.
- Perubahan kontrak dicatat dan diuji; jangan menambah field internal/DB ke DTO.

## 5. Error & Validation Contract

| Kondisi | HTTP (target API) | Body |
|---------|-------------------|------|
| `ValidationException` | 400 | `ProblemDetails` + `errors` |
| Business rule / invalid state | 409 | `ProblemDetails` + `code` |
| Not found | 404 | `ProblemDetails` |
| Unauthorized / Forbidden | 401 / 403 | default |
| `DbUpdateConcurrencyException` | 409 | `code = "concurrency"` |

Di fase in-process, Web masih menangani exception melalui `AppControllerBase`
dan menampilkan pesan ke `ModelState`/`TempData`.

## 6. Authentication Strategy

- **Sekarang (MVC):** ASP.NET Core Identity + cookie.
- **Target API:**
  - API mendukung **dual auth**: cookie untuk MVC dan Bearer JWT untuk client
    HTTP/SPA.
  - Web berperan sebagai BFF: menyimpan token server-side, browser tidak
    menerima token mentah.
  - `ICurrentUserService` tetap abstraksi Application; implementasi HTTP
    membaca claims dari JWT/cookie.
- CORS hanya diaktifkan untuk origin client yang dikonfigurasi.

## 7. Request Pipeline (target)

```
Client (Razor/HTTP) → Inventory.Api
   → Exception handler (ProblemDetails)
   → Authentication (cookie/JWT) → Authorization
   → Application service → EF Core → Database
```

## 8. Stock Mutation Rule (core invariant)

- `Stock` adalah agregat saldo per `(ProductId, LocationId)`.
- Setiap perubahan saldo **wajib** menghasilkan satu `StockMovement`.
- Mutasi hanya terjadi saat transaksi berstatus **Approved** (Admin: submit =
  auto-approve sesuai kebijakan saat ini).
- Operasi mutasi dijalankan dalam satu DB transaction.
- Stok tidak boleh negatif.

## 9. Database Provider Selection

`Infrastructure/DependencyInjection.cs` memilih provider dari
`Database:Provider` (`SqlServer` | `PostgreSql`) dan mengarahkan migration
assembly yang sesuai. Layer Application/Domain tetap provider-agnostic.

## 10. Migration Path to API + Client

| Phase | Fokus | Status |
|-------|-------|--------|
| 0 | `Inventory.Contracts` + enum/DTO dipindahkan; boundary dasar | Selesai |
| 1 | Service tidak lagi mengembalikan entity; response DTO dipakai UI | Selesai |
| 2 | `Inventory.Client` + `InProcess*Client`; Web memakai interface client | Selesai |
| 3 | `Inventory.Api` REST `/api/v1`, ProblemDetails, OpenAPI, auth | Berikutnya |
| 4 | `Http*Client` + Web swap ke HTTP; Account pindah ke API | Berikutnya |
| 5 | Deploy terpisah, CORS, health check, generated TypeScript client | Berikutnya |

Lihat [adr/006-api-first-client-server.md](adr/006-api-first-client-server.md).

## 11. Testing Strategy

- **Unit:** business rule (approval, perhitungan saldo, validasi) — 171 test saat ini.
- **Contract:** serialisasi DTO/response.
- **API integration (target):** WebApplicationFactory + SQLite, cek ProblemDetails
  dan otorisasi role.
- **Client:** mock `HttpMessageHandler` untuk mapping error.
- **Architecture:** NetArchTest untuk dependency rule (Web/Client/Contracts).

## 12. Naming & Conventions

- Tabel: PascalCase plural (`Products`, `StockMovements`).
- Primary key: `Id` (`Guid` untuk data bisnis, `int` untuk Identity/system).
- Nomor dokumen: `{PREFIX}-{yyyyMM}-{sequence}` (contoh `IN-202611-0001`).
- Endpoint MVC saat ini: `/products`, `/stock-ins`, `/approvals`.
- Endpoint API target: `/api/v1/products`, `/api/v1/stock-ins`, `/api/v1/approvals`.
