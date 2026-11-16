# 01 - Persiapan Kode Program

## 1. Spesifikasi Proyek

| Aspek | Ketentuan |
|---|---|
| Bahasa | C# |
| Framework | .NET 10 |
| Arsitektur | Clean Architecture ringan + client boundary |
| UI | ASP.NET Core MVC + Razor |
| Database | PostgreSQL / SQL Server |
| ORM | EF Core 10 |
| Testing | xUnit + FluentAssertions |

## 2. Panduan Penulisan Kode

1. **Naming**
   - Class/method/property: `PascalCase`.
   - Parameter/local variable: `camelCase`.
   - Interface: prefix `I`.
2. **Async**
   - Gunakan `async`/`await` untuk I/O.
   - Teruskan `CancellationToken`.
   - Hindari `.Result`/`.Wait()`.
3. **Validasi**
   - Validasi input di service, bukan hanya di controller.
   - Gunakan `Guard`/`ValidationException`.
4. **Keamanan**
   - Jangan commit kredensial.
   - Gunakan User Secrets/env var.
   - Jangan tampilkan stack trace ke user.
5. **Database**
   - Gunakan migration.
   - Jangan query tanpa parameter.
   - Gunakan transaksi untuk operasi multi-tabel.
6. **Test**
   - Setiap fitur baru memiliki unit test.
   - Jalankan `dotnet test` sebelum commit.

## 3. Struktur Solusi

```
src/
├─ Inventory.Domain/
├─ Inventory.Contracts/
├─ Inventory.Application/
├─ Inventory.Client/
├─ Inventory.Infrastructure/
├─ Inventory.Migrations.PostgreSql/
├─ Inventory.Migrations.SqlServer/
└─ Inventory.Web/
tests/
└─ Inventory.UnitTests/
deliverables/
└─ 06-code-preparation-and-debugging/
```

## 4. Persiapan Awal

```powershell
dotnet --version
dotnet restore
dotnet build Inventory.sln
dotnet test Inventory.sln
```

## 5. Checklist Persiapan

- [x] SDK .NET 10 tersedia.
- [x] Solution dapat di-restore.
- [x] Build sukses.
- [x] Unit test tersedia.
- [x] Konfigurasi database tersedia (tanpa hardcode di repo).
- [x] Alat debugging tersedia (Rider/VS + CLI).
- [x] Dokumentasi struktur proyek tersedia.

## 6. Konfigurasi Build

| Mode | Kegunaan |
|---|---|
| Debug | Breakpoint, logging detail, development |
| Release | Optimasi, tanpa symbol debug, produksi |

Perintah:

```powershell
dotnet build Inventory.sln -c Debug
dotnet build Inventory.sln -c Release
```
