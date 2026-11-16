# 07 - Verifikasi dan Regresi

## 1. Build

```powershell
dotnet clean Inventory.sln
dotnet build Inventory.sln -c Debug
```

Hasil:

```text
Build succeeded.
    0 Error(s)
```

## 2. Test

```powershell
dotnet test Inventory.sln
```

Hasil:

```text
Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
```

## 3. Regression Checklist

| Area | Dampak perubahan | Status |
|---|---|---|
| Domain | Hapus `Class1.cs` | Tidak ada perilaku berubah |
| Application | `ApprovalGuard` dipindah file | Build/test sukses |
| Infrastructure | Hapus `Class1.cs` | Tidak ada perilaku berubah |
| Web | Komentar exception | Tidak ada perilaku berubah |
| Contracts/Client | Tidak diubah | Aman |
| API/UI | Tidak diubah | Aman |
| Database | Tidak ada migrasi baru | Aman |

## 4. Pemeriksaan Spesifikasi

- Approval satu level tetap.
- Admin auto-approve tetap.
- Stok tidak negatif.
- Audit/soft delete tetap.
- Stock Opname wajib lokasi berstok.
- Kontrak DTO tidak berubah.

## 5. Pemeriksaan Gaya

- `.editorconfig` aktif.
- Namespace file-scoped.
- System using first.
- Tidak ada secret baru di source/dokumen.
- Komentar pengecualian ditambahkan pada `ApprovalGuard` dan `AppControllerBase`.

## 6. Kesimpulan Verifikasi

Perbaikan **tidak mengubah spesifikasi**. Build dan 171 test lulus.
