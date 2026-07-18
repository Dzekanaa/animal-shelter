using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnimalShelter.ViewModels;

public partial class PrijavaVolontiranjaEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string _ime = string.Empty;

    [ObservableProperty]
    private string _prezime = string.Empty;

    [ObservableProperty]
    private string? _opis;

    [ObservableProperty]
    private string? _telefon;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _adresa;

    public IAsyncRelayCommand? PosaljiCommand { get; }
    public IRelayCommand? OdustaniCommand { get; }

    public event EventHandler? RequestClose;

    public PrijavaVolontiranjaEditorViewModel()
    {
        PosaljiCommand = new AsyncRelayCommand(PosaljiAsync, CanPosalji);
        OdustaniCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    private bool CanPosalji() => !string.IsNullOrWhiteSpace(Ime) && !string.IsNullOrWhiteSpace(Prezime);

    private async Task PosaljiAsync()
    {
        if (!CanPosalji()) return;
        RequestClose?.Invoke(this, new DialogResultEventArgs(true));
        await Task.CompletedTask;
    }

    public class DialogResultEventArgs : EventArgs
    {
        public bool Result { get; }
        public DialogResultEventArgs(bool result) => Result = result;
    }
}