namespace AnimalShelter.DataBase.Dtos;

public class UdruzenjeUpdateDto
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public DateTime DatumOsnivanja { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }
}