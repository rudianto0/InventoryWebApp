# Dokumentasi Kode Program

Dokumen ini mengidentifikasi modul, parameter, algoritma, method, eksepsi, dan
cara menghasilkan dokumentasi API dari source code aplikasi Inventory.

## Daftar Dokumen

| File | Isi |
|---|---|
| `00-ringkasan.md` | Ringkasan dan hasil |
| `01-identifikasi-modul.md` | Modul + Module ID + file |
| `02-parameter-dan-kontrak.md` | Parameter, DTO, validasi |
| `03-algoritma-dan-definisi-masalah.md` | Cara kerja algoritma per modul |
| `04-annotated-code/` | Kode berkomentar per baris (modul inti) |
| `05-dokumentasi-modul.md` | Identitas, kegunaan, dependensi |
| `06-dokumentasi-method.md` | Signature, parameter, return, eksepsi |
| `07-katalog-eksepsi.md` | Daftar exception |
| `08-generate-dokumentasi.md` | DocFX + fallback |
| `09-revisi-dan-traceability.md` | Revisi & traceability |
| `../api-metadata/` | Hasil generate dokumentasi API |
| `../Inventory-Code-Documentation.docx` | Dokumen Word |

## Prinsip

- Setiap modul memiliki ID unik (`MOD-*`) untuk pelacakan.
- Setiap method penting memiliki ID (`MTH-*`) dan kategori
  fungsi/prosedur.
- Komentar baris demi baris disediakan untuk modul inti.
- XML documentation comments digunakan untuk API reference.
- Dokumen direvisi bila kode berubah.
