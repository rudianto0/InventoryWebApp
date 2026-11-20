# 05 - Code Documentation

## Konteks

Dokumentasi kode: modul, parameter, algoritma, annotated code, method,
eksepsi, traceability, dan API docs.

## Dokumentasi

`docs/` disusun mengikuti 4 langkah kerja:

| File | Butir |
|---|---|
| `docs/01-identifikasi-kode.md` | 1. Identifikasi kode program |
| `docs/02-dokumentasi-modul.md` | 2. Dokumentasi modul |
| `docs/03-dokumentasi-method.md` | 3. Dokumentasi method dan eksepsi |
| `docs/04-generate-dokumentasi.md` | 4. Generate dokumentasi |
| `docs/04-annotated-code/` | Komentar baris demi baris |
| `05-Inventory-Code-Documentation.docx` | Word |

## Generate API Docs

Konfigurasi ada di `docfx.json`, tool manifest di
`config/dotnet-tools.json`.

Dari root repository:

```powershell
Set-Location deliverables/05-code-documentation/config
dotnet tool restore
dotnet docfx ../docfx.json
```

Output:

- `api-metadata/` — metadata YAML.
- `api-site/` — HTML (dapat digenerate ulang).

## Sumber

Commit `169a891` (dokumentasi + konfigurasi DocFX dipindahkan ke section ini).
