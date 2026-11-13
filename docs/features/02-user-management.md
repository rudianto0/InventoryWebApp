# Feature 02 - User & Role Management

- **ID:** F-02
- **Phase:** 1
- **Status:** Implemented
- **Depends on:** F-01

## 1. Ringkasan

Admin mengelola user (CRUD) dan menetapkan role serta atasan (SPV/parent)
yang menjadi approver transaksi.

## 2. Scope

**In scope**
- CRUD user, assign role, set `ParentId` (SPV).
- Aktif/non-aktif user, reset password (set password baru).
- Read-only daftar role.

**Out of scope**
- CRUD role/permission granular, audit detail user (lihat F-12).

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Admin | Full CRUD |
| SPV / Manager / Auditor | Read-only (opsional) |

## 4. Data Model

`Users`: Id, UserName, Email, FullName, RoleId, **ParentId** (SPV), IsActive, CreatedAt.

Aturan parent: `ParentId != Id`; hanya user ber-role `SPV`/`Manager` yang boleh
dipilih sebagai parent.

## 5. Workflow

1. Admin membuka `/users`.
2. Create/Edit: isi data, pilih role, pilih atasan (SPV).
3. Simpan → validasi unik username/email.
4. Non-aktifkan → user tidak bisa login tapi data histori tetap ada.
5. Reset password → Admin set password baru (paksa ganti saat login, opsional).

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/users` | List (search, paging, filter role/status) |
| GET | `/users/create` | Form create |
| POST | `/users/create` | Simpan |
| GET | `/users/edit/{id}` | Form edit |
| POST | `/users/edit/{id}` | Update |
| POST | `/users/deactivate/{id}` | Non-aktifkan |
| POST | `/users/reset-password/{id}` | Reset password |

### UserFormViewModel
```csharp
public sealed class UserFormViewModel
{
    public int? Id { get; set; }
    [Required] public string UserName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required] public string FullName { get; set; }
    [Required] public int RoleId { get; set; }
    public int? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Password { get; set; } // wajib saat create
}
```

## 7. Validasi & Business Rules

- Username & email unik.
- Tidak boleh menonaktifkan / menghapus Admin terakhir.
- `ParentId` tidak boleh diri sendiri; parent harus aktif & role SPV/Manager.
- User dengan bawahan (children) tidak boleh dihapus permanen → gunakan non-aktif.
- Password saat create minimal 8 karakter.

## 8. Edge Cases & Error Handling

- Menghapus user yang sudah punya transaksi → blokir, sarankan non-aktif.
- Parent non-aktif saat submit transaksi → ditangani di fitur transaksi.
- Username/email bentrok → tampilkan error per-field.

## 9. Security Considerations

- Hanya Admin yang boleh akses.
- Reset password tidak menampilkan password lama.
- Perubahan role/parent dicatat di AuditLog.

## 10. Acceptance Criteria

- [ ] Admin bisa create/edit/non-aktif user.
- [ ] User baru bisa login dan melihat menu sesuai role.
- [ ] Atasan (SPV) tersimpan dan dipakai sebagai approver.
- [ ] Admin terakhir tidak bisa dinonaktifkan.
