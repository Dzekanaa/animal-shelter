using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using AnimalShelter.Models;
using AnimalShelter.Models.Enums;
using AnimalShelter.DataBase.Dtos;
using AnimalShelter.Database;

namespace AnimalShelter.DataBase.Services;

public class PrijavaVolontiranjaService
{
    public int Create(PrijavaVolontiranjaCreateDto dto)
    {
        const string sql = @"
            INSERT INTO prijavazavolontiranje 
                (ime, prezime, opis, telefon, email, adresa, udruzenje_id, datum_podnosenja, status_prijave)
            VALUES 
                (@Ime, @Prezime, @Opis, @Telefon, @Email, @Adresa, @UdruzenjeId, CURRENT_TIMESTAMP, 'CEKA'::status_prijave)
            RETURNING id
        ";

        using var connection = PostgresConnection.CreateConnection();
        return connection.ExecuteScalar<int>(sql, dto);
    }

    public IEnumerable<PrijavaZaVolontiranje> GetByUdruzenjeId(int udruzenjeId)
    {
        const string sql = @"
        SELECT * FROM prijavazavolontiranje 
        WHERE udruzenje_id = @UdruzenjeId 
          AND status_prijave = 'CEKA'  
        ORDER BY datum_podnosenja DESC
    ";

        using var connection = PostgresConnection.CreateConnection();
        return connection.Query<PrijavaZaVolontiranje>(sql, new { UdruzenjeId = udruzenjeId });
    }

    public bool UpdateStatus(int id, StatusPrijave status)
    {
        // Eksplicitno kastovanje enum vrijednosti
        const string sql = @"
            UPDATE prijavazavolontiranje 
            SET status_prijave = @Status::status_prijave 
            WHERE id = @Id
        ";

        using var connection = PostgresConnection.CreateConnection();
        var rows = connection.Execute(sql, new { Id = id, Status = status.ToString() }); // šaljemo string, kastujemo u SQL-u
        return rows > 0;
    }

    public bool AcceptApplication(int prijavaId, VolonterCreateDto volonterDto)
{
    using var connection = PostgresConnection.CreateConnection();
    using var transaction = connection.BeginTransaction();

    try
    {
        // 1. Ažuriraj status prijave na PRIHVACEN
        const string sqlUpdate = @"
            UPDATE prijavazavolontiranje 
            SET status_prijave = 'PRIHVACEN'::status_prijave 
            WHERE id = @Id
        ";
        connection.Execute(sqlUpdate, new { Id = prijavaId }, transaction);

        // 2. Preuzmi podatke iz prijave (ime, prezime, kontakt, udruzenje_id)
        const string sqlGetPrijava = @"
            SELECT ime, prezime, telefon, email, adresa, udruzenje_id 
            FROM prijavazavolontiranje 
            WHERE id = @Id
        ";
        var prijava = connection.QueryFirstOrDefault(sqlGetPrijava, new { Id = prijavaId }, transaction);
        if (prijava == null) return false;

        // 3. Kreiraj novog korisnika (Volonter) sa udruzenje_id
        const string sqlInsertKorisnik = @"
            INSERT INTO korisnik 
                (tip_korisnika, korisnicko_ime, lozinka, ime, prezime, 
                 datum_registracije, telefon, email, adresa, udruzenje_id)
            VALUES 
                ('Volonter'::tip_korisnika, @KorisnickoIme, @Lozinka, @Ime, @Prezime,
                 CURRENT_TIMESTAMP, @Telefon, @Email, @Adresa, @UdruzenjeId)
        ";

        connection.Execute(sqlInsertKorisnik, new
        {
            volonterDto.KorisnickoIme,
            volonterDto.Lozinka,
            Ime = prijava.ime,
            Prezime = prijava.prezime,
            Telefon = prijava.telefon,
            Email = prijava.email,
            Adresa = prijava.adresa,
            UdruzenjeId = prijava.udruzenje_id   // <-- koristi udruzenje_id iz prijave
        }, transaction);

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
    }
}