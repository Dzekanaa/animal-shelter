using AnimalShelter.Models.Enums;

namespace AnimalShelter.Models;

public class Korisnik
{
    public int Id { get; set; }
    public TipKorisnika TipKorisnika { get; set; }
    public string KorisnickoIme { get; set; } = string.Empty;
    public string Lozinka { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public DateTime DatumRegistracije { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }
    public int? UdruzenjeId { get; set; }   // samo za AdminUdruzenja

    // (opciono, za Dapper se ne mapira automatski)
    public Udruzenje? Udruzenje { get; set; }
}