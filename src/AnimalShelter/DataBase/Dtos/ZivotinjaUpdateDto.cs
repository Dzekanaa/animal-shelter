using AnimalShelter.Models.Enums;

namespace AnimalShelter.DataBase.Dtos;

public class ZivotinjaUpdateDto
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public KategorijaZivotinje Kategorija { get; set; }
    public int? Starost { get; set; }
    public PolZivotinje Pol { get; set; }
    public string? Rasa { get; set; }
    public ZdravljeZivotinje ZdravstvenoStanje { get; set; }
    public string? Opis { get; set; }
    public StatusZivotinje Status { get; set; }
    public string[] Slike { get; set; } = Array.Empty<string>();
    public string[] Video { get; set; } = Array.Empty<string>();
    public int? UdomiteljId { get; set; }
    public int? VolonterId { get; set; }
}