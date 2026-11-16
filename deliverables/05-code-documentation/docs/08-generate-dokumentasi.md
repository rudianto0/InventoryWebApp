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

Tool manifest sudah disediakan di `config/dotnet-tools.json`.

```powershell
Set-Location deliverables/05-code-documentation/config
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

- Metadata dari `src/**/*.csproj`.
- Konten dari `docs/*.md` di section ini.
- Output metadata ke `api-metadata/`, HTML ke `api-site/`.

## 4. Perintah Generate

Dari folder `deliverables/05-code-documentation/config`:

```powershell
dotnet docfx ../docfx.json
```

Output:

```
deliverables/05-code-documentation/
├─ api-metadata/           # metadata API (*.yml)
└─ api-site/               # HTML statis
```

Halaman utama HTML:

```
deliverables/05-code-documentation/api-site/docs/index.html
```

## 5. Hasil Verifikasi

- DocFX build: **0 warning, 0 error**.
- Metadata API berhasil dibuat untuk 8 project.
- Halaman konseptual + annotated code berhasil dirender.
- API reference tersedia di `api-metadata/` dan `api-site/`.

## 6. Solusi Jika Generate Gagal

| Masalah | Solusi |
|---|---|
| `docfx` tidak ditemukan | jalankan `dotnet tool restore` di folder `config` |
| XML doc tidak ditemukan | pastikan `GenerateDocumentationFile=true` |
| Build project gagal | perbaiki error `dotnet build` lebih dulu |
| Summary kosong | tambahkan XML comments pada public API |
| Tool gagal install (offline) | gunakan `DefaultDocumentation.Console` atau generate Markdown manual |
| Output HTML kosong | cek `docfx.json` pada bagian `content` |
| Port/gambar tidak muncul | pastikan aset ada di folder `assets/` |

## 7. Rekomendasi CI

```yaml
- dotnet build Inventory.sln
- cd deliverables/05-code-documentation/config
- dotnet tool restore
- dotnet docfx ../docfx.json
```

Arsipkan folder `api/_site` sebagai artifact.
