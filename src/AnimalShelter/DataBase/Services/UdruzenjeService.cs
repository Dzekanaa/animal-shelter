using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using AnimalShelter.Models;
using AnimalShelter.DataBase.Dtos;
using AnimalShelter.Database;
using AnimalShelter.Models.Enums;

namespace AnimalShelter.DataBase.Services;

public class UdruzenjeService
{
    // Uklanjamo _connection polje – kreiraćemo novu konekciju svaki put

    
    public IEnumerable<Udruzenje> GetAllForUser(Korisnik? user)
    {
        if (user == null || user.TipKorisnika == TipKorisnika.SistemskiAdmin)
        {
            // Gost ili SistemskiAdmin -> sva udruženja
            return GetAll();
        }
        else if (user.TipKorisnika == TipKorisnika.AdminUdruzenja || user.TipKorisnika == TipKorisnika.Volonter)
        {
            if (user.UdruzenjeId.HasValue)
            {
                var udruzenje = GetById(user.UdruzenjeId.Value);
                return udruzenje != null ? new List<Udruzenje> { udruzenje } : Enumerable.Empty<Udruzenje>();
            }
            else
            {
                // Ako nema dodeljeno udruženje, ne prikazuj ništa
                return Enumerable.Empty<Udruzenje>();
            }
        }
        else
        {
            // Ostali tipovi (npr. Udomitelj) nemaju pravo pregleda udruženja
            return Enumerable.Empty<Udruzenje>();
        }
    }
    public IEnumerable<Udruzenje> GetAll()
    {
        const string sql = @"
            SELECT 
                u.*,
                k.id as AdminId,
                k.korisnicko_ime as AdminKorisnickoIme,
                k.ime as AdminIme,
                k.prezime as AdminPrezime,
                k.email as AdminEmail,
                k.telefon as AdminTelefon,
                k.adresa as AdminAdresa
            FROM Udruzenje u
            LEFT JOIN Korisnik k ON k.udruzenje_id = u.id AND k.tip_korisnika = 'AdminUdruzenja'
            ORDER BY u.id
        ";

        using var connection = PostgresConnection.CreateConnection();
        return connection.Query<Udruzenje, Korisnik, Udruzenje>(
            sql,
            (udruzenje, admin) =>
            {
                udruzenje.Admin = admin;
                return udruzenje;
            },
            splitOn: "AdminId"
        );
    }

    public Udruzenje? GetById(int id)
    {
        const string sql = @"
            SELECT 
                u.*,
                k.id as AdminId,
                k.korisnicko_ime as AdminKorisnickoIme,
                k.ime as AdminIme,
                k.prezime as AdminPrezime,
                k.email as AdminEmail,
                k.telefon as AdminTelefon,
                k.adresa as AdminAdresa
            FROM Udruzenje u
            LEFT JOIN Korisnik k ON k.udruzenje_id = u.id AND k.tip_korisnika = 'AdminUdruzenja'
            WHERE u.id = @Id
        ";

        using var connection = PostgresConnection.CreateConnection();
        return connection.Query<Udruzenje, Korisnik, Udruzenje>(
            sql,
            (udruzenje, admin) =>
            {
                udruzenje.Admin = admin;
                return udruzenje;
            },
            new { Id = id },
            splitOn: "AdminId"
        ).FirstOrDefault();
    }

    public int Create(UdruzenjeCreateDto dto)
    {
        using var connection = PostgresConnection.CreateConnection();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlUdruzenje = @"
                INSERT INTO Udruzenje (naziv, opis, datum_osnivanja, telefon, email, adresa)
                VALUES (@Naziv, @Opis, @DatumOsnivanja, @Telefon, @Email, @Adresa)
                RETURNING id
            ";

            var udruzenjeId = connection.ExecuteScalar<int>(sqlUdruzenje, new
            {
                dto.Naziv,
                dto.Opis,
                dto.DatumOsnivanja,
                dto.Telefon,
                dto.Email,
                dto.Adresa
            }, transaction);

            const string sqlAdmin = @"
                INSERT INTO Korisnik 
                    (tip_korisnika, korisnicko_ime, lozinka, ime, prezime, 
                     datum_registracije, telefon, email, adresa, udruzenje_id)
                VALUES 
                    ('AdminUdruzenja', @KorisnickoIme, @Lozinka, @Ime, @Prezime,
                     CURRENT_TIMESTAMP, @Telefon, @Email, @Adresa, @UdruzenjeId)
            ";

            connection.Execute(sqlAdmin, new
            {
                dto.Admin.KorisnickoIme,
                dto.Admin.Lozinka,
                dto.Admin.Ime,
                dto.Admin.Prezime,
                dto.Admin.Telefon,
                dto.Admin.Email,
                dto.Admin.Adresa,
                UdruzenjeId = udruzenjeId
            }, transaction);

            transaction.Commit();
            return udruzenjeId;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            transaction.Rollback();
            throw new InvalidOperationException(
                "Korisničko ime već postoji. Molimo izaberite drugo.", ex);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        // Konekcija se automatski zatvara na kraju using bloka
    }
    

    public bool Update(UdruzenjeUpdateDto dto)
    {
        const string sql = @"
            UPDATE Udruzenje
            SET naziv = @Naziv,
                opis = @Opis,
                datum_osnivanja = @DatumOsnivanja,
                telefon = @Telefon,
                email = @Email,
                adresa = @Adresa
            WHERE id = @Id
        ";

        using var connection = PostgresConnection.CreateConnection();
        var affectedRows = connection.Execute(sql, new
        {
            dto.Id,
            dto.Naziv,
            dto.Opis,
            dto.DatumOsnivanja,
            dto.Telefon,
            dto.Email,
            dto.Adresa
        });

        return affectedRows > 0;
    }

    public bool Delete(int id)
    {
        using var connection = PostgresConnection.CreateConnection();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlDeleteAdmin = @"
                DELETE FROM Korisnik
                WHERE udruzenje_id = @Id AND tip_korisnika = 'AdminUdruzenja'
            ";
            connection.Execute(sqlDeleteAdmin, new { Id = id }, transaction);

            const string sqlDeleteUdruzenje = @"
                DELETE FROM Udruzenje WHERE id = @Id
            ";
            var rows = connection.Execute(sqlDeleteUdruzenje, new { Id = id }, transaction);

            transaction.Commit();
            return rows > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}