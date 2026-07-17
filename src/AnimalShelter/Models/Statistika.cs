namespace AnimalShelter.Models;

public class Statistika
{
    public int BrojUdomljenih { get; set; }
    public int BrojVracenih { get; set; }
    public decimal UkupnaDonacija { get; set; }
    public List<string> VrsteDonacija { get; set; } = new();
}