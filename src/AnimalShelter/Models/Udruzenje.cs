using System;

namespace AnimalShelter.Models;

public class Udruzenje
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public DateTime DatumOsnivanja { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }
    
    
    public Korisnik? Admin { get; set; }
}