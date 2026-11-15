# ADR 002 - Frontend: ASP.NET Core MVC + Razor

- **Status:** Accepted
- **Date:** 2026-11-06

## Context

Mini project dengan mayoritas layar berupa form CRUD dan tabel data.
Pilihan yang dipertimbangkan: Blazor (Server/WASM), ASP.NET Core MVC + Razor,
atau SPA terpisah.

## Decision

Gunakan **ASP.NET Core MVC + Razor Views + Bootstrap 5** sebagai UI saat ini.

- Server-side rendering, postback form + PRG (Post/Redirect/Get).
- Autentikasi memakai **Cookie** (ASP.NET Core Identity) pada fase MVC.
- Logika bisnis tetap di layer Application/Service, bukan di controller.
- UI mengakses use case melalui **`Inventory.Client`** dengan kontrak di
  **`Inventory.Contracts`**, tidak langsung ke service/entity.
- Interaksi dinamis ringan (modal, filter, konfirmasi) memakai JavaScript
  minimal + library tabel sisi server (paging/sorting di server).

## Rationale

- Paling sedikit moving parts untuk mini project: tidak ada API terpisah,
  tidak ada state management SPA, tidak ada bundler terpisah.
- Lebih mudah di-deploy sebagai satu aplikasi.
- Tim cukup familiar dengan pola MVC.

## Consequences

- **Positif:** cepat dibangun, ramah SEO (tidak relevan untuk internal),
  debugging sederhana.
- **Negatif:** interaktivitas terbatas dibanding Blazor/SPA.
- **Evolusi:** boundary `Inventory.Client` + `Inventory.Contracts` memungkinkan
  pemisahan menjadi `Inventory.Api` + client HTTP/SPA di masa depan tanpa
  menulis ulang controller UI. Lihat `adr/006-api-first-client-server.md`.
