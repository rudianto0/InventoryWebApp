# 06 - Koneksi Basis Data

## 6a. Teknologi Koneksi

Aplikasi memakai **Npgsql via EF Core**. Konfigurasi provider ada di
`Infrastructure/DependencyInjection.cs`.

### Contoh connection string (placeholder)

Development tanpa TLS:

```
Host=localhost;Port=5432;Database=InventoryAppDB;Username=inventory_app;Password=<PASSWORD>;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=50;Timeout=15;Command Timeout=30
```

Produksi dengan TLS:

```
Host=db.internal;Port=5432;Database=InventoryAppDB;Username=inventory_app;Password=<PASSWORD>;SSL Mode=VerifyFull;Trust Server Certificate=false;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=50;Timeout=15;Command Timeout=30;Application Name=InventoryWeb
```

### Rekomendasi konfigurasi

| Parameter | Rekomendasi |
|---|---|
| `Pooling` | `true` |
| `Maximum Pool Size` | 50–100 per instance |
| `Timeout` | 15 detik |
| `Command Timeout` | 30 detik |
| `Application Name` | `InventoryWeb` |
| `SSL Mode` | `Require`/`VerifyFull` di produksi |

### Retry

Belum aktif. Untuk produksi, pertimbangkan:

```csharp
options.UseNpgsql(connection, npgsql =>
    npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5),
        errorCodesToAdd: null));
```

## 6b. Keamanan Koneksi

### Temuan saat ini

`src/Inventory.Web/appsettings.json` masih menyimpan password plaintext.
Jangan commit file tersebut ke repository publik.

### Rekomendasi

1. Gunakan **User Secrets** saat development:
   ```powershell
   dotnet user-secrets init --project src/Inventory.Web
   dotnet user-secrets set "ConnectionStrings:Default" "<connection-string>" --project src/Inventory.Web
   ```
2. Gunakan **environment variable** di server:
   ```
   ConnectionStrings__Default=Host=...;Username=inventory_app;Password=...
   ```
3. Gunakan vault/secret manager untuk produksi.
4. Aktifkan TLS:
   - `SSL Mode=Require` minimal.
   - `SSL Mode=VerifyFull` + `Trust Server Certificate=false` untuk produksi.
5. Batasi akses port PostgreSQL ke aplikasi saja (firewall/security group).
6. Jangan memakai `postgres` sebagai user aplikasi.
7. Jangan mengaktifkan `EnableSensitiveDataLogging` di produksi.
8. Rotasi password secara berkala.

## 6c. Hak Pengguna Database

Hanya dua role yang dipakai:

| Role | Fungsi | Hak |
|---|---|---|
| `inventory_owner` | Pemilik schema/migrasi | DDL, ownership, grant |
| `inventory_app` | Runtime aplikasi | SELECT/INSERT/UPDATE/DELETE, EXECUTE function/procedure, USAGE sequence |

### Prinsip

- Aplikasi memakai `inventory_app`, bukan `inventory_owner`.
- Migrasi/DDL dijalankan oleh `inventory_owner` atau job migrasi.
- `inventory_app` tidak boleh `CREATE`, `ALTER`, `DROP`.
- Role aplikasi (Admin/SPV/Staff/Manager/Auditor) tetap diatur aplikasi.

### Script

`../sql/01-roles-and-grants/10-roles-and-grants.sql`

Isi:

- Membuat role `inventory_owner` dan `inventory_app` (placeholder password).
- Memindahkan ownership objek ke `inventory_owner`.
- Memberi `CONNECT`, `USAGE` schema, DML, sequence, execute.
- Mencabut `CREATE` dari `PUBLIC`.
- Default privileges untuk objek baru.

### Contoh manual

```sql
GRANT CONNECT ON DATABASE "InventoryAppDB" TO inventory_app;
GRANT USAGE ON SCHEMA public TO inventory_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO inventory_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO inventory_app;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO inventory_app;
GRANT EXECUTE ON ALL PROCEDURES IN SCHEMA public TO inventory_app;

REVOKE CREATE ON SCHEMA public FROM PUBLIC;
```
