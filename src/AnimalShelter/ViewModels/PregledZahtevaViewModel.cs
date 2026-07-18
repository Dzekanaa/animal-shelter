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

namespace AnimalShelter.ViewModels;

public partial class PregledZahtevaViewModel : ObservableObject
{
    private readonly ZahtevUdomljavanjeService _service;
    private readonly int _udruzenjeId;

    [ObservableProperty]
    private ObservableCollection<ZahtevZaUdomljavanje> _zahtevi = new();

    [ObservableProperty]
    private ZahtevZaUdomljavanje? _selectedZahtev;

    public ICommand PrihvatiCommand { get; }
    public ICommand OdbijCommand { get; }
    public ICommand OsveziCommand { get; }

    private Window? _ownerWindow;

    public PregledZahtevaViewModel(int udruzenjeId)
    {
        _udruzenjeId = udruzenjeId;
        _service = new ZahtevUdomljavanjeService();

        PrihvatiCommand = new AsyncRelayCommand(PrihvatiAsync, () => SelectedZahtev != null && SelectedZahtev.Status == StatusZahteva.CEKA);
        OdbijCommand = new AsyncRelayCommand(OdbijAsync, () => SelectedZahtev != null && SelectedZahtev.Status == StatusZahteva.CEKA);
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);

        _ = OsveziAsync();
    }

    public void SetOwnerWindow(Window owner) => _ownerWindow = owner;

    partial void OnSelectedZahtevChanged(ZahtevZaUdomljavanje? value)
    {
        (PrihvatiCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        (OdbijCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
    }

    private async Task OsveziAsync()
    {
        var lista = await Task.Run(() => _service.GetPendingByUdruzenjeId(_udruzenjeId));
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Zahtevi.Clear();
            foreach (var item in lista)
                Zahtevi.Add(item);
        });
    }

    private async Task PrihvatiAsync()
    {
        if (SelectedZahtev == null || _ownerWindow == null) return;

        var result = await MessageBoxManager
            .GetMessageBoxStandard("Potvrda", $"Da li želite prihvatiti zahtev za udomljavanje?", ButtonEnum.YesNo, Icon.Question)
            .ShowWindowDialogAsync(_ownerWindow);

        if (result == ButtonResult.Yes)
        {
            await Task.Run(() => _service.AcceptRequest(SelectedZahtev.Id));
            await OsveziAsync();
        }
    }

    private async Task OdbijAsync()
    {
        if (SelectedZahtev == null || _ownerWindow == null) return;

        var result = await MessageBoxManager
            .GetMessageBoxStandard("Potvrda", $"Da li želite odbiti zahtev za udomljavanje?", ButtonEnum.YesNo, Icon.Warning)
            .ShowWindowDialogAsync(_ownerWindow);

        if (result == ButtonResult.Yes)
        {
            await Task.Run(() => _service.UpdateStatus(SelectedZahtev.Id, StatusZahteva.ODBIJEN));
            await OsveziAsync();
        }
    }
}