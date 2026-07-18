using System.Data;
using Dapper;
using AnimalShelter.Database;
using AnimalShelter.Models;
using AnimalShelter.Models.Enums;

namespace AnimalShelter.DataBase.Services;

public class KorisnikService
{
    public Korisnik? Authenticate(string username, string password)
    {
        const string sql = @"
            SELECT id, tip_korisnika, korisnicko_ime, ime, prezime, udruzenje_id
            FROM Korisnik
            WHERE korisnicko_ime = @Username AND lozinka = @Password
        ";

        using var connection = PostgresConnection.CreateConnection();
        // Dohvati rezultat kao dinamički objekat (ili koristi QueryFirstOrDefault sa dynamic)
        var result = connection.QueryFirstOrDefault(sql, new { Username = username, Password = password });

        if (result == null)
            return null;

        // Ručno mapiraj
        var user = new Korisnik
        {
            Id = result.id,
            KorisnickoIme = result.korisnicko_ime,
            Ime = result.ime,
            Prezime = result.prezime,
            UdruzenjeId = result.udruzenje_id,
            // Ručno pretvori string u enum
            TipKorisnika = Enum.Parse<TipKorisnika>((string)result.tip_korisnika)
        };

        return user;
    }
    
    public IEnumerable<Korisnik> GetAllUdomitelji()
    {
        const string sql = @"
        SELECT id, tip_korisnika, korisnicko_ime, ime, prezime, 
               datum_registracije, telefon, email, adresa, udruzenje_id
        FROM korisnik
        WHERE tip_korisnika = 'Udomitelj'::tip_korisnika
        ORDER BY prezime, ime
    ";
        using var connection = PostgresConnection.CreateConnection();
        return connection.Query<Korisnik>(sql);
    }
}