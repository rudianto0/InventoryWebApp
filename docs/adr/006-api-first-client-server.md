# ADR 006 - API-first Client/Server Boundary

- **Status:** Accepted
- **Date:** 2026-11-15

## Context

Aplikasi saat ini menggabungkan UI (Razor MVC) dan service dalam satu host.
Controller langsung memanggil service Application dan sebagian view memakai
entity Domain. Kondisi ini menyulitkan pemisahan backend dan frontend di masa
depan.

## Decision

Terapkan boundary **API-first** secara bertahap tanpa big-bang rewrite:

1. **`Inventory.Contracts`** menjadi satu-satunya kontrak lintas boundary
   (request/response DTO, enum publik, `PagedResult<T>`). Tidak berisi entity.
2. **`Inventory.Client`** menyediakan interface per area dan implementasi
   in-process (`InProcess*Client`). Web hanya bergantung pada interface ini.
3. Service Application tidak lagi mengembalikan entity; hasil dipetakan ke
   response DTO di Contracts.
4. **`Inventory.Api`** akan menjadi host REST `/api/v1` dengan ProblemDetails,
   OpenAPI, dan dual auth (cookie + JWT).
5. Implementasi `Http*Client` menggantikan in-process pada fase berikutnya
   tanpa mengubah controller UI.
6. Web berperan sebagai BFF: menyimpan token server-side; browser tidak
   menerima token mentah.

## Consequences

**Positif**
- UI tidak lagi bergantung pada entity/DB.
- Pemisahan BE/FE dapat dilakukan dengan mengganti registrasi DI client.
- Kontrak dapat digenerate menjadi OpenAPI/TypeScript client.

**Negatif / Risiko**
- Refactor entity → DTO menyentuh banyak file; dilakukan per modul.
- Fase transisi masih mengomposisikan Infrastructure di Web.
- Perlu disiplin boundary (architecture test) agar tidak bocor lagi.

## Migration Phases

| Phase | Deliverable |
|-------|-------------|
| 0 | Contracts + dependency guardrails |
| 1 | Response DTO menggantikan entity di service/UI |
| 2 | Client interface + in-process implementation (selesai) |
| 3 | Inventory.Api + OpenAPI + dual auth |
| 4 | HTTP client + Web swap + Account pindah ke API |
| 5 | Deploy terpisah, CORS, health check, generated client |
