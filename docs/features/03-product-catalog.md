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

| Entity | Fields |
|--------|--------|
| Categories | Id, Name, Description, IsActive |
| Units | Id, Code, Name |
| Products | Id, Sku, Name, CategoryId, UnitId, ReorderLevel, IsActive |

Constraint: `Products.Sku` unik; `Categories.Name` unik; `Units.Code` unik.

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

### ProductFormViewModel
```csharp
public sealed class ProductFormViewModel
{
    public int? Id { get; set; }
    [Required, StringLength(50)] public string Sku { get; set; }
    [Required, StringLength(200)] public string Name { get; set; }
    [Required] public int CategoryId { get; set; }
    [Required] public int UnitId { get; set; }
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
