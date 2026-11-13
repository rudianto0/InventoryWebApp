# Feature 01 - Authentication & Authorization

- **ID:** F-01
- **Phase:** 1
- **Status:** Implemented
- **Depends on:** -

## 1. Ringkasan

Login/logout pengguna dan kontrol akses berbasis role menggunakan
ASP.NET Core Identity + Cookie Authentication. Tidak ada registrasi publik;
akun dibuat oleh Admin.

## 2. Scope

**In scope**
- Login, logout, ganti password sendiri.
- Role-based authorization: `Admin, SPV, Staff, Manager, Auditor`.
- Proteksi seluruh halaman kecuali `/Account/Login`.

**Out of scope**
- Registrasi mandiri, lupa password via email, 2FA, SSO.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Semua user | Login, logout, ubah password sendiri |
| Admin | Semua akses + kelola user/role |

## 4. Data Model

Menggunakan tabel Identity + `Users` (lihat `02-data-model.md`).

| Field | Type | Constraint |
|-------|------|------------|
| UserName | string(50) | Required, Unique |
| Email | string(150) | Required, Unique |
| FullName | string(150) | Required |
| RoleId | int | Required, FK Roles |
| ParentId | int? | FK Users (SPV) |
| IsActive | bool | Default true |

## 5. Workflow

1. User membuka aplikasi → diarahkan ke `/Account/Login`.
2. Submit kredensial → validasi Identity.
3. Jika `IsActive == false` → tolak login.
4. Berhasil → cookie dibuat, redirect ke Dashboard.
5. Logout → cookie dihapus.

## 6. Routing / Contract

| Method | Path | Deskripsi | Akses |
|--------|------|-----------|-------|
| GET | `/Account/Login` | Form login | Anonymous |
| POST | `/Account/Login` | Proses login | Anonymous |
| POST | `/Account/Logout` | Logout | Authenticated |
| GET | `/Account/ChangePassword` | Form ganti password | Authenticated |
| POST | `/Account/ChangePassword` | Simpan password baru | Authenticated |

### Login ViewModel
```csharp
public sealed class LoginViewModel
{
    [Required] public string UserName { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; }
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
```

## 7. Validasi & Business Rules

- Username & password wajib.
- Akun non-aktif tidak boleh login (pesan generik, jangan bocorkan status).
- Password minimal 8 karakter, kombinasi huruf & angka (kebijakan Identity).
- `ReturnUrl` hanya boleh local URL (cegah open redirect).

## 8. Edge Cases & Error Handling

- Kredensial salah → pesan generik "Username atau password salah".
- Terlalu banyak percobaan → lockout sementara (Identity lockout 5 menit).
- Cookie kedaluwarsa → redirect ke login.

## 9. Security Considerations

- Cookie `HttpOnly`, `Secure`, `SameSite=Lax`.
- Anti-forgery token di semua POST.
- Password di-hash Identity (PBKDF2), tidak pernah disimpan plaintext.
- Lockout on failed attempts.

## 10. Acceptance Criteria

- [ ] User valid bisa login dan melihat Dashboard.
- [ ] User non-aktif ditolak.
- [ ] Logout menghapus sesi.
- [ ] Halaman terproteksi tidak bisa diakses tanpa login.
- [ ] Password bisa diganti dan password lama divalidasi.
