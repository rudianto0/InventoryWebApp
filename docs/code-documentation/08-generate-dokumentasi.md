# 08 - Generate Dokumentasi

## 1. Tools yang Digunakan

| Tool | Versi | Fungsi |
|---|---|---|
| .NET SDK | 10.0.404 | Build project |
| DocFX | 2.81.0 | Generate API reference + HTML |
| XML Documentation | `GenerateDocumentationFile` | Sumber metadata API |

DocFX dipilih karena:

- Mendukung C#/.NET 10.
- Menghasilkan HTML statis + API reference.
- Terintegrasi dengan XML documentation comments.
- Dapat dijalankan lokal/CI.

## 2. Instalasi

Tool manifest sudah disediakan di `dotnet-tools.json`.

```powershell
dotnet tool restore
dotnet docfx --version
```

Jika belum ada manifest:

```powershell
dotnet new tool-manifest
dotnet tool install docfx
```

## 3. Konfigurasi

### XML documentation

Project utama sudah memiliki:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
```

`1591` disenyapkan agar build tidak gagal saat summary belum lengkap.

### `docfx.json`

- Metadata dari seluruh `src/**/*.csproj`.
- Konten dari Markdown dokumentasi.
- Output ke `docs/code-documentation/api`.

## 4. Perintah Generate

```powershell
dotnet docfx docfx.json
```

Output:

```
docs/code-documentation/api/
├─ *.yml                  # metadata API
└─ _site/                 # HTML statis
```

Halaman utama HTML:

```
docs/code-documentation/api/_site/docs/code-documentation/index.html
```

## 5. Hasil Verifikasi

- DocFX build: **0 warning, 0 error**.
- Metadata API berhasil dibuat untuk 8 project.
- Halaman konseptual + annotated code berhasil dirender.
- API reference tersedia di folder `api/`.

## 6. Solusi Jika Generate Gagal

| Masalah | Solusi |
|---|---|
| `docfx` tidak ditemukan | `dotnet tool restore` atau install ulang |
| XML doc tidak ditemukan | pastikan `GenerateDocumentationFile=true` |
| Build project gagal | perbaiki error `dotnet build` lebih dulu |
| Summary kosong | tambahkan XML comments pada public API |
| Tool gagal install (offline) | gunakan `DefaultDocumentation.Console` atau generate Markdown manual |
| Output HTML kosong | cek `docfx.json` pada bagian `content` |
| Port/gambar tidak muncul | pastikan aset ada di folder `assets/` |

## 7. Rekomendasi CI

```yaml
- dotnet tool restore
- dotnet build Inventory.sln
- dotnet docfx docfx.json
```

Arsipkan folder `api/_site` sebagai artifact.
