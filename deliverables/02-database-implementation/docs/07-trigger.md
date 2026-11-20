# 7. Membuat Trigger

## 7.1 Buat trigger dengan perintah SQL

Trigger adalah aksi otomatis yang berjalan saat data ditambah, diubah, atau
dihapus. Trigger yang tersedia:

| Trigger | Tugas |
|---|---|
| Validasi quantity | Menolak jumlah 0 atau negatif |
| Stok tidak negatif | Menolak saldo di bawah nol |
| Hitung selisih | Mengisi selisih adjustment dan opname |
| Normalisasi prefix | Merapikan kode nomor dokumen |
| Pergerakan tidak nol | Menolak pencatatan pergerakan kosong |

```sql
CREATE TRIGGER trg_stockout_details_qty
BEFORE INSERT OR UPDATE ON "StockOutDetails"
FOR EACH ROW EXECUTE FUNCTION fn_validate_positive_quantity();
```

Catatan penyesuaian: pada aplikasi Inventory, contoh "tagihan setelah input
penggunaan listrik" dipadankan dengan pencatatan pergerakan stok setelah
transaksi pemakaian barang. Validasi dijalankan oleh trigger, sedangkan
pencatatan pergerakan dilakukan oleh procedure.

## 7.2 Uji kesesuaian hasil trigger

```sql
-- Harus gagal karena jumlah 0
INSERT INTO "StockOutDetails" ("Id", "Quantity")
VALUES (gen_random_uuid(), 0);
```

Uji lain: isi selisih adjustment dengan angka salah, lalu periksa apakah
selisih otomatis berubah sesuai hitungan sistem.
