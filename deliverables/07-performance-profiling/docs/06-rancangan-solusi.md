# 06 - Rancangan Solusi

## 1. Prinsip

- Kurangi **jumlah round-trip** sebelum optimasi mikro.
- Batasi **materialisasi** data.
- Gunakan **index** dan query selective.
- Cache data yang jarang berubah.
- Ukur sebelum/sesudah.

## 2. Rancangan per Bottleneck

### B1 — Batch stock ledger
**Desain:**
1. Kumpulkan semua `(ProductId, LocationId)` unik.
2. Ambil stok sekali dengan `WHERE (ProductId, LocationId) IN (...)` atau
   query per batch.
3. Hitung perubahan di memori.
4. Insert movement sekali.
5. `SaveChanges` sekali.

**Target:** O(1) query stok per batch; alokasi turun.

### B2 — Document number
**Desain:**
- Atomic increment:
  ```sql
  INSERT INTO "DocumentSequences" (...)
  VALUES (...)
  ON CONFLICT ("Prefix","Year","Month")
  DO UPDATE SET "LastNumber" = "DocumentSequences"."LastNumber" + 1
  RETURNING "LastNumber";
  ```
- Hindari `SaveChanges` per nomor; ambil nomor dalam satu transaksi.

### B3 — Approval
**Desain:**
- Satu query header + join/batch creator.
- `GetDisplayNamesAsync` sekali.
- Hilangkan query per item.

### B4/B5 — Stock check & system qty
**Desain:**
- Ambil semua stok dengan `WHERE ProductId IN (...)`.
- Validasi di memori.
- Untuk adjustment/opname, ambil system qty sekali per batch.

### B6 — Stock card
**Desain:**
- Pagination `Skip/Take`.
- Default rentang tanggal (mis. 30/90 hari).
- Streaming untuk export.

### B7 — Auth
**Desain:**
- Cache user aktif/security stamp 30–60 detik.
- Atau gunakan `SecurityStampValidator` interval.

### B8 — Audit
**Desain:**
- Tulis `AuditLogs` dalam `SaveChanges` yang sama (hindari commit kedua).

### B9 — Sequence contention
**Desain:**
- Atomic upsert + retry terbatas.
- Alternatif: PostgreSQL sequence per prefix/bulan.

## 3. Rancangan Pengukuran

- BenchmarkDotNet untuk method.
- Prototype batch di benchmark project.
- `EXPLAIN ANALYZE` untuk query.
- Bandingkan baseline vs after.

## 4. Prioritas Implementasi

| Prioritas | Solusi | Risiko |
|---|---|---|
| 1 | Pagination stock card | Rendah |
| 2 | Batch stock check | Rendah |
| 3 | Batch approval creator | Rendah |
| 4 | Batch ledger + doc number | Sedang |
| 5 | Auth cache | Sedang |
| 6 | Audit single save | Sedang |

## 5. Kompatibilitas

- Perubahan tidak mengubah kontrak API/UI.
- Perlu regression test untuk stok, approval, laporan.
- Migrasi database tidak diperlukan untuk optimasi query.
