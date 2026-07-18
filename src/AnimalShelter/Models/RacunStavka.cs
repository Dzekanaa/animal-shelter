using System;

namespace AnimalShelter.Models;

public class RacunStavka
{
    public int Id { get; set; }
    public DateTime Datum { get; set; }
    public decimal Iznos { get; set; }
    public string Banka { get; set; } = string.Empty;
}