# 01 - Kebutuhan Uji Coba

## 1. Prosedur Uji per SDLC

| Fase | Aktivitas | Artefak |
|---|---|---|
| Requirement | Review acceptance criteria, validasi aturan bisnis | Test scenario |
| Design | Review DTO, alur approval, skema DB | Test plan |
| Implementation | Unit test per service | 171 test |
| Testing | Unit + integration + regression | Test result |
| Deployment | Smoke test setelah migrasi | Checklist |
| Maintenance | Regresi & reproduksi bug | Bug log |

## 2. Level Pengujian

| Level | Cakupan | Status |
|---|---|---|
| Unit | Service, ledger, guard | Ada (171) |
| Integration | EF Core + PostgreSQL | Belum |
| System/API | Endpoint/controller | Belum |
| UI | Razor flow | Belum |
| Performance | Benchmark/load | Ada (section 07) |
| UAT | Skenario bisnis | Manual |

## 3. Tools

| Tool | Fungsi |
|---|---|
| xUnit | Framework test |
| FluentAssertions | Assertion |
| EF Core SQLite in-memory | Database uji |
| coverlet.collector | Coverage |
| `dotnet test` | Runner + TRX |
| Rider/VS Test Explorer | Debug test |
| DBeaver | Verifikasi data |
| BenchmarkDotNet | Performa |

## 4. Standar Uji

- **AAA**: Arrange, Act, Assert.
- Nama: `Method_Scenario_Expected`.
- Satu test fokus pada satu perilaku.
- Isolasi: database baru per test.
- Deterministik: hindari waktu acak; gunakan UTC/seed.
- Tidak memakai data produksi.
- Build harus hijau sebelum test.

## 5. Kondisi Uji

| Aspek | Kondisi |
|---|---|
| .NET | 10 |
| OS | Windows 11 |
| Database | SQLite in-memory |
| Konfigurasi | Debug |
| Test runner | xUnit 2.9.3 |
| Coverage | coverlet.collector |
| Environment | Developer lokal |

## 6. Exit Criteria

- 0 failed test.
- 0 skipped test.
- Build 0 error.
- Coverage dilaporkan.
- Tidak ada bug blocker.

## 7. Risiko

| Risiko | Mitigasi |
|---|---|
| SQLite berbeda dengan PostgreSQL | Tambah integration test |
| Test tidak mencakup UI | Manual/smoke test |
| Coverage branch rendah | Tambah test boundary |
| Tidak ada CI | Tambah pipeline |
