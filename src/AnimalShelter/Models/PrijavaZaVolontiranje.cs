using AnimalShelter.Models.Enums;

namespace AnimalShelter.Models;

public class PrijavaZaVolontiranje
{
    public int Id { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public StatusPrijave StatusPrijave { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }
}