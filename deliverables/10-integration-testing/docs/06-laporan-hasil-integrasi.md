# 06 - Laporan Hasil Pengujian Integrasi

## 1. Identitas

| Item | Nilai |
|---|---|
| Proyek | Inventory Web App |
| Section | 10 - Integration Testing |
| Tanggal | 2026-11-16 |
| Environment | Windows 11, .NET 10 |
| Database uji | SQLite in-memory |
| Test suite | 171 test |

## 2. Peralatan yang Digunakan

| Peralatan | Versi |
|---|---|
| .NET SDK | 10.0.404 |
| .NET Runtime | 10.0.15 |
| xUnit | 2.9.3 |
| FluentAssertions | 8.11.0 |
| EF Core SQLite | 10.0.15 |
| coverlet.collector | 10.1.0 |
| Rider/Visual Studio | latest |
| DBeaver | latest |

## 3. Kondisi Selama Pengujian

- Build sukses 0 error.
- Database in-memory baru per test.
- Tidak ada akses jaringan/DB eksternal.
- Test berjalan ±7 detik.
- Tidak ada test gagal/skipped.

## 4. Data yang Diimplementasikan

| TestID | Input |
|---|---|
| INT-01 | qty 7, creator 10, approver 20 |
| INT-02 | stok 10, qty 4 |
| INT-03 | stok 2, qty 5 |
| INT-04 | WH1 10, qty 4 |
| INT-05 | system 10, counted 7 |
| INT-06 | system 10, counted 12 |
| INT-07 | actor 99 |
| INT-08 | admin |
| INT-09 | product + stock |
| INT-10 | CRUD category |

## 5. Hasil

| Metrik | Nilai |
|---|---:|
| Test | 171 |
| Passed | 171 |
| Failed | 0 |
| Line coverage | 74.69% |
| Branch coverage | 60.17% |

## 6. Analisis sesuai Standar

- Setiap skenario memiliki expected dan actual.
- Status dicatat pada `result-sheet.csv`.
- Evidence berupa nama test/TRX.
- Ketidaksesuaian cakupan dicatat di `defect-sheet.csv`.
- Kesimpulan: integrasi modul inti sesuai.

## 7. Lampiran

- `sheets/equipment-sheet.csv`
- `sheets/test-case-sheet.csv`
- `sheets/result-sheet.csv`
- `sheets/defect-sheet.csv`
- `results/test-results.trx`
- `results/test-summary.csv`
- `results/coverage-summary.txt`
