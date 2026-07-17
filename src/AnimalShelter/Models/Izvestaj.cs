using AnimalShelter.Models.Enums;

namespace AnimalShelter.Models;

public class Izvestaj
{
    public int Id { get; set; }
    public TipIzvestaja Tip { get; set; }
    public PeriodIzvestaja Period { get; set; }
    public DateTime DatumGenerisanja { get; set; }
    public Statistika? Statistika { get; set; }
    public int AdminId { get; set; }

    // Navigacija
    public Korisnik? Admin { get; set; }
}