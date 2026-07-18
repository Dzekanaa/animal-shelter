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

CREATE TABLE IF NOT EXISTS Udruzenje (
                                         id SERIAL PRIMARY KEY,
                                         naziv VARCHAR(255) NOT NULL,
    opis TEXT,
    datum_osnivanja DATE NOT NULL,
    telefon VARCHAR(50),
    email VARCHAR(255),
    adresa TEXT
    );

CREATE TABLE IF NOT EXISTS Korisnik (
                                        id SERIAL PRIMARY KEY,
                                        tip_korisnika tip_korisnika NOT NULL,
                                        korisnicko_ime VARCHAR(100) UNIQUE,
    lozinka VARCHAR(255),
    ime VARCHAR(100) NOT NULL,
    prezime VARCHAR(100) NOT NULL,
    datum_registracije TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    telefon VARCHAR(50),
    email VARCHAR(255),
    adresa TEXT,
    udruzenje_id INTEGER REFERENCES Udruzenje(id) ON DELETE SET NULL
    );

-- Indeksi na Korisnik
CREATE INDEX IF NOT EXISTS idx_korisnik_tip ON Korisnik(tip_korisnika);
CREATE INDEX IF NOT EXISTS idx_korisnik_udruzenje ON Korisnik(udruzenje_id);

CREATE TABLE IF NOT EXISTS Zivotinja (
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

CREATE INDEX IF NOT EXISTS idx_zivotinja_udruzenje ON Zivotinja(udruzenje_id);
CREATE INDEX IF NOT EXISTS idx_zivotinja_udomitelj ON Zivotinja(udomitelj_id);
CREATE INDEX IF NOT EXISTS idx_zivotinja_volonter ON Zivotinja(volonter_id);
CREATE INDEX IF NOT EXISTS idx_zivotinja_status ON Zivotinja(status);

CREATE TABLE IF NOT EXISTS ZahtevZaUdomljavanje (
                                                    id SERIAL PRIMARY KEY,
                                                    datum_podnosenja TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                                    datum_udomljavanja TIMESTAMP,
                                                    podnosilac_id INTEGER REFERENCES Korisnik(id) ON DELETE SET NULL,
    status status_zahteva NOT NULL DEFAULT 'CEKA',
    napomena TEXT,
    nacin_obavestavanja nacin_obavestavanja,
    zivotinja_id INTEGER NOT NULL REFERENCES Zivotinja(id) ON DELETE CASCADE,
    ime VARCHAR(100),
    prezime VARCHAR(100),
    telefon VARCHAR(50),
    email VARCHAR(255),
    adresa TEXT
    );

CREATE INDEX IF NOT EXISTS idx_zahtev_zivotinja ON ZahtevZaUdomljavanje(zivotinja_id);
CREATE INDEX IF NOT EXISTS idx_zahtev_status ON ZahtevZaUdomljavanje(status);

CREATE TABLE IF NOT EXISTS Izvestaj (
                                        id SERIAL PRIMARY KEY,
                                        tip tip_izvestaja NOT NULL,
                                        period period_izvestaja NOT NULL,
                                        datum_generisanja TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                        statistika JSONB,
                                        admin_id INTEGER NOT NULL REFERENCES Korisnik(id) ON DELETE CASCADE
    );

CREATE INDEX IF NOT EXISTS idx_izvestaj_admin ON Izvestaj(admin_id);
CREATE INDEX IF NOT EXISTS idx_izvestaj_datum ON Izvestaj(datum_generisanja);

CREATE TABLE IF NOT EXISTS PrijavaZaVolontiranje (
                                                     id SERIAL PRIMARY KEY,
                                                     ime VARCHAR(100) NOT NULL,
    prezime VARCHAR(100) NOT NULL,
    opis TEXT,
    status_prijave status_prijave NOT NULL DEFAULT 'CEKA',
    telefon VARCHAR(50),
    email VARCHAR(255),
    adresa TEXT,
    udruzenje_id INTEGER REFERENCES Udruzenje(id) ON DELETE CASCADE,
    datum_podnosenja TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

CREATE INDEX IF NOT EXISTS idx_prijava_udruzenje ON PrijavaZaVolontiranje(udruzenje_id);

-- =============================================
-- 3. Triggeri za provjeru konzistentnosti
-- =============================================

DROP TRIGGER IF EXISTS trg_korisnik_udruzenje ON Korisnik;
DROP FUNCTION IF EXISTS check_korisnik_udruzenje();

CREATE OR REPLACE FUNCTION check_korisnik_udruzenje()
RETURNS TRIGGER AS $$
BEGIN
    IF (NEW.tip_korisnika = 'AdminUdruzenja' AND NEW.udruzenje_id IS NULL) THEN
        RAISE EXCEPTION 'AdminUdruzenja mora imati dodeljeno udruzenje_id';
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_korisnik_udruzenje
    BEFORE INSERT OR UPDATE ON Korisnik
                         FOR EACH ROW EXECUTE FUNCTION check_korisnik_udruzenje();

-- Zivotinja triggeri
DROP TRIGGER IF EXISTS trg_zivotinja_korisnici ON Zivotinja;
DROP FUNCTION IF EXISTS check_zivotinja_korisnici();

CREATE OR REPLACE FUNCTION check_zivotinja_korisnici()
RETURNS TRIGGER AS $$
DECLARE
udomitelj_tip tip_korisnika;
    volonter_tip tip_korisnika;
BEGIN
    IF (NEW.udomitelj_id IS NOT NULL) THEN
SELECT tip_korisnika INTO udomitelj_tip FROM Korisnik WHERE id = NEW.udomitelj_id;
IF (udomitelj_tip IS NULL OR udomitelj_tip != 'Udomitelj') THEN
            RAISE EXCEPTION 'udomitelj_id mora referencirati korisnika tipa Udomitelj';
END IF;
END IF;

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

-- ZahtevZaUdomljavanje trigger (dozvoljava NULL podnosilac_id za goste)
DROP TRIGGER IF EXISTS trg_zahtev_podnosilac ON ZahtevZaUdomljavanje;
DROP FUNCTION IF EXISTS check_zahtev_podnosilac();

CREATE OR REPLACE FUNCTION check_zahtev_podnosilac()
RETURNS TRIGGER AS $$
DECLARE
podnosilac_tip tip_korisnika;
BEGIN
    IF NEW.podnosilac_id IS NULL THEN
        RETURN NEW;
END IF;

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

-- Izvestaj trigger
DROP TRIGGER IF EXISTS trg_izvestaj_admin ON Izvestaj;
DROP FUNCTION IF EXISTS check_izvestaj_admin();

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