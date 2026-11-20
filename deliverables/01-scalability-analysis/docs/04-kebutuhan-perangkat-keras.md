## 2.3 Analisis Kebutuhan Perangkat Keras

Infrastruktur adalah komputer server dan program pendukung agar aplikasi bisa
berjalan. Kebutuhannya disesuaikan dengan perkiraan jumlah pengguna dan data.

| Komponen | Fungsi sederhana |
|---|---|
| Server aplikasi | Menjalankan aplikasi web |
| Server database | Menyimpan seluruh data stok |
| Sistem operasi | Perangkat lunak dasar server |
| Web server | Melayani akses pengguna lewat browser |
| Penyimpanan SSD | Membuat baca/tulis data lebih cepat |
| Backup | Menyimpan salinan data agar aman |
| Monitoring | Memantau kondisi server |
| Jaringan aman | Menghubungkan pengguna dan database dengan aman |

### Pilihan ukuran server

| Skala | Pengguna | Server aplikasi | Server database |
|---|---|---|---|
| Kecil | 10–20 | 2 core, 4 GB RAM | 2 core, 8 GB RAM |
| Berkembang | 20–50 | 4 core, 8 GB RAM | 4 core, 16 GB RAM |
| Multi-cabang | 50–100 | 2–3 server + pembagi beban | 8 core, 32 GB RAM |

**Perlu disiapkan:** penyimpanan 100–250 GB termasuk cadangan, backup otomatis
harian, dan pemantauan server.

**Kesimpulan:** mulai dari ukuran kecil atau berkembang, lalu naikkan hanya
jika hasil pemantauan menunjukkan perlu. Dengan cara ini biaya awal tetap
wajar dan aplikasi tetap siap tumbuh.
