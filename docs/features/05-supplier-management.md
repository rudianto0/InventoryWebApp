# Feature 05 - Supplier Management

- **ID:** F-05
- **Phase:** 2
- **Status:** Implemented
- **Depends on:** F-03

## 1. Ringkasan

Master pemasok (supplier) yang dipakai pada transaksi Stock In.

## 2. Scope

**In scope**
- CRUD supplier, status aktif, pencarian.
- Kontak: contact person, phone, email, alamat.

**Out of scope**
- Purchase Order penuh, termin pembayaran, hutang/piutang.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Admin | Full CRUD |
| Staff | Read-only |

## 4. Data Model

`Suppliers` (PK `Guid`, soft delete).

### 4.1 Field Dictionary

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas unik supplier |
| `Code` | string(30) | Ya | Unik (aktif), uppercase | Kode supplier |
| `Name` | string(150) | Ya | - | Nama supplier |
| `ContactPerson` | string(150) | Tidak | - | Nama narahubung |
| `Phone` | string(30) | Tidak | - | Nomor telepon |
| `Email` | string(150) | Tidak | Format email valid | Email narahubung |
| `Address` | string(300) | Tidak | - | Alamat supplier |
| `IsActive` | bool | Ya | Default `true` | Status bisnis supplier |
| `CreatedAt` / `CreatedById` | DateTime / int? | Ya / Tidak | Otomatis | Audit pembuatan |
| `UpdatedAt` / `UpdatedById` | DateTime? / int? | Tidak | Otomatis | Audit perubahan |
| `IsDeleted` / `DeletedAt` / `DeletedById` | bool / DateTime? / int? | Ya / Tidak / Tidak | Otomatis | Soft delete & audit hapus |

## 5. Workflow

1. Admin membuat supplier baru.
2. Supplier aktif dapat dipilih di form Stock In.
3. Supplier dengan transaksi tidak bisa dihapus permanen → non-aktif.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/suppliers` | List + search |
| GET/POST | `/suppliers/create` | Create |
| GET/POST | `/suppliers/edit/{id}` | Update |
| POST | `/suppliers/deactivate/{id}` | Non-aktifkan |
| POST | `/suppliers/delete/{id}` | Soft delete (jika belum dipakai) |

### SupplierFormViewModel
```csharp
public sealed class SupplierFormViewModel
{
    public Guid? Id { get; set; }
    [Required, StringLength(30)] public string Code { get; set; }
    [Required, StringLength(150)] public string Name { get; set; }
    [StringLength(150)] public string? ContactPerson { get; set; }
    [StringLength(30)] public string? Phone { get; set; }
    [EmailAddress, StringLength(150)] public string? Email { get; set; }
    [StringLength(300)] public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
```

## 7. Validasi & Business Rules

- `Code` unik, uppercase.
- `Name` wajib.
- Email format valid bila diisi.

## 8. Edge Cases & Error Handling

- Duplikat kode → error per-field.
- Supplier non-aktif tidak muncul di transaksi baru.

## 9. Security Considerations

- Mutasi hanya Admin.
- Perubahan dicatat di AuditLog.

## 10. Acceptance Criteria

- [ ] CRUD supplier berjalan.
- [ ] Kode duplikat ditolak.
- [ ] Supplier aktif muncul di form Stock In.
