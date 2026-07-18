using System;
using AnimalShelter.Models.Enums;

namespace AnimalShelter.Models;

public class ZahtevZaUdomljavanje
{
    public int Id { get; set; }
    public DateTime DatumPodnosenja { get; set; }
    public DateTime? DatumUdomljavanja { get; set; }
    public int? PodnosilacId { get; set; }  // nullable
    public StatusZahteva Status { get; set; }
    public string? Napomena { get; set; }
    public NacinObavestavanja? NacinObavestavanja { get; set; }
    public int ZivotinjaId { get; set; }
    public string? Ime { get; set; }
    public string? Prezime { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adresa { get; set; }

    public Korisnik? Podnosilac { get; set; }
    public Zivotinja? Zivotinja { get; set; }
}