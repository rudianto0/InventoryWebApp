# 6. White Box Testing

White box testing melengkapi pengujian black-box/unit yang sudah ada. Fokusnya
bukan hanya “hasil sesuai”, tetapi juga **jalur kode mana yang dieksekusi**:
percabangan, kondisi, loop, dan jalur eksepsi.

## 6.1 Tujuan

- Memastikan cabang keputusan penting dieksekusi oleh test.
- Memetakan test yang ada ke method dan branch yang diuji.
- Menemukan cabang yang belum tercakup.
- Menjadi dasar peningkatan branch coverage.

## 6.2 Metode yang Digunakan

| Metode | Fokus |
|---|---|
| Statement coverage | Baris penting dieksekusi |
| Branch/decision coverage | `if`, `else`, `switch`, guard |
| Condition coverage | Kombinasi kondisi benar/salah |
| Path coverage | Jalur utama method |
| Loop testing | 0 iterasi, 1 iterasi, banyak iterasi |
| Exception path | `ValidationException`, data tidak ditemukan, konflik |

## 6.3 Tools

| Tool | Fungsi |
|---|---|
| xUnit | Menjalankan test |
| FluentAssertions | Memeriksa hasil |
| EF Core SQLite in-memory | Database uji |
| coverlet.collector | Mengukur line/branch coverage |
| `dotnet test` | Runner + TRX |

## 6.4 Target Method dan Bukti Test

### StockLedger.ApplyAsync

| Branch / Path | Test evidence |
|---|---|
| Quantity 0 -> skip | `StockLedgerExtraTests.ApplyAsync_SkipsZeroQuantity` |
| Stok belum ada -> buat stok | `StockLedgerTests.ApplyAsync_IncreasesStockAndCreatesMovement` |
| Stok ada -> update | `StockLedgerExtraTests.ApplyAsync_DecreasesStockWithinBalance` |
| Saldo negatif -> exception | `StockLedgerTests.ApplyAsync_RejectsNegativeBalance` |
| Beberapa perubahan key sama | `StockLedgerExtraTests.ApplyAsync_AccumulatesChangesForSameKey` |
| Adjustment sampai nol | `StockLedgerExtraTests.ApplyAsync_AdjustmentCanBringStockToZero` |
| Transfer dua lokasi | `StockLedgerExtraTests.ApplyAsync_TransferMovesStockBetweenLocations` |
| Pesan error memuat produk/lokasi | `StockLedgerExtraTests.ApplyAsync_InsufficientMessageContainsProductAndLocation` |

### StockOutService

| Branch / Path | Test evidence |
|---|---|
| Produk duplikat / nonaktif | `StockOutServiceTests.Create_RejectsDuplicateOrInactiveProduct` |
| Baris kosong / lokasi tidak ada / qty invalid | `StockOutServiceTests.Create_RejectsEmptyLinesInactiveLocationOrInvalidQuantity` |
| Stok kurang saat submit | `StockOutServiceTests.Submit_RejectsInsufficientStock` |
| Stok berubah saat approve | `StockOutServiceTests.Approve_RechecksStockAndRejectsWhenInsufficient` |
| Approve dokumen sendiri / status salah | `StockOutServiceTests.Approve_RejectsOwnDocumentOrNonSubmitted` |
| Atasan tidak ada / nonaktif | `StockOutServiceTests.Submit_RejectsMissingOrInactiveParent` |

### StockInService dan Approval

| Branch / Path | Test evidence |
|---|---|
| Admin auto-approve | `StockInServiceTests.AdminSubmit_AutoApprovesAndAppliesStock` |
| Approve dokumen bukan Submitted | `StockInServiceTests.Approve_RejectsNonSubmittedDocument` |
| Approve dokumen sendiri | `StockInServiceTests.Approve_RejectsOwnDocument` |
| Submit tanpa atasan | `StockInServiceTests.Submit_RejectsWhenNoParent` |
| Approver = diri sendiri | `StockInServiceTests.Submit_RejectsSelfApprover` |
| Approver nonaktif | `StockInServiceTests.Submit_RejectsInactiveParent` |
| Update oleh non-owner | `StockInServiceTests.Update_RejectsNonOwner` |
| Update status tidak editable | `StockInServiceTests.Update_RejectsNonEditableStatus` |

### LocationService.CreatesCycleAsync

| Branch / Path | Test evidence |
|---|---|
| Parent tidak ada / nonaktif / tipe salah | `LocationServiceTests.Create_RejectsMissingInactiveOrWrongParent` |
| Parent = diri sendiri | `LocationServiceTests.Update_RejectsSelfAsParent` |
| Siklus tidak langsung | `LocationServiceTests.Update_RejectsHierarchyCycle` |
| Parent valid tanpa siklus | `LocationServiceTests.Create_AllowsBranchChildUnderNonBranchParent` |

### ReportService.GetStockCardAsync

| Branch / Path | Test evidence |
|---|---|
| Tanpa ProductId -> list kosong | `ReportServiceTests.StockCard_ReturnsEmptyWithoutProduct` |
| Opening balance + running balance | `ReportServiceTests.StockCard_ComputesOpeningAndRunningBalance` |
| Filter saldo + pagination | `ReportServiceTests.StockBalance_AppliesFiltersAndPagination` |

### DocumentNumberService.NextAsync

| Branch / Path | Test evidence |
|---|---|
| Sequence baru + increment | `DocumentNumberServiceTests.Next_UsesFormatAndIncrements` |
| Reset per bulan + prefix berbeda | `DocumentNumberServiceTests.Next_ResetsPerMonthAndKeepsPrefixesIndependent` |

### AppDbContext.SaveChangesAsync

| Branch / Path | Test evidence |
|---|---|
| Soft delete + query filter | `SoftDeleteTests.Delete_MasterData_IsSoftDeletedAndHiddenFromQueries` |
| Audit create/update | `SoftDeleteTests.CreateAndUpdate_FillAuditUserFields` |
| Audit log delete | `SoftDeleteTests.Delete_WritesAuditLogWithStringEntityId` |
| Audit old/new values | `AuditTrailExtraTests.Update_WritesOldAndNewValues` |

## 6.5 Analisis Basis Path

### StockLedger.ApplyAsync

Path utama:
1. `quantity == 0` -> `continue`.
2. `quantity != 0`, stok null -> buat stok, lalu movement.
3. `quantity != 0`, stok ada, `newQuantity >= 0` -> update, movement.
4. `quantity != 0`, stok ada, `newQuantity < 0` -> `ValidationException`.

Estimasi independent path: 4 path utama. Loop diuji dengan 0 perubahan
(skip), 1 perubahan, dan banyak perubahan pada key yang sama.

### LocationService.CreatesCycleAsync

Path utama:
1. `parentId` null -> return false.
2. `cursor == currentId` -> return true.
3. `visited.Add` gagal -> return true.
4. rantai parent berakhir -> return false.

Estimasi independent path: 4 path utama.

## 6.6 Hasil Coverage

| Metrik | Nilai |
|---|---:|
| Total test | 171 |
| Passed | 171 |
| Failed | 0 |
| Line coverage | 74,69% |
| Branch coverage | 60,17% |

Sumber:
- `results/test-results.csv`
- `results/coverage-summary.txt`
- `results/coverage.cobertura.xml`

## 6.7 Gap yang Masih Ada

| Gap | Dampak | Rekomendasi |
|---|---|---|
| Beberapa kombinasi kondisi belum eksplisit | Branch tertentu belum terbukti | Tambah test kombinasi kondisi |
| UserService branch coverage rendah | Error Identity/role belum semua teruji | Tambah test validasi user/role |
| PostgreSQL nyata belum diuji | Perilaku provider berbeda | Integration test PostgreSQL |
| API/UI belum diuji | Jalur HTTP/Razor belum tercakup | API test + UI smoke test |
| Belum ada mutation test | Kualitas assertion belum diukur | Pertimbangkan Stryker.NET |

## 6.8 Kesimpulan

White box testing pada aplikasi Inventory sudah tercermin melalui test yang
menguji cabang penting: ledger, transaksi stok, approval, hierarki lokasi,
laporan, nomor dokumen, audit, dan sorting. Branch coverage saat ini **60,17%**;
peningkatan berikutnya sebaiknya difokuskan pada `UserService`, kombinasi
kondisi, dan integration test PostgreSQL/API/UI.
