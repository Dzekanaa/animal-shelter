using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnimalShelter.ViewModels;

public partial class VolonterKreiranjeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _korisnickoIme = string.Empty;

    [ObservableProperty]
    private string _lozinka = string.Empty;

    [ObservableProperty]
    private string _lozinkaPotvrda = string.Empty;

    public int UdruzenjeId { get; set; }

    public IAsyncRelayCommand? KreirajCommand { get; }
    public IRelayCommand? OdustaniCommand { get; }

    public event EventHandler? RequestClose;

    public VolonterKreiranjeViewModel()
    {
        KreirajCommand = new AsyncRelayCommand(KreirajAsync, CanKreiraj);
        OdustaniCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    private bool CanKreiraj() => !string.IsNullOrWhiteSpace(KorisnickoIme) && 
                                 !string.IsNullOrWhiteSpace(Lozinka) && 
                                 Lozinka == LozinkaPotvrda;

    private async Task KreirajAsync()
    {
        if (!CanKreiraj()) return;
        RequestClose?.Invoke(this, new DialogResultEventArgs(true));
        await Task.CompletedTask;
    }

    public class DialogResultEventArgs : EventArgs
    {
        public bool Result { get; }
        public DialogResultEventArgs(bool result) => Result = result;
    }
}