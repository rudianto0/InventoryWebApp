-- PostgreSQL profiling queries for InventoryAppDB

-- 1. Query paling mahal (jika pg_stat_statements aktif)
SELECT calls, total_exec_time, mean_exec_time, rows, left(query, 160) AS query
FROM pg_stat_statements
ORDER BY total_exec_time DESC
LIMIT 20;

-- 2. Aktivitas saat ini
SELECT pid, usename, state, wait_event_type, wait_event, left(query, 160) AS query
FROM pg_stat_activity
WHERE datname = 'InventoryAppDB'
ORDER BY query_start;

-- 3. Ukuran tabel/index
SELECT relname, pg_size_pretty(pg_total_relation_size(relid)) AS total_size
FROM pg_catalog.pg_statio_user_tables
ORDER BY pg_total_relation_size(relid) DESC;

-- 4. Index usage
SELECT relname, indexrelname, idx_scan, idx_tup_read, idx_tup_fetch
FROM pg_stat_user_indexes
ORDER BY idx_scan ASC;

-- 5. Rencana eksekusi stock card
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "StockMovements"
WHERE "ProductId" = '50000000-0000-0000-0000-000000000001'
ORDER BY "CreatedAt" DESC
LIMIT 100;

-- 6. Rencana eksekusi pending approval
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "v_pending_approvals" ORDER BY "SubmittedAt";

-- 7. Buffer hit ratio
SELECT datname,
       ROUND(100.0 * blks_hit / NULLIF(blks_hit + blks_read, 0), 2) AS buffer_hit_percent
FROM pg_stat_database
WHERE datname = 'InventoryAppDB';

-- 8. Lock wait
SELECT pid, usename, state, wait_event_type, wait_event, left(query, 160) AS query
FROM pg_stat_activity
WHERE datname = 'InventoryAppDB' AND wait_event_type = 'Lock';
