using System;
using AnimalShelter.Models.Enums;

namespace AnimalShelter.Models;

public class ZahtevZaUdomljavanje
{
    public int Id { get; set; }
    public DateTime DatumPodnosenja { get; set; }
    public DateTime? DatumUdomljavanja { get; set; }
    public int PodnosilacId { get; set; }
    public StatusZahteva Status { get; set; }
    public string? Napomena { get; set; }
    public NacinObavestavanja? NacinObavestavanja { get; set; }
    public int ZivotinjaId { get; set; }

    public Korisnik? Podnosilac { get; set; }
    public Zivotinja? Zivotinja { get; set; }
}