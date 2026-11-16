# 05 - Code Documentation

## Konteks

Dokumentasi kode: modul, parameter, algoritma, annotated code, method,
eksepsi, traceability, dan API docs.

## Dokumentasi

| File | Isi |
|---|---|
| `docs/00-ringkasan.md` | Ringkasan |
| `docs/01-identifikasi-modul.md` | Modul + ID |
| `docs/02-parameter-dan-kontrak.md` | Parameter & DTO |
| `docs/03-algoritma-dan-definisi-masalah.md` | Algoritma |
| `docs/04-annotated-code/` | 7 modul inti |
| `docs/05-dokumentasi-modul.md` | Modul |
| `docs/06-dokumentasi-method.md` | Method |
| `docs/07-katalog-eksepsi.md` | Eksepsi |
| `docs/08-generate-dokumentasi.md` | DocFX |
| `docs/09-revisi-dan-traceability.md` | Revisi |
| `Inventory-Code-Documentation.docx` | Word |

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
