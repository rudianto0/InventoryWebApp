# 2. Buatlah Prosedur Akses terhadap Basis Data

## a. Terapkan library akses basis data.

Aplikasi memakai **EF Core**. Provider PostgreSQL memakai **Npgsql**;
provider SQL Server memakai **Microsoft.EntityFrameworkCore.SqlServer**.
Library ini menerjemahkan perintah aplikasi menjadi SQL.

| Komponen | Fungsi sederhana |
|---|---|
| EF Core | Mengubah data aplikasi menjadi perintah database |
| Npgsql | Penghubung ke PostgreSQL |
| SQL Server provider | Penghubung ke SQL Server |
| ASP.NET Core Identity | Menangani login/logout dan peran |
| LINQ | Cara menulis query dalam bahasa C# |

```csharp
var products = await db.Products.AsNoTracking()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Sku)
    .ToListAsync(ct);
```

## b. Terapkan perintah akses basis data yang relevan dengan teknologi atau jenis data baru untuk mengakses data.

Aplikasi saat ini mengakses database melalui **EF Core + LINQ**. View, function,
dan stored procedure tersedia di script SQL, tetapi **belum dipanggil langsung**
oleh kode aplikasi.

| Kebutuhan | Perintah yang dipakai aplikasi |
|---|---|
| Tambah/ubah/hapus | `Add`, `Update`, `Remove`, `SaveChangesAsync` |
| Baca data | `Select`, `Where`, `OrderBy`, `ToListAsync` |
| Query baca tanpa tracking | `AsNoTracking` |
| Transaksi | `BeginTransactionAsync` |
| Halaman | `Skip`/`Take` |
| Panggil data turunan | Service Application, bukan raw SQL |

**Tersedia di database, belum dipakai aplikasi:**

| Objek | Contoh |
|---|---|
| View | `v_stock_balance`, `v_pending_approvals`, `v_stock_card` |
| Function | `fn_available_stock`, `fn_stock_balance`, `fn_stock_card` |
| Procedure | `sp_receive_stock`, `sp_issue_stock`, `sp_transfer_stock` |

Jika nanti ingin memanggil objek tersebut langsung, EF Core menyediakan
`SqlQuery<T>`/`ExecuteSqlInterpolatedAsync`. Contoh berikut adalah **contoh
manual**, bukan alur yang dipakai aplikasi saat ini:

```csharp
var pending = await db.Database
    .SqlQuery<ApprovalItem>($"SELECT * FROM \"v_pending_approvals\"")
    .ToListAsync(ct);
```

Tipe data yang dipakai: `uuid` untuk ID, `integer` untuk jumlah, `numeric`
untuk harga, dan `timestamptz` untuk waktu.

Praktik aman: gunakan parameter (bukan gabungan teks), batasi waktu query,
dan gunakan `CancellationToken` agar permintaan bisa dibatalkan.
