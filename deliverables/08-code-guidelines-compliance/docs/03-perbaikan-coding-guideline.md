# 03 - Perbaikan Coding Guideline

Prinsip: **tidak mengubah spesifikasi, kontrak, atau perilaku aplikasi.**

## 1. Perbaikan yang Diterapkan

### FIX-01 — Hapus `Class1.cs`
- File: `Domain/Class1.cs`, `Application/Class1.cs`, `Infrastructure/Class1.cs`.
- Alasan: dead code/template.
- Dampak: tidak ada perubahan perilaku.

### FIX-02 — Pisahkan `ApprovalGuard`
- Sebelum: `ApprovalGuard` berada di `StockLedger.cs`.
- Sesudah: `Application/Services/ApprovalGuard.cs`.
- Alasan: cohesion/SRP, file fokus.
- Dampak: tidak ada perubahan API (internal).

### FIX-03 — Tambah `.editorconfig`
- Aturan: charset UTF-8, indent 4, CRLF, system using first,
  namespace file-scoped, hint `IDE0005`.
- Alasan: konsistensi gaya lintas IDE.
- Dampak: tidak ada perubahan runtime.

## 2. Perbaikan yang Direncanakan

| ID | Perbaikan | Risiko |
|---|---|---|
| FIX-04 | `RoleNames` constants | Rendah |
| FIX-05 | `DocumentPrefixes` constants | Rendah |
| FIX-06 | `Paging` constants/helper | Rendah |
| FIX-07 | XML comments public API | Rendah |
| FIX-08 | Ekstrak `FormError`/handling | Rendah |
| FIX-09 | Pisahkan class besar | Sedang |
| FIX-10 | Ganti EF reference di Web | Sedang |
| FIX-11 | Batch query (lihat 07) | Sedang |

## 3. Contoh Perbaikan

### Sebelum
```csharp
if (currentUser.IsInRole("Admin")) { ... }
```

### Sesudah (rencana)
```csharp
if (currentUser.IsInRole(RoleNames.Admin)) { ... }
```

### Sebelum
```csharp
var pageSize = Math.Clamp(filter.PageSize, 1, 200);
```

### Sesudah (rencana)
```csharp
var pageSize = Paging.Clamp(filter.PageSize);
```

## 4. Batasan

- Refactor besar tidak dilakukan dalam satu perubahan agar risiko regresi kecil.
- Setiap perbaikan wajib melewati build + 171 test.
- Kontrak `Contracts`/`Client` tidak berubah.
