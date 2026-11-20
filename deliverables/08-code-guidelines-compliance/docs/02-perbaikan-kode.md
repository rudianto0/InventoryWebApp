# 2. Memperbaiki Kode Sesuai Coding Guidelines dan Best Practices

Prinsip utama: **perbaikan tidak mengubah spesifikasi, kontrak, atau perilaku
aplikasi.**

## 2.1 Perbaiki kode yang tidak sesuai coding-guideline tanpa mengubah spesifikasi

**Perbaikan yang sudah diterapkan:**

| ID | Perbaikan | Alasan | Dampak |
|---|---|---|---|
| FIX-01 | Hapus `Class1.cs` di 3 project | Kode contoh tidak terpakai | Tidak ada |
| FIX-02 | Pisahkan `ApprovalGuard` ke file sendiri | Satu file satu tanggung jawab | Tidak ada |
| FIX-03 | Tambah `.editorconfig` | Gaya kode seragam | Tidak ada |
| FIX-04 | Tambah komentar pengecualian | Alasan pengecualian jelas | Tidak ada |

**Perbaikan yang direncanakan:**

| Rencana | Contoh |
|---|---|
| Konstanta nama role | `RoleNames.Admin` |
| Konstanta prefix dokumen | `DocumentPrefixes.StockIn` |
| Helper batas halaman | `Paging.Clamp(...)` |
| Komentar XML API publik | `<summary>` pada method publik |
| Pisahkan class besar | Pecah sesuai tanggung jawab |

**Contoh perbaikan yang aman:**

```csharp
// Sebelum
if (currentUser.IsInRole("Admin")) { ... }

// Sesudah
if (currentUser.IsInRole(RoleNames.Admin)) { ... }
```

Perubahan ini hanya mengganti teks tetap menjadi konstanta. Perilaku aplikasi
tetap sama.

## 2.2 Perbaiki kode yang tidak menerapkan best practices

**Praktik baik yang sudah dipakai:**

| Praktik | Bukti |
|---|---|
| Async sampai database | Service dan EF Core |
| DTO di batas aplikasi | `Inventory.Contracts` |
| Validasi terpusat | `Guard` |
| Transaksi banyak tabel | `BeginTransactionAsync` |
| Audit dan soft delete | `AppDbContext` |
| Halaman data | `PagedResult`, `Skip/Take` |
| Query baca tanpa tracking | `AsNoTracking` |

**Rekomendasi perbaikan:**

| Bidang | Tindakan |
|---|---|
| Keamanan | Pindahkan kata sandi ke User Secrets/environment |
| Performa | Batch query, halaman kartu stok, cache user |
| Perawatan | Konstanta, pecah class besar, komentar XML |
| Pemantauan | Health check, logging terstruktur, metrics |

**Contoh query baca yang baik:**

```csharp
var items = await db.Products.AsNoTracking()
    .Where(x => x.IsActive)
    .OrderBy(x => x.Sku)
    .Select(x => new ProductListItem { ... })
    .ToListAsync(ct);
```

**Hasil verifikasi:**

```text
dotnet build Inventory.sln -> 0 error
dotnet test Inventory.sln  -> 171 lulus
```

**Kesimpulan:** perbaikan aman sudah diterapkan, sedangkan perbaikan besar
dilakukan bertahap agar tidak menimbulkan kesalahan baru.
