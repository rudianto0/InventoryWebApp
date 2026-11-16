# Inventory App - Design Documentation

Kumpulan dokumen perancangan untuk aplikasi **Inventory Web App** berbasis .NET.
Setiap fitur didokumentasikan dalam satu file Markdown di folder `features/`.

## Struktur Folder

```
docs/
├─ README.md                 # Index & konvensi dokumentasi (file ini)
├─ 00-project-overview.md    # Visi, scope, tech stack, aktor
├─ 01-architecture.md        # Struktur solusi, layer, provider DB
├─ 02-data-model.md          # ERD & entitas
├─ features/                 # 1 fitur = 1 file .md
├─ adr/                      # Architecture Decision Records
└─ templates/
   └─ feature-template.md    # Template standar dokumen fitur

deliverables/                # Dokumen hasil generate (skalabilitas, database, akses DB,
                             # programming fundamentals, code documentation, manifest)
```

## Dokumen Fondasi

| File | Isi |
|------|-----|
| [00-project-overview.md](00-project-overview.md) | Visi, aktor, tech stack, roadmap |
| [01-architecture.md](01-architecture.md) | Solution layout, layer, provider selection |
| [02-data-model.md](02-data-model.md) | ERD, entitas, enum, constraint |
| [adr/001-database-provider-strategy.md](adr/001-database-provider-strategy.md) | Dukungan SQL Server + PostgreSQL |
| [adr/002-frontend-mvc.md](adr/002-frontend-mvc.md) | MVC + Razor sebagai UI saat ini, dengan boundary client untuk pemisahan API |
| [adr/003-single-level-approval.md](adr/003-single-level-approval.md) | Approval 1 level ke SPV |
| [adr/004-location-branch.md](adr/004-location-branch.md) | Tanpa multi-tenant, cabang di Location |
| [adr/005-guid-soft-delete-audit.md](adr/005-guid-soft-delete-audit.md) | GUID untuk data bisnis, soft delete, dan audit user |
| [adr/006-api-first-client-server.md](adr/006-api-first-client-server.md) | Boundary API-first: Contracts + Client, target pemisahan API/UI |

## Deliverables

Seluruh dokumen hasil generate (skalabilitas, implementasi database, akses basis data,
dasar pemrograman, dan dokumentasi kode) dipindahkan ke folder deliverable:

**[deliverables/README.md](../deliverables/README.md)**

| Section | Isi |
|---|---|
| [01-scalability-analysis](../deliverables/01-scalability-analysis/README.md) | Analisis skalabilitas + Word |
| [02-database-implementation](../deliverables/02-database-implementation/README.md) | Migrasi, seed, index, view, procedure, function, trigger, transaksi + SQL per konteks |
| [03-database-access-and-security](../deliverables/03-database-access-and-security/README.md) | Akses DB, koneksi, keamanan, role, performa |
| [04-programming-fundamentals](../deliverables/04-programming-fundamentals/README.md) | Tipe data, flowchart, algoritma, kompleksitas |
| [05-code-documentation](../deliverables/05-code-documentation/README.md) | Dokumentasi modul/method/eksepsi + DocFX |
| [06-code-preparation-and-debugging](../deliverables/06-code-preparation-and-debugging/README.md) | Persiapan kode, build, debugging, perbaikan |
| [07-performance-profiling](../deliverables/07-performance-profiling/README.md) | Waktu/memori, bottleneck, solusi, kompleksitas |
| [08-code-guidelines-compliance](../deliverables/08-code-guidelines-compliance/README.md) | Kesesuaian kode, perbaikan, pengecualian |
| [09-software-testing](../deliverables/09-software-testing/README.md) | Kebutuhan, data, skenario, hasil, coverage |
| [10-integration-testing](../deliverables/10-integration-testing/README.md) | Peralatan, data integrasi, pelaksanaan, analisis, arsip |
| [99-source-map](../deliverables/99-source-map/README.md) | Manifest & source commit |

## Index Fitur

| No | File | Fitur | Fase | Status |
|----|------|-------|------|--------|
| 01 | [features/01-authentication.md](features/01-authentication.md) | Authentication & Authorization | 1 | Implemented |
| 02 | [features/02-user-management.md](features/02-user-management.md) | User & Role Management | 1 | Implemented |
| 03 | [features/03-product-catalog.md](features/03-product-catalog.md) | Product & Category Catalog | 1 | Implemented |
| 04 | [features/04-warehouse-location.md](features/04-warehouse-location.md) | Warehouse & Location | 1 | Implemented |
| 05 | [features/05-supplier-management.md](features/05-supplier-management.md) | Supplier Management | 2 | Implemented |
| 06 | [features/06-stock-in.md](features/06-stock-in.md) | Stock In (Receiving) | 2 | Implemented |
| 07 | [features/07-stock-out.md](features/07-stock-out.md) | Stock Out (Issuing) | 2 | Implemented |
| 08 | [features/08-stock-transfer.md](features/08-stock-transfer.md) | Stock Transfer | 3 | Implemented |
| 09 | [features/09-stock-adjustment.md](features/09-stock-adjustment.md) | Stock Adjustment | 3 | Implemented |
| 10 | [features/10-stock-opname.md](features/10-stock-opname.md) | Stock Opname (Cycle Count) | 3 | Implemented |
| 11 | [features/11-dashboard-report.md](features/11-dashboard-report.md) | Dashboard & Report | 4 | Implemented |
| 12 | [features/12-audit-log.md](features/12-audit-log.md) | Audit Trail | 4 | Implemented |

## Konvensi Penamaan

- **Fitur:** `NN-kebab-case-name.md` (contoh: `06-stock-in.md`)
- **Status:** `Draft` → `Review` → `Approved` → `Implemented`
- **Bahasa:** Penjelasan dalam Bahasa Indonesia; nama variabel/entitas/endpoint dalam English.

## Aturan Dokumentasi

1. Satu file hanya membahas satu fitur.
2. Setiap file wajib mengikuti `templates/feature-template.md`.
3. Perubahan keputusan arsitektur besar dicatat di `adr/` dengan format `NNN-title.md`.
4. Tidak ada kode implementasi di folder ini (hanya kontrak, skema, dan alur).
5. Setiap fitur wajib memiliki **Field Dictionary** yang menjelaskan fungsi tiap field, tipe, constraint, dan audit/soft delete.