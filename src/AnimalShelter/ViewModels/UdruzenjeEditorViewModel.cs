using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnimalShelter.ViewModels;

public partial class UdruzenjeEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _naziv = string.Empty;

    [ObservableProperty]
    private string _opis = string.Empty;
    
    [ObservableProperty]
    private bool _isEditMode;

    // PROMJENA: DateTime? -> DateTimeOffset?
    [ObservableProperty]
    private DateTimeOffset? _datumOsnivanja = DateTimeOffset.Now;

    [ObservableProperty]
    private string? _telefon;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _adresa;

    // Admin podaci
    [ObservableProperty]
    private string _korisnickoIme = string.Empty;

    [ObservableProperty]
    private string _lozinka = string.Empty;

    [ObservableProperty]
    private string _ime = string.Empty;

    [ObservableProperty]
    private string _prezime = string.Empty;

    [ObservableProperty]
    private string _adminEmail = string.Empty;

    [ObservableProperty]
    private string _adminTelefon = string.Empty;

    [ObservableProperty]
    private string _adminAdresa = string.Empty;

    public IAsyncRelayCommand? SacuvajCommand { get; }
    public IRelayCommand? OdustaniCommand { get; }

    public event EventHandler? RequestClose;

    public UdruzenjeEditorViewModel()
    {
        IsEditMode = false;
        SacuvajCommand = new AsyncRelayCommand(SacuvajAsync, CanSacuvaj);
        OdustaniCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    private bool CanSacuvaj()
    {
        if (IsEditMode)
        {
            // U edit modu provjeravamo samo podatke o udruženju
            return !string.IsNullOrWhiteSpace(Naziv);
        }
        else
        {
            // U create modu provjeravamo i admin podatke
            return !string.IsNullOrWhiteSpace(Naziv)
                   && !string.IsNullOrWhiteSpace(KorisnickoIme)
                   && !string.IsNullOrWhiteSpace(Lozinka)
                   && !string.IsNullOrWhiteSpace(Ime)
                   && !string.IsNullOrWhiteSpace(Prezime)
                   && !string.IsNullOrWhiteSpace(AdminAdresa);
        }
    }

    private async Task SacuvajAsync()
    {
        if (!CanSacuvaj()) return;
        RequestClose?.Invoke(this, new DialogResultEventArgs(true));
        await Task.CompletedTask; // samo da zadovolji async
    }

    public class DialogResultEventArgs : EventArgs
    {
        public bool Result { get; }
        public DialogResultEventArgs(bool result) => Result = result;
    }
}