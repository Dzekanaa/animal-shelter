using AnimalShelter.Models.Enums;

namespace AnimalShelter.DataBase.Dtos;

public class PrijavaVolontiranjaUpdateDto
{
    public int Id { get; set; }
    public StatusPrijave StatusPrijave { get; set; }
}