using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using AnimalShelter.Models;
using AnimalShelter.Models.Enums;
using AnimalShelter.DataBase.Services;
using AnimalShelter.DataBase.Dtos;
using AnimalShelter.Views;

namespace AnimalShelter.ViewModels;

public partial class PregledPrijavaViewModel : ObservableObject
{
    private readonly PrijavaVolontiranjaService _service;
    private readonly int _udruzenjeId;

    [ObservableProperty]
    private ObservableCollection<PrijavaZaVolontiranje> _prijave = new();

    [ObservableProperty]
    private PrijavaZaVolontiranje? _selectedPrijava;

    public ICommand PrihvatiCommand { get; }
    public ICommand OdbijCommand { get; }
    public ICommand OsveziCommand { get; }

    private Window? _ownerWindow;

    public PregledPrijavaViewModel(int udruzenjeId)
    {
        _udruzenjeId = udruzenjeId;
        _service = new PrijavaVolontiranjaService();

        PrihvatiCommand = new AsyncRelayCommand(PrihvatiAsync, () => SelectedPrijava != null && SelectedPrijava.StatusPrijave == StatusPrijave.CEKA);
        OdbijCommand = new AsyncRelayCommand(OdbijAsync, () => SelectedPrijava != null && SelectedPrijava.StatusPrijave == StatusPrijave.CEKA);
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);

        _ = OsveziAsync();
    }

    public void SetOwnerWindow(Window owner) => _ownerWindow = owner;

    private async Task OsveziAsync()
    {
        var lista = await Task.Run(() => _service.GetByUdruzenjeId(_udruzenjeId));
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Prijave.Clear();
            foreach (var item in lista)
                Prijave.Add(item);
        });
    }
    
    partial void OnSelectedPrijavaChanged(PrijavaZaVolontiranje? value)
    {
        (PrihvatiCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        (OdbijCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
    }

    private async Task PrihvatiAsync()
    {
        if (SelectedPrijava == null || _ownerWindow == null) return;

        // Otvori dialog za unos korisničkog imena i lozinke
        var dialogVm = new VolonterKreiranjeViewModel
        {
            UdruzenjeId = _udruzenjeId
        };
        var window = new VolonterKreiranjeWindow { DataContext = dialogVm };
        var result = await window.ShowDialog<bool?>(_ownerWindow);
        if (result == true)
        {
            // Kreiraj volontera i prihvati prijavu
            var volonterDto = new VolonterCreateDto
            {
                KorisnickoIme = dialogVm.KorisnickoIme,
                Lozinka = dialogVm.Lozinka,
                UdruzenjeId = _udruzenjeId
            };
            await Task.Run(() => _service.AcceptApplication(SelectedPrijava.Id, volonterDto));
            await OsveziAsync();
        }
    }

    private async Task OdbijAsync()
    {
        if (SelectedPrijava == null) return;

        // Potvrda odbijanja
        var msg = MessageBoxManager.GetMessageBoxStandard(
            "Potvrda",
            $"Da li ste sigurni da želite odbiti prijavu '{SelectedPrijava.Ime} {SelectedPrijava.Prezime}'?",
            ButtonEnum.YesNo,
            Icon.Warning);
        var result = await msg.ShowWindowDialogAsync(_ownerWindow);
        if (result == ButtonResult.Yes)
        {
            await Task.Run(() => _service.UpdateStatus(SelectedPrijava.Id, StatusPrijave.ODBIJEN));
            await OsveziAsync();
        }
    }
}