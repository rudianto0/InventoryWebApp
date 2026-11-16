# 07 - Analisis Hasil Uji Coba

## 1. Pass Rate

- 171/171 test lulus (100%).
- Tidak ada test gagal atau skipped.
- Durasi ±10 detik → cepat untuk feedback lokal.

## 2. Coverage per Package

| Package | Line | Branch |
|---|---:|---:|
| Inventory.Application | 73.79% | 59.91% |
| Inventory.Infrastructure | 72.06% | 61.60% |
| Inventory.Domain | 77.89% | 50.00% |
| Inventory.Contracts | 84.80% | 0%* |

\*Branch pada DTO umumnya tidak relevan; line lebih penting.

## 3. Area dengan Coverage Rendah

| Area | Indikasi |
|---|---|
| `StockTransferService.List/GetDetail/Update/Delete` | 0% |
| `StockAdjustmentService.List/GetDetail/Delete` | 0% |
| `StockInService.GetDetail/MapDetail` | 0% |
| `StockOutService.GetDetail` | 0% |
| DTO/contracts tertentu | 0% (tidak dipakai test) |

## 4. Gap Pengujian

| Gap | Dampak | Rekomendasi |
|---|---|---|
| Tidak ada PostgreSQL integration test | Query/constraint DB belum teruji nyata | Tambah Testcontainers/Postgres |
| Tidak ada API test | Controller/filter/auth belum teruji end-to-end | WebApplicationFactory |
| Tidak ada UI test | Razor flow belum teruji | Smoke test manual/Playwright |
| Branch coverage 60% | Banyak cabang validasi belum | Tambah test boundary |
| Tidak ada CI | Regresi tidak otomatis | Pipeline build+test+coverage |
| Performance test terpisah | Beban HTTP belum | k6/NBomber |

## 5. Kualitas Test

**Kuat:**
- Isolasi DB in-memory.
- Fake user/directory.
- Assertion state DB, bukan hanya exception.
- Test admin auto-approve, soft delete, audit.

**Perlu ditingkatkan:**
- Test List/GetDetail sebagian modul.
- Test pagination/filter.
- Test concurrency stok.
- Test PostgreSQL-specific behavior.

## 6. Risiko

| Risiko | Level |
|---|---|
| Bug khusus PostgreSQL | Sedang |
| Regresi UI | Sedang |
| Query lambat | Sedang |
| Coverage branch rendah | Rendah–sedang |

## 7. Kesimpulan

Unit test sehat dan konsisten. Untuk kesiapan produksi, prioritaskan integration
test PostgreSQL dan API test, lalu naikkan branch coverage.
