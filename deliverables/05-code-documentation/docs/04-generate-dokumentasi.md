# 4. Generate Dokumentasi

## a. Identifikasi tools untuk generate dokumentasi dan pastikan dapat digunakan

| Tool | Versi | Fungsi |
|---|---|---|
| .NET SDK | 10.0.404 | Membangun project C# |
| DocFX | 2.81.0 | Menghasilkan dokumentasi API dan HTML |
| XML Documentation | `GenerateDocumentationFile` | Sumber keterangan API |
| DBeaver | - | Melihat database (pendukung) |

DocFX dipilih karena mendukung C#/.NET, menghasilkan HTML statis, dan membaca
komentar XML pada kode.

**Memastikan tool dapat digunakan:**

```powershell
Set-Location deliverables/05-code-documentation/config
dotnet tool restore
dotnet docfx --version
```

Jika DocFX belum terpasang:

```powershell
dotnet new tool-manifest
dotnet tool install docfx
```

## b. Lakukan generate dokumentasi dan berikan solusi jika gagal

**Perintah generate:**

```powershell
Set-Location deliverables/05-code-documentation/config
dotnet docfx ../docfx.json
```

**Hasil generate:**

```
deliverables/05-code-documentation/
├─ api-metadata/     # metadata API (*.yml)
└─ api-site/         # dokumentasi HTML
```

Halaman utama: `api-site/docs/index.html`.

**Hasil verifikasi:** DocFX berhasil membuat metadata untuk 8 project dan
halaman konseptual tanpa error.

**Jika generate gagal:**

| Masalah | Solusi |
|---|---|
| `docfx` tidak ditemukan | Jalankan `dotnet tool restore` di folder `config` |
| File XML tidak ada | Pastikan `GenerateDocumentationFile=true` |
| Build project gagal | Perbaiki error `dotnet build` lebih dulu |
| Summary API kosong | Tambahkan komentar XML pada method public |
| Tool tidak bisa diinstal | Gunakan `DefaultDocumentation.Console` atau buat Markdown manual |
| Halaman HTML kosong | Periksa bagian `content` pada `docfx.json` |
| Gambar tidak muncul | Pastikan file ada di folder yang benar |

Setelah kode berubah, jalankan ulang perintah generate agar dokumentasi selalu
sesuai dengan program.
