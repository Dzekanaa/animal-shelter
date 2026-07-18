namespace AnimalShelter.DataBase.Dtos;

public class PrijavaVolontiranjaCreateDto
{
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }
    public int UdruzenjeId { get; set; }
}