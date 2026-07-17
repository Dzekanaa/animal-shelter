using AnimalShelter.Models;
using AnimalShelter.Models.Enums;

namespace AnimalShelter;

public static class AppSession
{
    public static Korisnik? CurrentUser { get; set; }

    public static bool IsGuest => CurrentUser == null;

    public static bool IsSistemskiAdmin => CurrentUser?.TipKorisnika == TipKorisnika.SistemskiAdmin;

    public static bool IsAdminUdruzenja => CurrentUser?.TipKorisnika == TipKorisnika.AdminUdruzenja;

    public static bool IsVolonter => CurrentUser?.TipKorisnika == TipKorisnika.Volonter;

    // Udomitelj nije predviđen za logovanje
}