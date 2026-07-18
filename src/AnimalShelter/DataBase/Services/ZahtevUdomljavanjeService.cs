using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using AnimalShelter.Models;
using AnimalShelter.Models.Enums;
using AnimalShelter.DataBase.Dtos;
using AnimalShelter.Database;

namespace AnimalShelter.DataBase.Services;

public class ZahtevUdomljavanjeService
{
    private readonly KorisnikService _korisnikService = new();

    public int Create(AdoptionRequestCreateDto dto)
    {
        const string sql = @"
        INSERT INTO zahtevzaudomljavanje 
            (zivotinja_id, datum_podnosenja, status, 
             ime, prezime, telefon, email, adresa, nacin_obavestavanja, podnosilac_id)
        VALUES 
            (@ZivotinjaId, CURRENT_TIMESTAMP, 'CEKA'::status_zahteva,
             @Ime, @Prezime, @Telefon, @Email, @Adresa, @NacinObavestavanja::nacin_obavestavanja, NULL)
        RETURNING id
    ";

        // Parametri – enum pretvaramo u string, a null ostaje null
        var parameters = new
        {
            dto.ZivotinjaId,
            dto.Ime,
            dto.Prezime,
            dto.Telefon,
            dto.Email,
            dto.Adresa,
            NacinObavestavanja = dto.NacinObavestavanja?.ToString()   // <-- ključno
        };

        using var connection = PostgresConnection.CreateConnection();
        return connection.ExecuteScalar<int>(sql, parameters);
    }

    public IEnumerable<ZahtevZaUdomljavanje> GetPendingByUdruzenjeId(int udruzenjeId)
    {
        const string sql = @"
            SELECT z.* 
            FROM zahtevzaudomljavanje z
            JOIN zivotinja a ON a.id = z.zivotinja_id
            WHERE a.udruzenje_id = @UdruzenjeId
              AND z.status = 'CEKA'::status_zahteva
            ORDER BY z.datum_podnosenja DESC
        ";

        using var connection = PostgresConnection.CreateConnection();
        return connection.Query<ZahtevZaUdomljavanje>(sql, new { UdruzenjeId = udruzenjeId });
    }

    public bool UpdateStatus(int id, StatusZahteva status)
    {
        const string sql = @"
            UPDATE zahtevzaudomljavanje
            SET status = @Status::status_zahteva
            WHERE id = @Id
        ";

        using var connection = PostgresConnection.CreateConnection();
        var rows = connection.Execute(sql, new { Id = id, Status = status.ToString() });
        return rows > 0;
    }

  public bool AcceptRequest(int requestId)
{
    using var connection = PostgresConnection.CreateConnection();
    using var transaction = connection.BeginTransaction();

    try
    {
        // 1. Preuzmi podatke o zahtevu i životinji
        const string sqlGet = @"
            SELECT z.*, a.udruzenje_id, a.status AS animal_status
            FROM zahtevzaudomljavanje z
            JOIN zivotinja a ON a.id = z.zivotinja_id
            WHERE z.id = @Id
            FOR UPDATE OF a
        ";
        var requestData = connection.QueryFirstOrDefault(sqlGet, new { Id = requestId }, transaction);
        if (requestData == null) return false;

        // 2. Provjeri da li je životinja još uvijek DOSTUPNA
        if (requestData.animal_status != "DOSTUPNA")
        {
            // Vraćamo false bez izuzetka – UI će prikazati poruku
            return false;
        }

        // 3. Kreiraj novog Udomitelja
        const string sqlInsertUdomitelj = @"
            INSERT INTO korisnik 
                (tip_korisnika, ime, prezime, telefon, email, adresa, datum_registracije,
                 korisnicko_ime, lozinka)
            VALUES 
                ('Udomitelj'::tip_korisnika, @Ime, @Prezime, @Telefon, @Email, @Adresa, CURRENT_TIMESTAMP,
                 NULL, NULL)
            RETURNING id
        ";

        var noviUdomiteljId = connection.ExecuteScalar<int>(sqlInsertUdomitelj, new
        {
            Ime = requestData.ime,
            Prezime = requestData.prezime,
            Telefon = requestData.telefon,
            Email = requestData.email,
            Adresa = requestData.adresa
        }, transaction);

        // 4. Ažuriraj zahtev
        const string sqlUpdateRequest = @"
            UPDATE zahtevzaudomljavanje
            SET status = 'PRIHVACEN'::status_zahteva,
                datum_udomljavanja = CURRENT_TIMESTAMP
            WHERE id = @Id
        ";
        connection.Execute(sqlUpdateRequest, new { Id = requestId }, transaction);

        // 5. Ažuriraj životinju – samo ako je još DOSTUPNA
        const string sqlUpdateAnimal = @"
            UPDATE zivotinja
            SET status = 'UDOMLJENA'::status_zivotinje,
                udomitelj_id = @UdomiteljId
            WHERE id = @ZivotinjaId
              AND status = 'DOSTUPNA'
        ";
        var rowsAffected = connection.Execute(sqlUpdateAnimal, new
        {
            UdomiteljId = noviUdomiteljId,
            ZivotinjaId = requestData.zivotinja_id
        }, transaction);

        if (rowsAffected == 0)
        {
            // Neko drugi je u međuvremenu udomio životinju – vraćamo false
            return false;
        }

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        throw; // Ostale greške (baza, konekcija) i dalje bacamo – to su sistemske greške
    }
}
}