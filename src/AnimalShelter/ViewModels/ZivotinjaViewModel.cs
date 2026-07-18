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
using AnimalShelter.DataBase.Services;
using AnimalShelter.DataBase.Dtos;
using AnimalShelter.Models.Enums;
using AnimalShelter.Views;

namespace AnimalShelter.ViewModels;

public partial class ZivotinjeViewModel : ObservableObject
{
    private readonly ZivotinjaService _service;
    private readonly int _udruzenjeId;

    [ObservableProperty]
    private ObservableCollection<Zivotinja> _zivotinje = new();

    [ObservableProperty]
    private Zivotinja? _selectedZivotinja;
    
    public bool IsGuest => AppSession.IsGuest;
    public bool CanRequestAdoption => IsGuest && SelectedZivotinja != null;

    public ICommand DodajCommand { get; }
    public ICommand IzmeniCommand { get; }
    public ICommand ObrisiCommand { get; }
    public ICommand OsveziCommand { get; }
    public ICommand PrivremeniSmestajCommand { get; }  // NOVO
    public bool CanDoPrivremeniSmestaj => AppSession.IsVolonter && SelectedZivotinja != null;


    public ICommand ZahtevZaUdomljavanjeCommand { get; }
    public ICommand PrijavaZaVolontiranjeCommand { get; }
    
    
    
    private Window? _ownerWindow;
    

    public ZivotinjeViewModel(int udruzenjeId)
    {
        _udruzenjeId = udruzenjeId;
        _service = new ZivotinjaService();

        // Dozvole
        bool CanManageZivotinje() => AppSession.IsSistemskiAdmin || AppSession.IsAdminUdruzenja;
        bool CanDoPrivremeniSmestaj() => AppSession.IsVolonter && SelectedZivotinja != null;
        
        ZahtevZaUdomljavanjeCommand = new AsyncRelayCommand(ZahtevZaUdomljavanjeAsync, () => CanRequestAdoption);
        PrijavaZaVolontiranjeCommand = new AsyncRelayCommand(PrijaviSeZaVolontiranjeAsync);

        DodajCommand = new AsyncRelayCommand(DodajAsync, CanManageZivotinje);
        IzmeniCommand = new AsyncRelayCommand(IzmeniAsync, () => CanManageZivotinje() && SelectedZivotinja != null);
        ObrisiCommand = new AsyncRelayCommand(ObrisiAsync, () => CanManageZivotinje() && SelectedZivotinja != null);
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);
        PrivremeniSmestajCommand = new AsyncRelayCommand(PrivremeniSmestajAsync, CanDoPrivremeniSmestaj);

        _ = OsveziAsync();
    }
    
    private async Task PrijaviSeZaVolontiranjeAsync()
    {
        var editorVm = new PrijavaVolontiranjaEditorViewModel();
        var window = new PrijavaVolontiranjaEditorWindow { DataContext = editorVm };
        var result = await window.ShowDialog<bool?>(_ownerWindow);
        if (result == true)
        {
            var dto = new PrijavaVolontiranjaCreateDto
            {
                Ime = editorVm.Ime,
                Prezime = editorVm.Prezime,
                Opis = editorVm.Opis,
                Telefon = editorVm.Telefon,
                Email = editorVm.Email,
                Adresa = editorVm.Adresa,
                UdruzenjeId = _udruzenjeId
            };
            await Task.Run(() => new PrijavaVolontiranjaService().Create(dto));
            var msg = MessageBoxManager.GetMessageBoxStandard("Uspeh", "Prijava je poslata.", ButtonEnum.Ok, Icon.Success);
            await msg.ShowWindowDialogAsync(_ownerWindow);
        }
    }
    private async Task ZahtevZaUdomljavanjeAsync()
    {
        if (SelectedZivotinja == null) return;
        var msg = MessageBoxManager.GetMessageBoxStandard(
            "Zahtev za udomljavanje",
            $"Zahtev za udomljavanje životinje '{SelectedZivotinja.Naziv}' je poslat.",
            ButtonEnum.Ok,
            Icon.Info);
        await msg.ShowWindowDialogAsync(_ownerWindow);
    }
    
    
// ViewModels/ZivotinjeViewModel.cs

    private async Task PrivremeniSmestajAsync()
    {
        if (SelectedZivotinja == null || _ownerWindow == null)
            return;

        // Provjera da li je već zauzeta ili udomljena
        if (SelectedZivotinja.Status == StatusZivotinje.ZAUZETA || 
            SelectedZivotinja.Status == StatusZivotinje.UDOMLJENA)
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(
                "Greška",
                "Ova životinja je već zauzeta ili udomljena.",
                ButtonEnum.Ok,
                Icon.Error);
            await msg.ShowWindowDialogAsync(_ownerWindow);
            return;
        }

        if (AppSession.CurrentUser == null || !AppSession.IsVolonter)
            return;

        var success = await Task.Run(() => 
            _service.UpdateTemporaryShelter(SelectedZivotinja.Id, AppSession.CurrentUser.Id));

        if (success)
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(
                "Uspjeh",
                $"Životinja '{SelectedZivotinja.Naziv}' je smještena u privremeni smještaj.",
                ButtonEnum.Ok,
                Icon.Success);
            await msg.ShowWindowDialogAsync(_ownerWindow);
            await OsveziAsync();
        }
        else
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(
                "Greška",
                "Životinja nije mogla biti smještena. Možda je već zauzeta ili udomljena.",
                ButtonEnum.Ok,
                Icon.Error);
            await msg.ShowWindowDialogAsync(_ownerWindow);
        }
    }
    partial void OnSelectedZivotinjaChanged(Zivotinja? value)
    {
        OnPropertyChanged(nameof(CanRequestAdoption));
        (ZahtevZaUdomljavanjeCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanDoPrivremeniSmestaj));
        (IzmeniCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        (ObrisiCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        (PrivremeniSmestajCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
    }
    public void SetOwnerWindow(Window owner) => _ownerWindow = owner;

    private async Task OsveziAsync()
    {
        var lista = await Task.Run(() => _service.GetAllByUdruzenjeId(_udruzenjeId));
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Zivotinje.Clear();
            foreach (var item in lista)
                Zivotinje.Add(item);
        });
    }

    private async Task DodajAsync()
    {
        var editorVm = new ZivotinjeEditorViewModel(_udruzenjeId);
        var window = new ZivotinjeEditorWindow { DataContext = editorVm };

        var result = await window.ShowDialog<bool?>(_ownerWindow);
        if (result == true)
        {
            var dto = new ZivotinjaCreateDto
            {
                Naziv = editorVm.Naziv,
                Kategorija = editorVm.Kategorija,
                Starost = editorVm.Starost,
                Pol = editorVm.Pol,
                Rasa = editorVm.Rasa,
                ZdravstvenoStanje = editorVm.ZdravstvenoStanje,
                Opis = editorVm.Opis,
                Status = editorVm.Status,
                Slike = editorVm.Slike.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => s.Trim()).ToArray(),
                Video = editorVm.Video.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => s.Trim()).ToArray(),
                UdruzenjeId = _udruzenjeId,
                UdomiteljId = editorVm.UdomiteljId,
                VolonterId = editorVm.VolonterId
            };

            await Task.Run(() => _service.Create(dto));
            await OsveziAsync();
        }
    }

    private async Task IzmeniAsync()
    {
        if (SelectedZivotinja == null) return;

        var editorVm = new ZivotinjeEditorViewModel(_udruzenjeId)
        {
            Id = SelectedZivotinja.Id,
            Naziv = SelectedZivotinja.Naziv,
            Kategorija = SelectedZivotinja.Kategorija,
            Starost = SelectedZivotinja.Starost,
            Pol = SelectedZivotinja.Pol,
            Rasa = SelectedZivotinja.Rasa,
            ZdravstvenoStanje = SelectedZivotinja.ZdravstvenoStanje,
            Opis = SelectedZivotinja.Opis,
            Status = SelectedZivotinja.Status,
            Slike = string.Join(", ", SelectedZivotinja.Slike),
            Video = string.Join(", ", SelectedZivotinja.Video),
            UdomiteljId = SelectedZivotinja.UdomiteljId,
            VolonterId = SelectedZivotinja.VolonterId,
            IsEditMode = true
        };

        var window = new ZivotinjeEditorWindow
        {
            DataContext = editorVm,
            Title = "Izmeni životinju"
        };

        var result = await window.ShowDialog<bool?>(_ownerWindow);
        if (result == true)
        {
            var dto = new ZivotinjaUpdateDto
            {
                Id = editorVm.Id,
                Naziv = editorVm.Naziv,
                Kategorija = editorVm.Kategorija,
                Starost = editorVm.Starost,
                Pol = editorVm.Pol,
                Rasa = editorVm.Rasa,
                ZdravstvenoStanje = editorVm.ZdravstvenoStanje,
                Opis = editorVm.Opis,
                Status = editorVm.Status,
                Slike = editorVm.Slike.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => s.Trim()).ToArray(),
                Video = editorVm.Video.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => s.Trim()).ToArray(),
                UdomiteljId = editorVm.UdomiteljId,
                VolonterId = editorVm.VolonterId
            };

            await Task.Run(() => _service.Update(dto));
            await OsveziAsync();
        }
    }

    private async Task ObrisiAsync()
    {
        if (SelectedZivotinja == null || _ownerWindow == null) return;

        var result = await MessageBoxManager
            .GetMessageBoxStandard(
                "Potvrda brisanja",
                $"Da li ste sigurni da želite obrisati životinju '{SelectedZivotinja.Naziv}'?",
                ButtonEnum.YesNo,
                Icon.Warning)
            .ShowWindowDialogAsync(_ownerWindow);

        if (result == ButtonResult.Yes)
        {
            await Task.Run(() => _service.Delete(SelectedZivotinja.Id));
            await OsveziAsync();
        }
    }
}