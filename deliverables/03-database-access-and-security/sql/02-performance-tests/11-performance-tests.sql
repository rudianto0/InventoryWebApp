-- ============================================================
-- Inventory App - Performance Tests
-- PostgreSQL / InventoryAppDB
-- Catatan: dokumentasi + query EXPLAIN. Tidak menjalankan load test.
-- ============================================================

-- ------------------------------------------------------------
-- 1. Cek ukuran tabel dan index
-- ------------------------------------------------------------
SELECT
    relname AS "Table",
    pg_size_pretty(pg_total_relation_size(relid)) AS "TotalSize",
    pg_size_pretty(pg_relation_size(relid)) AS "TableSize",
    pg_size_pretty(pg_total_relation_size(relid) - pg_relation_size(relid)) AS "IndexSize"
FROM pg_catalog.pg_statio_user_tables
ORDER BY pg_total_relation_size(relid) DESC;

-- ------------------------------------------------------------
-- 2. Index yang tersedia
-- ------------------------------------------------------------
SELECT
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
ORDER BY tablename, indexname;

-- ------------------------------------------------------------
-- 3. Penggunaan index (setelah ada aktivitas)
-- ------------------------------------------------------------
SELECT
    relname AS "Table",
    indexrelname AS "Index",
    idx_scan AS "IndexScans",
    idx_tup_read AS "TuplesRead",
    idx_tup_fetch AS "TuplesFetched"
FROM pg_stat_user_indexes
ORDER BY idx_scan ASC, relname;

-- ------------------------------------------------------------
-- 4. Query lambat (jika pg_stat_statements aktif)
-- Aktifkan dulu: CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
-- Jika belum aktif, blok ini hanya memberi NOTICE.
-- ------------------------------------------------------------
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_stat_statements') THEN
        RAISE NOTICE 'pg_stat_statements aktif. Jalankan query manual:';
        RAISE NOTICE 'SELECT calls,total_exec_time,mean_exec_time,rows,left(query,160) FROM pg_stat_statements ORDER BY total_exec_time DESC LIMIT 20;';
    ELSE
        RAISE NOTICE 'pg_stat_statements belum aktif. Aktifkan jika ingin analisis query lambat.';
    END IF;
END;
$$;

-- Aktivitas saat ini
SELECT pid, usename, state, wait_event_type, wait_event,
       left(query, 160) AS query
FROM pg_stat_activity
WHERE datname = 'InventoryAppDB'
ORDER BY query_start;

-- ------------------------------------------------------------
-- 5. EXPLAIN: saldo stok per lokasi
-- ------------------------------------------------------------
EXPLAIN (ANALYZE, BUFFERS)
SELECT
    p."Sku", p."Name", l."Name" AS "Location", s."Quantity", p."ReorderLevel"
FROM "Stocks" s
JOIN "Products" p ON p."Id" = s."ProductId"
JOIN "Locations" l ON l."Id" = s."LocationId"
WHERE l."Code" = 'HQ-WH1'
ORDER BY p."Sku";

-- ------------------------------------------------------------
-- 6. EXPLAIN: kartu stok 30 hari
-- ------------------------------------------------------------
EXPLAIN (ANALYZE, BUFFERS)
SELECT *
FROM "v_stock_card"
WHERE "Sku" = 'SKU-001'
ORDER BY "CreatedAt" DESC
LIMIT 50;

-- ------------------------------------------------------------
-- 7. EXPLAIN: pending approval
-- ------------------------------------------------------------
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "v_pending_approvals" ORDER BY "SubmittedAt";

-- ------------------------------------------------------------
-- 8. EXPLAIN: reorder alert
-- ------------------------------------------------------------
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "v_reorder_alert" ORDER BY "Quantity";

-- ------------------------------------------------------------
-- 9. EXPLAIN: function stock card
-- ------------------------------------------------------------
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM fn_stock_card(
    '50000000-0000-0000-0000-000000000001',
    CURRENT_DATE - 30,
    CURRENT_DATE
);

-- ------------------------------------------------------------
-- 10. Perbandingan index (manual)
-- Jalankan query di bawah dengan index ada dan tanpa index.
-- ------------------------------------------------------------
-- Turunkan index sementara di DB uji (JANGAN di produksi):
-- DROP INDEX IF EXISTS "IX_StockMovements_ProductId_CreatedAt";
-- Jalankan EXPLAIN di atas, catat waktu.
-- Buat ulang index:
-- CREATE INDEX "IX_StockMovements_ProductId_CreatedAt"
--     ON "StockMovements" ("ProductId", "CreatedAt" DESC);
-- Jalankan EXPLAIN lagi dan bandingkan.

-- ------------------------------------------------------------
-- 11. Statistik koneksi
-- ------------------------------------------------------------
SELECT
    datname,
    numbackends AS "Connections",
    xact_commit AS "Commits",
    xact_rollback AS "Rollbacks",
    blks_read AS "BlocksRead",
    blks_hit AS "BlocksHit",
    ROUND(100.0 * blks_hit / NULLIF(blks_hit + blks_read, 0), 2) AS "BufferHitPercent"
FROM pg_stat_database
WHERE datname = 'InventoryAppDB';

-- ------------------------------------------------------------
-- 12. Lock wait
-- ------------------------------------------------------------
SELECT
    a.pid,
    a.usename,
    a.state,
    a.wait_event_type,
    a.wait_event,
    left(a.query, 160) AS query
FROM pg_stat_activity a
WHERE a.datname = 'InventoryAppDB'
  AND a.wait_event_type = 'Lock';
