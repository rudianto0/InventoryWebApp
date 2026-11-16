# 05 - Pelaksanaan Uji Coba

## 1. Persiapan

```powershell
dotnet restore
dotnet build Inventory.sln
```

Hasil build: **0 error**.

## 2. Perintah Eksekusi

```powershell
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj `
  --logger "trx;LogFileName=test-results.trx" `
  --collect:"XPlat Code Coverage"
```

## 3. Hasil Eksekusi

```text
Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
Duration: 10 s
```

## 4. Artefak

| File | Isi |
|---|---|
| `results/test-results.trx` | Hasil lengkap Visual Studio |
| `results/test-results.csv` | 171 test + outcome |
| `results/test-summary.csv` | Ringkasan metrik |
| `results/coverage.cobertura.xml` | Coverage |
| `results/coverage-summary.txt` | Ringkasan coverage |

## 5. Environment

| Aspek | Nilai |
|---|---|
| OS | Windows 11 |
| .NET | 10.0.15/10.0.404 |
| Database test | SQLite in-memory |
| Konfigurasi | Debug |
| Runner | xUnit 2.9.3 |

## 6. Variasi Kondisi yang Diuji

- Data valid/invalid/boundary.
- Aktif/nonaktif/soft-deleted.
- Staff/SPV/Admin.
- Draft/Submitted/Approved/Rejected.
- Stok cukup/kurang.
- Duplikat/unik.
- Konkurensi stok.

## 7. Catatan

- Test berjalan tanpa PostgreSQL.
- Tidak ada test UI/API otomatis.
- Test performa terpisah di section 07.
