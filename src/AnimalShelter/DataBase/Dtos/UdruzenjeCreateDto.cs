namespace AnimalShelter.DataBase.Dtos;

public class UdruzenjeCreateDto
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public DateTime DatumOsnivanja { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }

    public AdminDto Admin { get; set; } = new();
}