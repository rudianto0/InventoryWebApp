# Feature 12 - Audit Trail

- **ID:** F-12
- **Phase:** 4
- **Status:** Implemented
- **Depends on:** F-01, F-02, F-06, F-07, F-08, F-09, F-10

## 1. Ringkasan

Jejak audit otomatis untuk perubahan data penting (master & transaksi) dan
aksi approval, agar dapat ditelusuri siapa melakukan apa dan kapan.

## 2. Scope

**In scope**
- Pencatatan create/update/delete pada entity kritikal.
- Pencatatan perubahan status approval.
- Halaman pencarian audit log dengan filter.

**Out of scope**
- Tamper-proof/append-only ledger, retensi otomatis, integrasi SIEM.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Admin | Akses penuh |
| Auditor | Read-only |
| Lainnya | Tidak ada |

## 4. Data Model

`AuditLogs` (PK `int`, append-only, tanpa soft delete).

### 4.1 Field Dictionary

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | int | Ya | PK | Identitas unik log |
| `EntityName` | string(100) | Ya | - | Nama entity yang berubah |
| `EntityId` | string(64) | Ya | `Guid`/`int` dalam bentuk string | ID record yang berubah |
| `Action` | string(30) | Ya | Create/Update/Delete/Submit/Approve/Reject | Jenis aksi |
| `OldValues` | string? | Tidak | JSON | Nilai sebelum perubahan (delta) |
| `NewValues` | string? | Tidak | JSON | Nilai sesudah perubahan (delta) |
| `UserId` | int? | Tidak | `Users.Id` | User pelaku aksi |
| `IpAddress` | string(50) | Tidak | - | Alamat IP pelaku |
| `CreatedAt` | DateTime (UTC) | Ya | Otomatis | Waktu aksi tercatat |

Catatan:
- `EntityId` bertipe `string` karena entity bisnis memakai `Guid`, sedangkan
  `AppUser` memakai `int`.
- Soft delete tetap dicatat sebagai aksi `Delete` (dengan `IsDeleted = true`),
  bukan hard delete.

## 5. Workflow

1. User melakukan aksi pada entity yang diaudit.
2. `SaveChanges` interceptor menangkap perubahan → menulis baris `AuditLogs`.
3. Auditor membuka `/audit-logs`, memfilter berdasarkan entity/action/user/tanggal.
4. Detail menampilkan diff old vs new.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/audit-logs` | List + filter + paging |
| GET | `/audit-logs/details/{id}` | Detail old/new |

### Filter
```csharp
public sealed class AuditFilter
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public int? UserId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
```

## 7. Validasi & Business Rules

- Audit log **tidak boleh** diubah/dihapus dari UI.
- `OldValues`/`NewValues` disimpan sebagai JSON string (portabel SQL Server & PostgreSQL).
- Menyembunyikan field sensitif (password hash, token) sebelum disimpan.
- Hanya field yang berubah disimpan di `NewValues` (delta).

## 8. Edge Cases & Error Handling

- Serialisasi gagal → catat minimal metadata & tandai error, jangan gagalkan transaksi bisnis.
- Update tanpa perubahan efektif → tidak menulis log.
- Data lama dalam jumlah besar → simpan diff terbatas pada field berubah.

## 9. Security Considerations

- Akses hanya Admin/Auditor.
- Jangan simpan secret/PII berlebih.
- Log tidak dapat dimodifikasi via UI.

## 10. Acceptance Criteria

- [ ] Create/update/delete entity kritikal tercatat.
- [ ] Aksi approval tercatat.
- [ ] Filter audit log berfungsi.
- [ ] Field sensitif tidak tersimpan.
- [ ] Auditor dapat melihat detail old/new.
