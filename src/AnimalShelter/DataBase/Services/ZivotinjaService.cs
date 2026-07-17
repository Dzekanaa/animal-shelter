using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using AnimalShelter.Models;
using AnimalShelter.DataBase.Dtos;
using AnimalShelter.Database;

namespace AnimalShelter.DataBase.Services;

public class ZivotinjaService
{
    public IEnumerable<Zivotinja> GetAllByUdruzenjeId(int udruzenjeId)
    {
        const string sql = @"
            SELECT *
            FROM Zivotinja
            WHERE udruzenje_id = @UdruzenjeId
            ORDER BY id
        ";

        using var connection = PostgresConnection.CreateConnection();
        return connection.Query<Zivotinja>(sql, new { UdruzenjeId = udruzenjeId });
    }

    public Zivotinja? GetById(int id)
    {
        const string sql = "SELECT * FROM Zivotinja WHERE id = @Id";
        using var connection = PostgresConnection.CreateConnection();
        return connection.QueryFirstOrDefault<Zivotinja>(sql, new { Id = id });
    }

    public int Create(ZivotinjaCreateDto dto)
    {
        const string sql = @"
        INSERT INTO Zivotinja 
            (naziv, kategorija, starost, pol, rasa, zdravstveno_stanje, 
             opis, status, slike, video, datum_unosa, udruzenje_id, 
             udomitelj_id, volonter_id)
        VALUES 
            (@Naziv, @Kategorija::kategorija_zivotinje, @Starost, 
             @Pol::pol_zivotinje, @Rasa, @ZdravstvenoStanje::zdravlje_zivotinje,
             @Opis, @Status::status_zivotinje, @Slike, @Video, CURRENT_TIMESTAMP, 
             @UdruzenjeId, @UdomiteljId, @VolonterId)
        RETURNING id
    ";

        var parameters = new
        {
            dto.Naziv,
            Kategorija = dto.Kategorija.ToString(),
            dto.Starost,
            Pol = dto.Pol.ToString(),
            dto.Rasa,
            ZdravstvenoStanje = dto.ZdravstvenoStanje.ToString(),
            dto.Opis,
            Status = dto.Status.ToString(),
            dto.Slike,
            dto.Video,
            dto.UdruzenjeId,
            dto.UdomiteljId,
            dto.VolonterId
        };

        using var connection = PostgresConnection.CreateConnection();
        return connection.ExecuteScalar<int>(sql, parameters);
    }

    public bool Update(ZivotinjaUpdateDto dto)
    {
        const string sql = @"
        UPDATE Zivotinja
        SET naziv = @Naziv,
            kategorija = @Kategorija::kategorija_zivotinje,
            starost = @Starost,
            pol = @Pol::pol_zivotinje,
            rasa = @Rasa,
            zdravstveno_stanje = @ZdravstvenoStanje::zdravlje_zivotinje,
            opis = @Opis,
            status = @Status::status_zivotinje,
            slike = @Slike,
            video = @Video,
            udomitelj_id = @UdomiteljId,
            volonter_id = @VolonterId
        WHERE id = @Id
    ";

        var parameters = new
        {
            dto.Id,
            dto.Naziv,
            Kategorija = dto.Kategorija.ToString(),
            dto.Starost,
            Pol = dto.Pol.ToString(),
            dto.Rasa,
            ZdravstvenoStanje = dto.ZdravstvenoStanje.ToString(),
            dto.Opis,
            Status = dto.Status.ToString(),
            dto.Slike,
            dto.Video,
            dto.UdomiteljId,
            dto.VolonterId
        };

        using var connection = PostgresConnection.CreateConnection();
        var rows = connection.Execute(sql, parameters);
        return rows > 0;
    }

    public bool Delete(int id)
    {
        const string sql = "DELETE FROM Zivotinja WHERE id = @Id";
        using var connection = PostgresConnection.CreateConnection();
        var rows = connection.Execute(sql, new { Id = id });
        return rows > 0;
    }
}