using AnimalShelter.Models.Enums;

namespace AnimalShelter.DataBase.Dtos;

public class AdoptionRequestCreateDto
{
    public int ZivotinjaId { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }
    public NacinObavestavanja? NacinObavestavanja { get; set; }
}