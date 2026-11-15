-- ============================================================
-- Inventory App - Create Database (PostgreSQL)
-- Jalankan script ini saat terhubung ke database "postgres".
-- Di DBeaver: buka koneksi ke database postgres, lalu execute script.
-- ============================================================

-- Hentikan koneksi lain ke database target (opsional, untuk recreate)
-- SELECT pg_terminate_backend(pid)
-- FROM pg_stat_activity
-- WHERE datname = 'InventoryAppDB' AND pid <> pg_backend_pid();

DROP DATABASE IF EXISTS "InventoryAppDB";
CREATE DATABASE "InventoryAppDB"
    WITH ENCODING 'UTF8'
         TEMPLATE template0;

COMMENT ON DATABASE "InventoryAppDB" IS 'Inventory application database';
