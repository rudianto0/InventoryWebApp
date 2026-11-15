# Feature: <Feature Name>

- **ID:** F-<NN>
- **Status:** Draft
- **Owner:** <Nama>
- **Last Updated:** <YYYY-MM-DD>

## 1. Ringkasan

Deskripsi singkat tujuan fitur dan nilai bisnisnya.

## 2. Scope

**In scope**
- ...

**Out of scope**
- ...

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Admin | ... |
| Staff  | ... |

## 4. Data Model

> Wajib: setiap field yang dipakai fitur harus dijelaskan fungsinya, bukan hanya namanya.

### 4.1 Field Dictionary

| Entity | Field | Type | Required | Constraint / Default | Fungsi |
|--------|-------|------|----------|----------------------|--------|
| ...    | ...   | ...  | ...      | ...                  | ...    |

### 4.2 Audit & Soft Delete

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `CreatedAt` | DateTime (UTC) | Ya | Otomatis saat insert | Waktu record dibuat |
| `CreatedById` | int? | Tidak | Otomatis dari sesi | User pembuat record |
| `UpdatedAt` | DateTime? | Tidak | Otomatis saat update | Waktu terakhir diubah |
| `UpdatedById` | int? | Tidak | Otomatis dari sesi | User terakhir yang mengubah |
| `IsDeleted` | bool | Ya | Default `false` | Penanda soft delete |
| `DeletedAt` | DateTime? | Tidak | Otomatis saat delete | Waktu dihapus |
| `DeletedById` | int? | Tidak | Otomatis dari sesi | User yang menghapus |

> Jika entity tidak mendukung soft delete, tulis alasannya di bagian ini.

**Relasi**
- ...

## 5. Workflow

1. ...
2. ...

## 6. API Contract

> Target kontrak API (`/api/v1`). Implementasi saat ini masih berupa route MVC
> (`/{controller}/{action}`) di `Inventory.Web`; endpoint API akan tersedia pada
> fase `Inventory.Api`.

| Method | Endpoint | Deskripsi | Auth |
|--------|----------|-----------|------|
| GET | `/api/v1/...` | ... | ... |

**Request/Response contoh**

```json
{ }
```

## 7. Validasi & Business Rules

- ...

## 8. Edge Cases & Error Handling

- ...

## 9. Security Considerations

- ...

## 10. UI/UX Notes (opsional)

- ...

## 11. Open Questions

- ...
