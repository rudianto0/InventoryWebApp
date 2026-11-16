-- ============================================================
-- Inventory App - Database Roles and Grants
-- PostgreSQL / InventoryAppDB
-- Jalankan sebagai superuser (postgres) setelah schema dibuat.
--
-- Ganti password placeholder sebelum digunakan:
--   CHANGE_ME_OWNER
--   CHANGE_ME_APP
-- ============================================================

-- 1. Buat role (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'inventory_owner') THEN
        CREATE ROLE inventory_owner
            LOGIN PASSWORD 'CHANGE_ME_OWNER'
            NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'inventory_app') THEN
        CREATE ROLE inventory_app
            LOGIN PASSWORD 'CHANGE_ME_APP'
            NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT;
    END IF;
END;
$$;

-- 2. Kepemilikan database dan schema
ALTER DATABASE "InventoryAppDB" OWNER TO inventory_owner;
ALTER SCHEMA public OWNER TO inventory_owner;

-- 3. Pindahkan ownership objek yang sudah ada ke inventory_owner
DO $$
DECLARE
    r record;
BEGIN
    FOR r IN
        SELECT tablename FROM pg_tables
        WHERE schemaname = 'public'
    LOOP
        EXECUTE format('ALTER TABLE public.%I OWNER TO inventory_owner', r.tablename);
    END LOOP;

    FOR r IN
        SELECT sequencename FROM pg_sequences
        WHERE schemaname = 'public'
    LOOP
        EXECUTE format('ALTER SEQUENCE public.%I OWNER TO inventory_owner', r.sequencename);
    END LOOP;

    FOR r IN
        SELECT viewname FROM pg_views
        WHERE schemaname = 'public'
    LOOP
        EXECUTE format('ALTER VIEW public.%I OWNER TO inventory_owner', r.viewname);
    END LOOP;
END;
$$;

-- 4. Hak koneksi
REVOKE ALL ON DATABASE "InventoryAppDB" FROM PUBLIC;
GRANT CONNECT ON DATABASE "InventoryAppDB" TO inventory_owner;
GRANT CONNECT ON DATABASE "InventoryAppDB" TO inventory_app;

-- 5. Hak schema
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
GRANT USAGE, CREATE ON SCHEMA public TO inventory_owner;
GRANT USAGE ON SCHEMA public TO inventory_app;

-- 6. Hak runtime aplikasi (DML + execute, tanpa DDL)
GRANT SELECT, INSERT, UPDATE, DELETE
    ON ALL TABLES IN SCHEMA public TO inventory_app;

GRANT USAGE, SELECT
    ON ALL SEQUENCES IN SCHEMA public TO inventory_app;

GRANT EXECUTE
    ON ALL FUNCTIONS IN SCHEMA public TO inventory_app;

GRANT EXECUTE
    ON ALL PROCEDURES IN SCHEMA public TO inventory_app;

-- 7. Default privileges untuk objek baru milik inventory_owner
ALTER DEFAULT PRIVILEGES FOR ROLE inventory_owner IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO inventory_app;

ALTER DEFAULT PRIVILEGES FOR ROLE inventory_owner IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO inventory_app;

ALTER DEFAULT PRIVILEGES FOR ROLE inventory_owner IN SCHEMA public
    GRANT EXECUTE ON FUNCTIONS TO inventory_app;

-- 8. Pastikan inventory_app tidak bisa DDL
REVOKE CREATE ON SCHEMA public FROM inventory_app;

-- 9. Verifikasi role dan grant
SELECT rolname, rolsuper, rolcreatedb, rolcreaterole, rolcanlogin
FROM pg_roles
WHERE rolname IN ('inventory_owner', 'inventory_app')
ORDER BY rolname;

SELECT grantee, privilege_type
FROM information_schema.role_table_grants
WHERE table_schema = 'public'
  AND grantee = 'inventory_app'
ORDER BY table_name, privilege_type
LIMIT 100;
