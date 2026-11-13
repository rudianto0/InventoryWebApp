# Feature 04 - Warehouse & Location

- **ID:** F-04
- **Phase:** 1
- **Status:** Implemented
- **Depends on:** -

## 1. Ringkasan

Master lokasi untuk menandai cabang dan gudang. Semua transaksi & saldo stok
terikat ke `LocationId`. Cabang ditandai di sini (bukan multi-tenant).

## 2. Scope

**In scope**
- CRUD Location dengan tipe `Branch | Warehouse | Store`.
- Hierarki `ParentLocationId` (Cabang → Gudang).
- `BranchCode` untuk pengelompokan laporan.

**Out of scope**
- Denah/bin location detail (rak), geo-location, multi-tenant.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Admin | Full CRUD |
| Staff/Manager | Read-only |

## 4. Data Model

`Locations`: Id, Code (unik), Name, Type, ParentLocationId, BranchCode, Address, IsActive.

| Code | Name | Type | Parent | BranchCode |
|------|------|------|--------|-----------|
| HQ | Kantor Pusat | Branch | null | HQ |
| HQ-WH1 | Gudang Pusat | Warehouse | HQ | HQ |
| BR1 | Cabang Bandung | Branch | null | BR1 |

## 5. Workflow

1. Buat lokasi tipe Branch terlebih dahulu.
2. Buat Warehouse dengan `ParentLocationId` ke branch terkait.
3. Non-aktifkan lokasi yang tidak dipakai (tidak menghapus histori).

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/locations` | List (tree/flat + filter type) |
| GET/POST | `/locations/create` | Create |
| GET/POST | `/locations/edit/{id}` | Update |
| POST | `/locations/deactivate/{id}` | Non-aktifkan |

### LocationFormViewModel
```csharp
public sealed class LocationFormViewModel
{
    public int? Id { get; set; }
    [Required, StringLength(30)] public string Code { get; set; }
    [Required, StringLength(150)] public string Name { get; set; }
    [Required] public LocationType Type { get; set; }
    public int? ParentLocationId { get; set; }
    [StringLength(30)] public string? BranchCode { get; set; }
    [StringLength(300)] public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
```

## 7. Validasi & Business Rules

- `Code` unik, uppercase.
- `ParentLocationId` tidak boleh diri sendiri; tidak boleh membentuk siklus.
- Parent harus bertipe `Branch` (untuk `Warehouse`/`Store`).
- Lokasi dengan stok/transaksi tidak boleh dihapus → non-aktif.

## 8. Edge Cases & Error Handling

- Parent non-aktif → tolak simpan child baru.
- Menonaktifkan branch dengan child aktif → tolak atau non-aktifkan berantai (pilih: tolak).
- Siklus hierarki (A parent B, B parent A) → validasi.

## 9. Security Considerations

- Hanya Admin yang boleh mutasi.
- Semua perubahan dicatat di AuditLog.

## 10. Acceptance Criteria

- [ ] CRUD lokasi berjalan dengan tipe & parent.
- [ ] Kode duplikat ditolak.
- [ ] Siklus hierarki ditolak.
- [ ] Lokasi non-aktif tidak muncul di transaksi baru.
