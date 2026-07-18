namespace AnimalShelter.DataBase.Dtos;

public class KorisnikDto
{
    public int Id { get; set; }
    public string TipKorisnika { get; set; } = string.Empty;  // string umjesto enum-a
    public string? KorisnickoIme { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public DateTime DatumRegistracije { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }
    public int? UdruzenjeId { get; set; }
}