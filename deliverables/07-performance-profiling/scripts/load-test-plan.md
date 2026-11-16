# Load Test Plan

## Tujuan

Mengukur performa endpoint aplikasi pada beban nyata.

## Tools

- k6 atau NBomber.
- PostgreSQL dengan data uji.
- Monitoring CPU/RAM/DB.

## Skenario

| ID | Endpoint/Alur | Beban |
|---|---|---|
| LT-01 | Login | 50 VU |
| LT-02 | Dashboard | 50 VU |
| LT-03 | Daftar transaksi + filter | 50 VU |
| LT-04 | Stock Balance | 30 VU |
| LT-05 | Stock Card 90 hari | 20 VU |
| LT-06 | Submit Stock In 100 baris | 10 VU |
| LT-07 | Approval 50 pending | 20 VU |
| LT-08 | Export CSV | 5 VU |

## Metrik

- p50/p95/p99 response time.
- Throughput request/detik.
- Error rate (< 1%).
- DB query time & lock wait.
- CPU/RAM aplikasi & database.

## Target Awal

| Metrik | Target |
|---|---|
| p95 list | < 500 ms |
| p95 report | < 2.000 ms |
| Error rate | < 1% |
| DB CPU | < 70% |
