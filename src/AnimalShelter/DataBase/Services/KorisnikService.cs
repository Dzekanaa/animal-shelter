using System.Data;
using Dapper;
using AnimalShelter.Database;
using AnimalShelter.Models;

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
        return connection.QueryFirstOrDefault<Korisnik>(sql, new { Username = username, Password = password });
    }
}