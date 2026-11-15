# Feature 03 - Product & Category Catalog

- **ID:** F-03
- **Phase:** 1
- **Status:** Implemented
- **Depends on:** -

## 1. Ringkasan

Master data produk beserta kategori dan satuan (unit) sebagai dasar semua
transaksi stok.

## 2. Scope

**In scope**
- CRUD Category, Unit, Product.
- Reorder level per produk, status aktif.
- Pencarian & filter produk.

**Out of scope**
- Varian produk, multi-SKU/barcode, gambar produk, harga jual.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Admin | Full CRUD |
| Staff | Read-only |

## 4. Data Model

Semua PK memakai `Guid` dan mendukung soft delete.

### 4.1 Field Dictionary - Categories

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas unik kategori |
| `Name` | string(150) | Ya | Unik (aktif) | Nama kategori |
| `Description` | string(500) | Tidak | - | Deskripsi kategori |
| `IsActive` | bool | Ya | Default `true` | Status bisnis kategori |
| `CreatedAt` / `CreatedById` | DateTime / int? | Ya / Tidak | Otomatis | Audit pembuatan |
| `UpdatedAt` / `UpdatedById` | DateTime? / int? | Tidak | Otomatis | Audit perubahan |
| `IsDeleted` / `DeletedAt` / `DeletedById` | bool / DateTime? / int? | Ya / Tidak / Tidak | Otomatis | Soft delete & audit hapus |

### 4.2 Field Dictionary - Units

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas unik satuan |
| `Code` | string(30) | Ya | Unik (aktif), uppercase | Kode satuan (PCS, BOX) |
| `Name` | string(100) | Ya | - | Nama satuan |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

### 4.3 Field Dictionary - Products

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas unik produk |
| `Sku` | string(50) | Ya | Unik (aktif), uppercase, tanpa spasi | Kode stok unik |
| `Name` | string(200) | Ya | - | Nama produk |
| `CategoryId` | Guid | Ya | FK Categories | Kategori produk |
| `UnitId` | Guid | Ya | FK Units | Satuan produk |
| `ReorderLevel` | int | Ya | `>= 0` | Ambang batas reorder |
| `IsActive` | bool | Ya | Default `true` | Status bisnis produk |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

Constraint: `Products.Sku` unik; `Categories.Name` unik; `Units.Code` unik.
Semua unique index memakai filter `IsDeleted = false`.

## 5. Workflow

1. Admin membuat Category & Unit terlebih dahulu.
2. Create Product: isi SKU, nama, kategori, unit, reorder level.
3. Edit/non-aktifkan produk.
4. Produk yang sudah punya transaksi tidak bisa dihapus permanen.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET/POST | `/categories`, `/categories/create`, `/categories/edit/{id}` | CRUD kategori |
| GET/POST | `/units`, `/units/create`, `/units/edit/{id}` | CRUD unit |
| GET | `/products` | List + search + filter |
| GET/POST | `/products/create` | Create |
| GET/POST | `/products/edit/{id}` | Update |
| POST | `/products/deactivate/{id}` | Non-aktifkan |
| POST | `/products/delete/{id}` | Soft delete (jika belum dipakai) |

### ProductFormViewModel
```csharp
public sealed class ProductFormViewModel
{
    public Guid? Id { get; set; }
    [Required, StringLength(50)] public string Sku { get; set; }
    [Required, StringLength(200)] public string Name { get; set; }
    [Required] public Guid CategoryId { get; set; }
    [Required] public Guid UnitId { get; set; }
    [Range(0, int.MaxValue)] public int ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
}
```

## 7. Validasi & Business Rules

- SKU unik, uppercase, tanpa spasi.
- `Name` wajib, maks 200 karakter.
- Kategori & unit harus aktif.
- `ReorderLevel >= 0`.

## 8. Edge Cases & Error Handling

- Hapus kategori/unit yang dipakai produk → blokir, arahkan non-aktif.
- Produk non-aktif tidak muncul di form transaksi baru, tapi tetap di histori.

## 9. Security Considerations

- Hanya Admin yang boleh mutasi master data; Staff read-only.
- Validasi input panjang & tipe untuk mencegah injection.

## 10. Acceptance Criteria

- [ ] CRUD kategori, unit, produk berjalan.
- [ ] SKU duplikat ditolak.
- [ ] Produk non-aktif tidak bisa dipilih di transaksi baru.
- [ ] Search produk berdasarkan SKU/nama berfungsi.
