# 03.01 - Roles and Grants

## Konteks

Membuat role database dan hak akses minimal.

## File

- `10-roles-and-grants.sql`

## Role

| Role | Fungsi |
|---|---|
| `inventory_owner` | Pemilik schema, migrasi/DDL |
| `inventory_app` | Runtime aplikasi: DML + execute |

## Prasyarat

Schema dan objek database sudah dibuat. Jalankan sebagai superuser.

## Catatan

- Ganti password placeholder `CHANGE_ME_OWNER` dan `CHANGE_ME_APP`.
- Aplikasi memakai `inventory_app`, bukan `postgres`.
- `inventory_app` tidak memiliki hak DDL.

## Verifikasi

```sql
SELECT rolname, rolsuper, rolcanlogin FROM pg_roles
WHERE rolname IN ('inventory_owner', 'inventory_app');
```
