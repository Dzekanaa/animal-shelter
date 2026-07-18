-- =============================================
-- SEED PODACI
-- =============================================

TRUNCATE TABLE korisnik, zivotinja, zahtevzaudomljavanje, prijavazavolontiranje, izvestaj RESTART IDENTITY CASCADE;
-- 1. Udruženje
INSERT INTO Udruzenje (id, naziv, opis, datum_osnivanja, telefon, email, adresa)
VALUES (
           1,
           'Azil za životinje "Prijatelji"',
           'Udruženje za zaštitu i udomljavanje napuštenih životinja.',
           '2018-05-10',
           '+381 11 1234567',
           'info@prijatelji-azil.rs',
           'Bulevar Oslobođenja 15, Beograd'
       ) ON CONFLICT (id) DO NOTHING;

-- 2. Korisnici
INSERT INTO Korisnik (id, tip_korisnika, korisnicko_ime, lozinka, ime, prezime, datum_registracije, telefon, email, adresa, udruzenje_id)
VALUES
    (1, 'AdminUdruzenja', 'admin_azil', 'admin123', 'Marko', 'Marković', '2020-01-15 10:00:00', '+381 64 1111111', 'marko.markovic@prijatelji-azil.rs', 'Ulica Heroja 5, Beograd', 1),
    (2, 'Volonter', 'volonter1', 'pass123', 'Ana', 'Anić', '2021-03-20 14:30:00', '+381 63 2222222', 'ana.anic@email.com', 'Nemanjina 10, Beograd', 1),
    (3, 'Volonter', 'volonter2', 'pass123', 'Petar', 'Petrović', '2022-07-01 09:15:00', '+381 62 3333333', 'petar.petrovic@email.com', 'Kralja Milana 22, Beograd', 1),
    (4, 'Volonter', 'volonter3', 'pass123', 'Jelena', 'Jelić', '2023-11-05 18:45:00', '+381 65 4444444', 'jelena.jelic@email.com', 'Vojvode Stepe 8, Novi Sad', 1),
    (5, 'Udomitelj', NULL, NULL, 'Milan', 'Milanović', '2022-05-10 11:20:00', '+381 61 5555555', 'milan.milanovic@email.com', 'Vukovarska 3, Beograd', NULL),
    (6, 'Udomitelj', NULL, NULL, 'Sandra', 'Sandravić', '2023-01-25 16:40:00', '+381 64 6666666', 'sandra.sandravic@email.com', 'Ivanjska 7, Niš', NULL),
    (7, 'Udomitelj', NULL, NULL, 'Nenad', 'Nenadić', '2024-02-14 08:30:00', '+381 63 7777777', 'nenad.nenadic@email.com', 'Partizanska 12, Kragujevac', NULL),
    (8, 'SistemskiAdmin', 'superadmin', 'admin123', 'Sistem', 'Administrator', '2020-01-01 00:00:00', '+381 60 0000000', 'admin@azil-sistem.rs', 'Glavna 1, Beograd', NULL)
    ON CONFLICT (id) DO NOTHING;

-- 3. Životinje
INSERT INTO Zivotinja (id, naziv, kategorija, starost, pol, rasa, zdravstveno_stanje, opis, status, slike, video, datum_unosa, udruzenje_id, udomitelj_id, volonter_id)
VALUES
    (1, 'Bela', 'PAS', 3, 'ZENSKI', 'Labrador', 'ZDRAVA', 'Prijateljska, voli decu.', 'DOSTUPNA', '{"beli1.jpg", "beli2.jpg"}', '{}', '2024-01-10 09:00:00', 1, NULL, NULL),
    (2, 'Maks', 'PAS', 5, 'MUSKI', 'Nemački ovčar', 'ZDRAVA', 'Zaštitnički nastrojen.', 'DOSTUPNA', '{"maks1.jpg"}', '{}', '2024-02-15 10:30:00', 1, NULL, NULL),
    (3, 'Lili', 'MACKA', 2, 'ZENSKI', 'Persijska', 'LOSIJEG_ZDRAVLJA', 'Ima blagi alergijski dermatitis.', 'ZAUZETA', '{"lili1.jpg"}', '{}', '2024-03-01 14:20:00', 1, 5, 2),
    (4, 'Šeki', 'PAS', 1, 'MUSKI', 'Mešanac', 'ZDRAVA', 'Igrač, pun energije.', 'DOSTUPNA', '{}', '{}', '2024-04-05 11:10:00', 1, NULL, 3),
    (5, 'Mici', 'MACKA', 4, 'ZENSKI', 'Evropska kratkodlaka', 'BOLESNA', 'Trenutno na terapiji, potrebna posebna nega.', 'ZAUZETA', '{"mici1.jpg"}', '{}', '2024-05-20 16:00:00', 1, 6, 4),
    (6, 'Cvrle', 'PTICE', 1, 'MUSKI', 'Papagaj', 'ZDRAVA', 'Govori nekoliko reči.', 'UDOMLJENA', '{"cvrle1.jpg"}', '{}', '2024-06-01 08:45:00', 1, 7, NULL)
    ON CONFLICT (id) DO NOTHING;

-- 4. Zahtevi za udomljavanje (neki od gostiju, pa podnosilac_id NULL)
INSERT INTO ZahtevZaUdomljavanje (id, datum_podnosenja, datum_udomljavanja, podnosilac_id, status, napomena, nacin_obavestavanja, zivotinja_id, ime, prezime, telefon, email, adresa)
VALUES
    (1, '2024-03-02 09:30:00', NULL, NULL, 'CEKA', 'Porodica sa dvoje dece, imaju dvorište.', 'EMAIL', 1, 'Stevan', 'Stevanović', '+381 62 123456', 'stevan@email.com', 'Beogradska 5'),
    (2, '2024-05-21 12:15:00', NULL, NULL, 'CEKA', 'Udomiteljica živi sama, voli mačke.', 'SMS', 5, 'Marija', 'Marić', '+381 63 987654', 'marija@email.com', 'Novosadska 12')
    ON CONFLICT (id) DO NOTHING;

-- 5. Prijave za volontiranje
INSERT INTO PrijavaZaVolontiranje (id, ime, prezime, opis, status_prijave, telefon, email, adresa, udruzenje_id, datum_podnosenja)
VALUES
    (1, 'Stefan', 'Stefanović', 'Iskusan u radu sa psima, slobodan vikendom.', 'PRIHVACEN', '+381 62 8888888', 'stefan.stefanovic@email.com', 'Beogradska 23, Beograd', 1, '2024-01-15 10:00:00'),
    (2, 'Marija', 'Marić', 'Želi da pomaže mačkama, ima iskustva sa udomljavanjem.', 'CEKA', '+381 63 9999999', 'marija.maric@email.com', 'Novosadska 5, Subotica', 1, '2024-02-20 14:30:00'),
    (3, 'Nikola', 'Nikolić', 'Student, može vikendima, vozi auto.', 'ODBIJEN', '+381 64 1010101', 'nikola.nikolic@email.com', 'Kragujevačka 12, Kragujevac', 1, '2024-03-10 08:15:00')
    ON CONFLICT (id) DO NOTHING;

-- 6. Izveštaj (primer)
INSERT INTO Izvestaj (id, tip, period, datum_generisanja, statistika, admin_id)
VALUES (
           1,
           'UDOMLJAVANJA',
           'MESECNI',
           '2024-06-30 23:59:59',
           '{"brojUdomljenih": 3, "brojVracenih": 0, "ukupnaDonacija": 12500.50, "vrsteDonacija": ["NOVAC", "HRANA", "LEKOVI"]}'::jsonb,
           1
       ) ON CONFLICT (id) DO NOTHING;

-- Resetovanje sekvenci
SELECT setval('udruzenje_id_seq', (SELECT MAX(id) FROM Udruzenje));
SELECT setval('korisnik_id_seq', (SELECT MAX(id) FROM Korisnik));
SELECT setval('zivotinja_id_seq', (SELECT MAX(id) FROM Zivotinja));
SELECT setval('zahtevzaudomljavanje_id_seq', (SELECT MAX(id) FROM ZahtevZaUdomljavanje));
SELECT setval('izvestaj_id_seq', (SELECT MAX(id) FROM Izvestaj));
SELECT setval('prijavazavolontiranje_id_seq', (SELECT MAX(id) FROM PrijavaZaVolontiranje));