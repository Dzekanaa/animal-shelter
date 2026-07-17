-- =============================================
-- 1. Enumeracije (ENUM)
-- =============================================

CREATE TYPE tip_korisnika AS ENUM (
    'SistemskiAdmin',
    'AdminUdruzenja',
    'Volonter',
    'Udomitelj'
);

CREATE TYPE kategorija_zivotinje AS ENUM (
    'PAS',
    'MACKA',
    'ZECEVI',
    'PTICE',
    'OSTALO'
);

CREATE TYPE status_zivotinje AS ENUM (
    'DOSTUPNA',
    'ZAUZETA',
    'UDOMLJENA'
);

CREATE TYPE pol_zivotinje AS ENUM (
    'MUSKI',
    'ZENSKI',
    'NEPOZNATO'
);

CREATE TYPE zdravlje_zivotinje AS ENUM (
    'ZDRAVA',
    'LOSIJEG_ZDRAVLJA',
    'BOLESNA'
);

CREATE TYPE nacin_obavestavanja AS ENUM (
    'SMS',
    'EMAIL',
    'RUCNO'
);

CREATE TYPE status_zahteva AS ENUM (
    'CEKA',
    'PRIHVACEN',
    'ODBIJEN',
    'PREUZETA'
);

CREATE TYPE tip_izvestaja AS ENUM (
    'UDOMLJAVANJA',
    'DONACIJE'
);

CREATE TYPE period_izvestaja AS ENUM (
    'DNEVNI',
    'NEDELJNI',
    'MESECNI'
);

CREATE TYPE status_prijave AS ENUM (
    'CEKA',
    'ODBIJEN',
    'PRIHVACEN'
);

-- =============================================
-- 2. Tabele
-- =============================================

CREATE TABLE Udruzenje (
                           id SERIAL PRIMARY KEY,
                           naziv VARCHAR(255) NOT NULL,
                           opis TEXT,
                           datum_osnivanja DATE NOT NULL,
                           telefon VARCHAR(50),
                           email VARCHAR(255),
                           adresa TEXT
);

CREATE TABLE Korisnik (
                          id SERIAL PRIMARY KEY,
                          tip_korisnika tip_korisnika NOT NULL,
                          korisnicko_ime VARCHAR(100) UNIQUE NOT NULL,
                          lozinka VARCHAR(255) NOT NULL,
                          ime VARCHAR(100) NOT NULL,
                          prezime VARCHAR(100) NOT NULL,
                          datum_registracije TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                          telefon VARCHAR(50),
                          email VARCHAR(255),
                          adresa TEXT,
                          udruzenje_id INTEGER REFERENCES Udruzenje(id) ON DELETE SET NULL
);

-- (CHECK ograničenje za udruzenje_id ćemo provjeriti triggerom)

CREATE TABLE Zivotinja (
                           id SERIAL PRIMARY KEY,
                           naziv VARCHAR(100) NOT NULL,
                           kategorija kategorija_zivotinje NOT NULL,
                           starost INTEGER CHECK (starost >= 0),
                           pol pol_zivotinje NOT NULL,
                           rasa VARCHAR(100),
                           zdravstveno_stanje zdravlje_zivotinje NOT NULL,
                           opis TEXT,
                           status status_zivotinje NOT NULL,
                           slike TEXT[] DEFAULT '{}',
                           video TEXT[] DEFAULT '{}',
                           datum_unosa TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                           udruzenje_id INTEGER NOT NULL REFERENCES Udruzenje(id) ON DELETE CASCADE,
                           udomitelj_id INTEGER REFERENCES Korisnik(id) ON DELETE SET NULL,
                           volonter_id INTEGER REFERENCES Korisnik(id) ON DELETE SET NULL
);

CREATE TABLE ZahtevZaUdomljavanje (
                                      id SERIAL PRIMARY KEY,
                                      datum_podnosenja TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                      datum_udomljavanja TIMESTAMP,
                                      podnosilac_id INTEGER NOT NULL REFERENCES Korisnik(id) ON DELETE CASCADE,
                                      status status_zahteva NOT NULL DEFAULT 'CEKA',
                                      napomena TEXT,
                                      nacin_obavestavanja nacin_obavestavanja,
                                      zivotinja_id INTEGER NOT NULL REFERENCES Zivotinja(id) ON DELETE CASCADE
);

CREATE TABLE Izvestaj (
                          id SERIAL PRIMARY KEY,
                          tip tip_izvestaja NOT NULL,
                          period period_izvestaja NOT NULL,
                          datum_generisanja TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                          statistika JSONB,
                          admin_id INTEGER NOT NULL REFERENCES Korisnik(id) ON DELETE CASCADE
);

CREATE TABLE PrijavaZaVolontiranje (
                                       id SERIAL PRIMARY KEY,
                                       ime VARCHAR(100) NOT NULL,
                                       prezime VARCHAR(100) NOT NULL,
                                       opis TEXT,
                                       status_prijave status_prijave NOT NULL DEFAULT 'CEKA',
                                       telefon VARCHAR(50),
                                       email VARCHAR(255),
                                       adresa TEXT
);

-- =============================================
-- 3. Indeksi (sada nakon tabela)
-- =============================================

CREATE INDEX idx_korisnik_tip ON Korisnik(tip_korisnika);
CREATE INDEX idx_korisnik_udruzenje ON Korisnik(udruzenje_id);

CREATE INDEX idx_zivotinja_udruzenje ON Zivotinja(udruzenje_id);
CREATE INDEX idx_zivotinja_udomitelj ON Zivotinja(udomitelj_id);
CREATE INDEX idx_zivotinja_volonter ON Zivotinja(volonter_id);
CREATE INDEX idx_zivotinja_status ON Zivotinja(status);

CREATE INDEX idx_zahtev_zivotinja ON ZahtevZaUdomljavanje(zivotinja_id);
CREATE INDEX idx_zahtev_podnosilac ON ZahtevZaUdomljavanje(podnosilac_id);
CREATE INDEX idx_zahtev_status ON ZahtevZaUdomljavanje(status);

CREATE INDEX idx_izvestaj_admin ON Izvestaj(admin_id);
CREATE INDEX idx_izvestaj_datum ON Izvestaj(datum_generisanja);

-- =============================================
-- 4. Trigeri za provjeru konzistentnosti
-- =============================================

-- 4.1. Funkcija za provjeru na Korisnik (samo AdminUdruzenja smije imati udruzenje_id)
CREATE OR REPLACE FUNCTION check_korisnik_udruzenje()
RETURNS TRIGGER AS $$
BEGIN
    IF (NEW.tip_korisnika = 'AdminUdruzenja' AND NEW.udruzenje_id IS NULL) THEN
        RAISE EXCEPTION 'AdminUdruzenja mora imati dodeljeno udruzenje_id';
END IF;
    IF (NEW.tip_korisnika != 'AdminUdruzenja' AND NEW.udruzenje_id IS NOT NULL) THEN
        RAISE EXCEPTION 'Samo AdminUdruzenja sme imati udruzenje_id';
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_korisnik_udruzenje
    BEFORE INSERT OR UPDATE ON Korisnik
                         FOR EACH ROW EXECUTE FUNCTION check_korisnik_udruzenje();

-- 4.2. Funkcija za provjeru na Zivotinja (udomitelj_id i volonter_id moraju biti odgovarajućeg tipa)
CREATE OR REPLACE FUNCTION check_zivotinja_korisnici()
RETURNS TRIGGER AS $$
DECLARE
udomitelj_tip tip_korisnika;
    volonter_tip tip_korisnika;
BEGIN
    -- Provjera udomitelja
    IF (NEW.udomitelj_id IS NOT NULL) THEN
SELECT tip_korisnika INTO udomitelj_tip FROM Korisnik WHERE id = NEW.udomitelj_id;
IF (udomitelj_tip IS NULL OR udomitelj_tip != 'Udomitelj') THEN
            RAISE EXCEPTION 'udomitelj_id mora referencirati korisnika tipa Udomitelj';
END IF;
END IF;

    -- Provjera volontera
    IF (NEW.volonter_id IS NOT NULL) THEN
SELECT tip_korisnika INTO volonter_tip FROM Korisnik WHERE id = NEW.volonter_id;
IF (volonter_tip IS NULL OR volonter_tip != 'Volonter') THEN
            RAISE EXCEPTION 'volonter_id mora referencirati korisnika tipa Volonter';
END IF;
END IF;

RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_zivotinja_korisnici
    BEFORE INSERT OR UPDATE ON Zivotinja
                         FOR EACH ROW EXECUTE FUNCTION check_zivotinja_korisnici();

-- 4.3. Funkcija za provjeru na ZahtevZaUdomljavanje (podnosilac mora biti Udomitelj)
CREATE OR REPLACE FUNCTION check_zahtev_podnosilac()
RETURNS TRIGGER AS $$
DECLARE
podnosilac_tip tip_korisnika;
BEGIN
SELECT tip_korisnika INTO podnosilac_tip FROM Korisnik WHERE id = NEW.podnosilac_id;
IF (podnosilac_tip IS NULL OR podnosilac_tip != 'Udomitelj') THEN
        RAISE EXCEPTION 'podnosilac_id mora referencirati korisnika tipa Udomitelj';
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_zahtev_podnosilac
    BEFORE INSERT OR UPDATE ON ZahtevZaUdomljavanje
                         FOR EACH ROW EXECUTE FUNCTION check_zahtev_podnosilac();

-- 4.4. Funkcija za provjeru na Izvestaj (admin mora biti AdminUdruzenja)
CREATE OR REPLACE FUNCTION check_izvestaj_admin()
RETURNS TRIGGER AS $$
DECLARE
admin_tip tip_korisnika;
BEGIN
SELECT tip_korisnika INTO admin_tip FROM Korisnik WHERE id = NEW.admin_id;
IF (admin_tip IS NULL OR admin_tip != 'AdminUdruzenja') THEN
        RAISE EXCEPTION 'admin_id mora referencirati korisnika tipa AdminUdruzenja';
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_izvestaj_admin
    BEFORE INSERT OR UPDATE ON Izvestaj
                         FOR EACH ROW EXECUTE FUNCTION check_izvestaj_admin();