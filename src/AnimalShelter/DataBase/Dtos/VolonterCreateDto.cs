namespace AnimalShelter.DataBase.Dtos;

public class VolonterCreateDto
{
    public string KorisnickoIme { get; set; } = string.Empty;
    public string Lozinka { get; set; } = string.Empty;
    public int UdruzenjeId { get; set; }
}