# ADR 001 - Database Provider Strategy (SQL Server & PostgreSQL)

- **Status:** Accepted
- **Date:** 2026-11-06

## Context

Aplikasi harus dapat dijalankan di atas SQL Server maupun PostgreSQL
tergantung kebutuhan deployment, tanpa mengubah kode domain/application.

## Decision

1. Gunakan **EF Core** sebagai satu-satunya akses data. Dilarang raw SQL
   provider-specific di layer Application/Domain.
2. Provider dipilih via konfigurasi:
   ```json
   { "Database": { "Provider": "PostgreSql", "ConnectionString": "..." } }
   ```
   Nilai yang didukung: `SqlServer`, `PostgreSql`.
3. Registrasi provider di `Infrastructure` melalui factory:
   `DataProvider.SqlServer` / `DataProvider.PostgreSql`.
4. **Migrations dipisah per provider** (migration assembly terpisah:
   `Inventory.Migrations.SqlServer`, `Inventory.Migrations.PostgreSql`)
   karena DDL EF Core bersifat provider-specific.
5. Hindari tipe kolom native yang tidak portabel. Gunakan:
   - `string` dengan panjang eksplisit (indexable di kedua provider).
   - `int` untuk quantity dan `decimal(18,4)` untuk cost.
   - `DateTime` dalam **UTC**.
   - Nilai JSON disimpan sebagai `string` (bukan tipe `jsonb` native).
6. Row versioning / concurrency memakai `rowversion` (SQL Server) atau
   `xmin` (PostgreSQL) hanya bila diperlukan; utilitas lintas provider
   diisolasi di Infrastructure.

## Consequences

- **Positif:** fleksibel, mudah diuji, mendukung 2 target deployment.
- **Negatif:** dua set migration harus dijaga sinkron; disiplin menghindari
  fitur DB provider-specific.
- **Mitigasi:** integration test dijalankan minimal pada satu provider di CI,
  smoke test provider kedua sebelum rilis.

## Alternatives Considered

- Dapper + SQL per provider: ditolak, duplikasi query.
- Hanya satu provider: ditolak sesuai kebutuhan.