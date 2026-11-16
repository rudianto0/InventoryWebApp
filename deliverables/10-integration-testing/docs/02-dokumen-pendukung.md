# 02 - Dokumen Pendukung Pengujian

## 1. Dokumen yang Dibutuhkan

| Dokumen | Isi | Lokasi |
|---|---|---|
| Test plan | Scope, jadwal, tools | Dokumen 10 |
| Test case sheet | Skenario & expected | `sheets/test-case-sheet.csv` |
| Result sheet | Hasil aktual | `sheets/result-sheet.csv` |
| Defect sheet | Ketidaksesuaian | `sheets/defect-sheet.csv` |
| Equipment sheet | Peralatan | `sheets/equipment-sheet.csv` |
| Test result | TRX/CSV | `results/` |
| Coverage report | Line/branch | `results/coverage-*` |
| Source code | Modul yang diuji | `src/` |
| Database schema | Tabel/relasi | `deliverables/02` |
| API/UI reference | Kontrak | `deliverables/05` |

## 2. Template Lembar

### Equipment Sheet
`No, Equipment, Version, Function, Status`

### Test Case Sheet
`TestID, Module, Scenario, Precondition, Input, Expected, Status`

### Result Sheet
`TestID, Actual, Status, Evidence, Notes`

### Defect Sheet
`DefectID, TestID, Description, Expected, Actual, RootCause, FixStatus`

## 3. Dokumen Referensi

- `deliverables/09-software-testing/` — unit test & coverage.
- `deliverables/07-performance-profiling/` — performa.
- `deliverables/02-database-implementation/` — schema & SQL.
- `deliverables/05-code-documentation/` — modul/method.

## 4. Aturan Pengisian

- TestID unik.
- Expected ditulis sebelum eksekusi.
- Actual diisi setelah eksekusi.
- Status: Pass/Fail/Blocked/Not Run.
- Evidence berupa nama test/TRX/log.
- Defect harus memiliki root cause dan status perbaikan.

## 5. Penyimpanan

Semua dokumen disimpan di section 10 dan diarsipkan bersama deliverables.
