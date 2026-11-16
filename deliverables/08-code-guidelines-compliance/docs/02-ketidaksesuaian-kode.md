# 02 - Ketidaksesuaian Kode

## 1. Daftar Ketidaksesuaian

| ID | Temuan | Kategori | Dampak | Status |
|---|---|---|---|---|
| NC-01 | `Class1.cs` kosong di 3 project | Dead code | Kebingungan struktur | Fixed |
| NC-02 | Magic string role (`"Admin"`, `"SPV"`, dll.) | Readability | Sulit diubah konsisten | Open |
| NC-03 | Magic prefix dokumen (`IN`, `OUT`, dll.) | Maintainability | Risiko typo | Open |
| NC-04 | Clamp pagination berulang | Duplikasi | Inkonsistensi | Open |
| NC-05 | Method/class besar | SRP | Sulit diuji/dibaca | Open |
| NC-06 | `ApprovalGuard` menumpang di `StockLedger.cs` | Cohesion | File tidak fokus | Fixed |
| NC-07 | `AppControllerBase` memakai EF exception | Layering | Coupling Web–EF | Open (transisi) |
| NC-08 | `catch (Exception ex) when` berulang | Duplikasi | Sulit dipelihara | Open |
| NC-09 | Sebagian public API tanpa XML comments | Dokumentasi | API docs kurang lengkap | Open |
| NC-10 | Kredensial di `appsettings.json` | Security | Risiko kebocoran | Open |
| NC-11 | N+1/query per baris | Performance | Latensi linear | Open |
| NC-12 | Tidak ada `.editorconfig` | Tooling | Gaya tidak konsisten | Fixed |
| NC-13 | Pagination/clamp duplikat di ReportService | Duplikasi | Inkonsistensi | Open |

## 2. Detail

### NC-01 — Class1.cs
File template tidak dipakai. Dihapus dari Domain, Application, Infrastructure.

### NC-02 — Magic string role
Contoh: `IsInRole("Admin")`, validasi role. Rekomendasi: `RoleNames` constants.

### NC-03 — Prefix dokumen
`"IN"`, `"OUT"`, `"TRF"`, `"ADJ"`, `"OPN"`, `"MOV"` tersebar. Rekomendasi:
`DocumentPrefixes`.

### NC-04 — Pagination
`Math.Clamp(Page, 1, ...)` dan `PageSize 1..200` berulang. Rekomendasi:
`Paging` constants/helper.

### NC-05 — Class besar
`CatalogService` dan beberapa controller menangani banyak use case.

### NC-06 — ApprovalGuard
Sudah dipisahkan ke `ApprovalGuard.cs`.

### NC-07 — EF di Web
`AppControllerBase` menangani `DbUpdateConcurrencyException`. Solusi target:
map ke exception client.

### NC-08 — Catch berulang
Controller memiliki pola try/catch serupa.

### NC-09 — XML comments
`GenerateDocumentationFile` aktif, tetapi belum semua public API memiliki summary.

### NC-10 — Kredensial
`appsettings.json` lokal berisi password. Tidak di-commit; gunakan secrets.

### NC-11 — Query per baris
Ledger/approval/stock check. Lihat `deliverables/07`.

### NC-12 — `.editorconfig`
Ditambahkan di root.

### NC-13 — Duplikasi report
Clamp/pagination berulang.

## 3. Prioritas

| Prioritas | Item |
|---|---|
| Tinggi | NC-10, NC-11, NC-02, NC-03 |
| Sedang | NC-04, NC-05, NC-07, NC-09 |
| Rendah | NC-08, NC-13 |
