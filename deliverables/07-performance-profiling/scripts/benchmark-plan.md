# Benchmark Plan

## Project

`tests/Inventory.Benchmarks/Inventory.Benchmarks.csproj`

## Menjalankan

```powershell
dotnet run -c Release --project tests/Inventory.Benchmarks -- --filter "*"
```

## Skenario

| Benchmark | Fokus |
|---|---|
| Ledger_Apply_100 | Waktu & memori ledger 100 changes |
| Approval_Pending_100 | N+1 approval |
| StockCard_10000 | Materialisasi movement |
| StockCheck_Naive_100 | Query per baris |
| StockCheck_Batch_100 | Query batch |
| CreatorLookup_Naive_100 | Linear lookup |
| CreatorLookup_Batch_100 | Dictionary lookup |

## Catatan

- Benchmark memakai SQLite in-memory.
- Warmup 1, iterasi 3 (short run).
- Untuk hasil stabil, gunakan minimal 10 iterasi dan lingkungan idle.
- Untuk PostgreSQL, ganti provider dan sediakan database uji.
