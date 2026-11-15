# 03 - Algoritma dan Definisi Masalah

## 1. Stock Ledger (`MOD-APP-LEDGER`)

**Masalah:** menjaga saldo stok per `(ProductId, LocationId)` tetap konsisten
dan tidak negatif saat terjadi mutasi.

**Algoritma:**

```
PROCEDURE Apply(changes):
    FOR EACH change IN changes DO
        IF change.Quantity = 0 THEN CONTINUE
        stock ← FIND Stock(ProductId, LocationId)
        IF stock = NULL THEN
            stock ← NEW Stock(ProductId, LocationId, Quantity=0)
            SAVE stock
        END IF
        newQuantity ← stock.Quantity + change.Quantity
        IF newQuantity < 0 THEN
            THROW ValidationException
        END IF
        stock.Quantity ← newQuantity
        stock.Version ← stock.Version + 1
        stock.UpdatedAt ← NOW
        INSERT StockMovement(change)
    END FOR
END PROCEDURE
```

**Kompleksitas:** waktu O(k) untuk k perubahan; memori O(1) tambahan per
perubahan (entity di-track). Risiko: konflik baris `Stocks` yang sama.

## 2. Stock In (`MOD-APP-STOCKIN`)

**Masalah:** mencatat penerimaan barang dengan approval satu level.

**Algoritma:**

```
FUNCTION Create(input):
    VALIDATE supplier, location, lines, qty
    header ← NEW StockInHeader(Draft, DocNo)
    header.Details ← input.Lines
    SAVE header
    RETURN header

PROCEDURE Submit(id):
    header ← LOAD(id)
    ENSURE owner
    ENSURE editable (Draft/Rejected)
    VALIDATE stored
    IF admin THEN
        ApproveAndApply(header)
    ELSE
        approver ← GET parent(currentUser)
        VALIDATE approver exist, active, not self
        header.Status ← Submitted
        header.ApproverId ← approver
        SAVE
    END IF

PROCEDURE Approve(id):
    header ← LOAD(id)
    ENSURE can approve
    VALIDATE stored
    BEGIN TRANSACTION
        header.Status ← Approved
        ApplyLedger(header.Details)
        SAVE
        COMMIT
    END
END PROCEDURE
```

**Kompleksitas:** O(n) validasi baris + O(k) ledger.

## 3. Stock Out (`MOD-APP-STOCKOUT`)

**Masalah:** mengeluarkan stok tanpa membuat saldo negatif.

**Algoritma:**

```
PROCEDURE Submit(id):
    header ← LOAD(id)
    ENSURE owner + editable
    FOR EACH line IN header.Details DO
        available ← SUM Stock(line.ProductId, header.LocationId)
        IF available < line.Quantity THEN THROW
    END FOR
    header.Status ← Submitted
    SAVE

PROCEDURE Approve(id):
    RE-CHECK stock
    BEGIN TRANSACTION
        ApplyLedger(negative quantities)
        header.Status ← Approved
        COMMIT
END PROCEDURE
```

**Kompleksitas:** O(n) query stok; memori O(n).

## 4. Transfer (`MOD-APP-TRANSFER`)

**Masalah:** memindahkan stok antar lokasi secara atomik.

```
PROCEDURE Approve(id):
    VALIDATE from/to active, stock at from sufficient
    BEGIN TRANSACTION
        ApplyLedger(TransferOut from, qty negative)
        ApplyLedger(TransferIn to, qty positive)
        header.Status ← Approved
        COMMIT
END PROCEDURE
```

**Kompleksitas:** O(n) detail; dua movement per detail.

## 5. Adjustment (`MOD-APP-ADJUSTMENT`)

**Masalah:** mengoreksi saldo berdasarkan hitung fisik.

```
FUNCTION Difference(counted, system):
    RETURN counted - system

PROCEDURE Create(input):
    VALIDATE reason, location, products, counted >= 0
    FOR EACH line:
        system ← GET Stock
        detail ← (system, counted, difference)
    SAVE

PROCEDURE Approve(id):
    REFRESH system quantities
    ApplyLedger(difference)
    header.Status ← Approved
END PROCEDURE
```

## 6. Opname (`MOD-APP-OPNAME`)

```
PROCEDURE Create(location):
    stocks ← GET all Stocks WHERE location
    IF empty THEN THROW
    CREATE header + detail per stock (counted = null)
    SAVE

PROCEDURE Submit(id):
    ENSURE all counted filled
    ENSURE no other Submitted opname at location
    REFRESH system, difference
    header.Status ← Submitted
END PROCEDURE

PROCEDURE Approve(id):
    REFRESH system, difference
    ApplyLedger(difference)
    header.Status ← Approved
END PROCEDURE
```

## 7. Approval Guard (`MOD-APP-APPROVAL`)

```
FUNCTION EnsureOwner(createdBy, current, isAdmin):
    IF createdBy ≠ current AND NOT isAdmin THEN THROW
END FUNCTION

FUNCTION EnsureEditable(header):
    IF header.Status NOT IN (Draft, Rejected) THEN THROW
END FUNCTION

PROCEDURE Submit(header, current, directory):
    approver ← directory.GetParent(current)
    IF approver NULL THEN THROW
    IF approver = current THEN THROW
    IF NOT directory.IsActive(approver) THEN THROW
    header.ApproverId ← approver
    header.Status ← Submitted
END PROCEDURE

FUNCTION EnsureCanApprove(header, createdBy, current, isAdmin):
    IF createdBy = current THEN THROW
    IF header.Status ≠ Submitted THEN THROW
    IF header.ApproverId NULL THEN THROW
    IF header.ApproverId = current OR isAdmin THEN RETURN
    THROW
END FUNCTION
```

## 8. AppDbContext (`MOD-INF-DBCONTEXT`)

**Masalah:** audit otomatis dan soft delete.

```
PROCEDURE SaveChanges:
    snapshots ← CAPTURE changes (sebelum mutasi)
    FOR EACH entry:
        IF Added: CreatedAt/By
        IF Modified: UpdatedAt/By
        IF Deleted dan soft-deletable: IsDeleted, DeletedAt/By, IsActive=false
    SAVE
    IF snapshots: WRITE AuditLogs
END PROCEDURE
```

## 9. Report (`MOD-APP-REPORT`)

- Stock balance: join Stock/Product/Unit/Location + filter + pagination.
- Stock card: filter movement by product/tanggal + running balance.
- Movement/reorder: filter + pagination.
- Kompleksitas: O(n) data terfilter; index membantu.

## 10. Document Number (`MOD-INF-DOCNUM`)

```
FUNCTION Next(prefix, date):
    key ← (prefix, year, month)
    sequence ← INCREMENT DocumentSequences[key]
    RETURN prefix + '-' + YYYYMM + '-' + PAD(sequence,4)
END FUNCTION
```

Kompleksitas O(1) (satu baris sequence), tetapi rawan contention.
