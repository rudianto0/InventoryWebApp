# 2. Buatlah Prosedur Akses terhadap Basis Data

## a. Terapkan library akses basis data.

Aplikasi memakai **EF Core** dengan penghubung **Npgsql** untuk berbicara
dengan PostgreSQL. Library ini menerjemahkan perintah aplikasi menjadi SQL.

| Komponen | Fungsi sederhana |
|---|---|
| EF Core | Mengubah data aplikasi menjadi perintah database |
| Npgsql | Penghubung resmi ke PostgreSQL |
| ASP.NET Core Identity | Menangani login/logout dan peran |
| LINQ | Cara menulis query dalam bahasa C# |

```csharp
var products = await db.Products.AsNoTracking()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Sku)
    .ToListAsync(ct);
```

## b. Terapkan perintah akses basis data yang relevan dengan teknologi atau jenis data baru untuk mengakses data.

| Kebutuhan | Perintah yang dipakai |
|---|---|
| Tambah/ubah/hapus | `Add`, `Update`, `Remove`, `SaveChangesAsync` |
| Baca data | `Select`, `Where`, `OrderBy`, `ToListAsync` |
| Ambil view | `SqlQuery<T>` dari `v_...` |
| Panggil function | `SELECT fn_...` |
| Panggil procedure | `CALL sp_...` |
| Transaksi | `BeginTransactionAsync` |
| Halaman | `Skip`/`Take` |

Contoh memanggil view dan function:

```csharp
var pending = await db.Database
    .SqlQuery<ApprovalItem>($"SELECT * FROM \"v_pending_approvals\"")
    .ToListAsync(ct);

var stock = await db.Database
    .SqlQuery<int>($"SELECT fn_available_stock({productId}, {locationId})")
    .SingleAsync(ct);
```

Tipe data yang dipakai: `uuid` untuk ID, `integer` untuk jumlah, `numeric`
untuk harga, dan `timestamptz` untuk waktu.

Praktik aman: gunakan parameter (bukan gabungan teks), batasi waktu query,
dan gunakan `CancellationToken` agar permintaan bisa dibatalkan.
