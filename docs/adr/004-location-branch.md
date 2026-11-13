# ADR 004 - No Multi-Tenant; Branch on Location Master

- **Status:** Accepted
- **Date:** 2026-11-06

## Context

Aplikasi dipakai oleh satu perusahaan dengan beberapa kantor cabang /
lokasi penyimpanan. Tidak diperlukan isolasi data antar tenant.

## Decision

1. **Tidak ada** konsep `TenantId` / multi-company di seluruh model data.
2. Cabang dan gudang dimodelkan pada master **Location**:
   - `Type` = `Branch` | `Warehouse` (dapat diperluas, mis. `Store`).
   - `BranchCode` opsional untuk menandai lokasi yang berada di bawah cabang.
   - `ParentLocationId` opsional untuk hierarki (Cabang → Gudang).
3. Semua transaksi stok mereferensikan `LocationId`.
4. Laporan dapat difilter berdasarkan `LocationId` / `BranchCode`.

## Example

| Code | Name | Type | ParentLocationId | BranchCode |
|------|------|------|------------------|------------|
| HQ | Kantor Pusat | Branch | null | HQ |
| HQ-WH1 | Gudang Pusat | Warehouse | HQ | HQ |
| BR1 | Cabang Bandung | Branch | null | BR1 |
| BR1-WH1 | Gudang Bandung | Warehouse | BR1 | BR1 |

## Consequences

- **Positif:** model sederhana, tidak ada overhead tenant resolution.
- **Negatif:** jika kelak butuh isolasi data antar perusahaan, perlu migrasi
  besar. Diterima karena scope adalah satu perusahaan.
