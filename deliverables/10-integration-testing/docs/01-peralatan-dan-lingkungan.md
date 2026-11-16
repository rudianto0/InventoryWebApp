# 01 - Peralatan dan Lingkungan Pengujian

## 1. Peralatan Pengujian

| No | Peralatan | Versi | Fungsi |
|---|---|---|---|
| 1 | Windows 11 | - | Sistem operasi |
| 2 | .NET SDK | 10.0.404 | Build & test runner |
| 3 | .NET Runtime | 10.0.15 | Eksekusi |
| 4 | xUnit | 2.9.3 | Framework test |
| 5 | FluentAssertions | 8.11.0 | Assertion |
| 6 | EF Core SQLite | 10.0.15 | Database integrasi in-memory |
| 7 | coverlet.collector | 10.1.0 | Coverage |
| 8 | Rider / Visual Studio | latest | Debug & test explorer |
| 9 | DBeaver | latest | Verifikasi database |
| 10 | BenchmarkDotNet | 0.14.0 | Performance (section 07) |

## 2. Lingkungan

| Aspek | Nilai |
|---|---|
| OS | Windows 11 |
| Runtime | .NET 10.0.15 |
| Konfigurasi | Debug |
| Database uji | SQLite in-memory |
| Test directory | `tests/Inventory.UnitTests` |
| Hasil | `deliverables/10-integration-testing/results` |
| CI | Belum tersedia |

## 3. Kebutuhan Hardware

| Komponen | Minimum | Disarankan |
|---|---|---|
| CPU | 2 core | 4 core |
| RAM | 4 GB | 8 GB |
| Disk | 2 GB free | 10 GB free |
| Jaringan | - | Untuk restore NuGet |

## 4. Kesesuaian Peralatan

| Kebutuhan | Peralatan | Status |
|---|---|---|
| Build .NET 10 | SDK 10.0.404 | Sesuai |
| Unit/integration test | xUnit | Sesuai |
| Assertion | FluentAssertions | Sesuai |
| Database uji | SQLite in-memory | Sesuai ( terbatas ) |
| Coverage | coverlet | Sesuai |
| Debug | Rider/VS | Sesuai |
| Verifikasi DB | DBeaver | Sesuai |
| PostgreSQL nyata | Belum disiapkan | Gap |

## 5. Catatan

SQLite in-memory dipilih agar test cepat, deterministik, dan tidak membutuhkan
server. Konsekuensinya, perilaku khusus PostgreSQL (constraint, locking, query
planner) belum tercakup.
