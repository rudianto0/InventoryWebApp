# 1. Persiapkan Kode Program

Contoh pada skenario menyebut aplikasi pembayaran listrik. Standar persiapan
dan debugging yang sama diterapkan pada aplikasi **Inventory** yang dikembangkan.

## a. Siapkan kode program sesuai spesifikasi dan panduan penulisan

| Aspek | Ketentuan |
|---|---|
| Bahasa | C# |
| Framework | .NET 10 |
| Arsitektur | Clean Architecture ringan |
| Tampilan | ASP.NET Core MVC + Razor |
| Database | PostgreSQL atau SQL Server |
| ORM | EF Core 10 |
| Pengujian | xUnit |

**Panduan penulisan kode:**

- Nama class dan method memakai `PascalCase`, variabel `camelCase`.
- Gunakan `async`/`await` untuk proses database.
- Validasi data dilakukan di lapisan service.
- Jangan menyimpan password di repository.
- Setiap fitur baru memiliki unit test.

**Persiapan awal:**

```powershell
dotnet --version
dotnet restore
dotnet build Inventory.sln
dotnet test Inventory.sln
```

**Checklist persiapan:**

- [x] SDK .NET 10 tersedia.
- [x] Solution berhasil di-restore.
- [x] Build sukses tanpa error.
- [x] Unit test tersedia.
- [x] Konfigurasi database tersedia.
- [x] Alat debugging tersedia.

## b. Siapkan debugging tools untuk melihat proses modul dan mengatasi kendala

| Tool | Fungsi |
|---|---|
| Rider / Visual Studio | Breakpoint, watch, call stack |
| `dotnet build` | Melihat error dan warning kompilasi |
| `dotnet test` | Menjalankan unit test |
| `dotnet run` | Menjalankan aplikasi |
| Logging EF Core | Melihat query database |
| Browser DevTools | Melihat request dan tampilan |
| DBeaver | Memeriksa isi database |

**Langkah memulai debugging:**

1. Buka solution di Rider atau Visual Studio.
2. Pilih konfigurasi **Debug** dan project `Inventory.Web`.
3. Pastikan environment **Development**.
4. Pasang breakpoint pada method yang dicurigai.
5. Jalankan dengan **F5**.
6. Gunakan **Step Into/Over/Out**, **Watch**, dan **Call Stack**.

![Konfigurasi Debug](assets/debug-configuration.png)

**Kendala umum dan cara mengatasinya:**

| Kendala | Cara mengatasi |
|---|---|
| File `.dll` terkunci | Hentikan debugger/IIS Express, lalu build ulang |
| Razor merah di IDE | Clean, rebuild, atau invalidate cache |
| Database tidak terhubung | Periksa service PostgreSQL dan connection string |
| Migrasi gagal | Periksa schema dan jalankan script manual |
| Test gagal | Baca pesan error, perbaiki, ulangi test |
