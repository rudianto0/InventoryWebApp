# 02 - Teknologi Akses Data

## 2.1 Stack yang Digunakan

| Komponen | Teknologi |
|---|---|
| Bahasa | C# / .NET 10 |
| Framework | ASP.NET Core MVC |
| ORM | EF Core 10 |
| Provider PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Identity | ASP.NET Core Identity + EF Core |
| Database | PostgreSQL |

## 2.2 Pola Akses Saat Ini

```
Controller/Client
      │
      ▼
Application Service (CatalogService, StockInService, dst.)
      │
      ▼
IAppDbContext (DbSet<T>)
      │
      ▼
EF Core + Npgsql
      │
      ▼
PostgreSQL
```

Karakteristik:

- Service memakai `DbSet` dan LINQ async (`ToListAsync`, `FirstOrDefaultAsync`).
- `AsNoTracking()` dipakai pada query baca.
- Pagination memakai `Skip/Take` dengan clamp 1–200.
- Proyeksi ke DTO/response dilakukan di query.
- Tidak ada repository formal; `IAppDbContext` adalah abstraksi utama.
- Audit/soft delete otomatis di `SaveChangesAsync`.
- Transaksi eksplisit untuk approval/ledger.

## 2.3 Alternatif Library

| Library | Kelebihan | Kekurangan | Status |
|---|---|---|---|
| EF Core | ORM, tracking, migration, LINQ | Overhead untuk query kompleks | Dipakai |
| NpgsqlCommand (ADO.NET) | Kontrol penuh, cepat | Manual mapping | Tersedia |
| Dapper | Ringan, SQL eksplisit | Mapping manual, tanpa migration | Tidak dipakai |
| Raw SQL via EF | Fleksibel untuk view/function | Perlu parameter hati-hati | Dipakai bila perlu |

## 2.4 Async dan CancellationToken

- Semua query I/O memakai `async`/`await`.
- `CancellationToken` diteruskan dari controller ke service ke EF Core.
- Menghindari blocking thread dan memungkinkan pembatalan request.

## 2.5 Transaction dan Concurrency

- `BeginTransactionAsync` digunakan untuk approval dan mutasi stok.
- `Stocks.Version` adalah concurrency token.
- Konflik menghasilkan `DbUpdateConcurrencyException`.
- Belum ada retry policy otomatis; perlu dipertimbangkan untuk produksi.

## 2.6 Keterbatasan

- Belum ada integration test dengan PostgreSQL nyata.
- Belum ada query logging/telemetry terpusat.
- Belum ada repository/unit-of-work formal (tidak wajib untuk skala CV).
