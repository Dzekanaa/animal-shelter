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
using AnimalShelter.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AnimalShelter.ViewModels;

public partial class UdruzenjaViewModel : ObservableObject
{
    private readonly UdruzenjeService _service;
    

    [ObservableProperty]
    private ObservableCollection<Udruzenje> _udruzenja = new();

    [ObservableProperty]
    private Udruzenje? _selectedUdruzenje;

    public ICommand DodajCommand { get; }
    public ICommand IzmeniCommand { get; }
    public ICommand ObrisiCommand { get; }
    public ICommand OsveziCommand { get; }
    public ICommand PrikaziZivotinjeCommand { get; }
    public ICommand PregledPrijavaCommand { get; }
    
    public bool CanManageUdruzenja => AppSession.IsSistemskiAdmin;
    public bool isUdruzenjeAdmin => AppSession.IsAdminUdruzenja;

    private Window? _ownerWindow;

    public UdruzenjaViewModel()
    {
        _service = new UdruzenjeService();
        
        
        bool CanManageUdruzenja() => AppSession.IsSistemskiAdmin;
        
        PregledPrijavaCommand = new AsyncRelayCommand(PregledPrijavaAsync, () => AppSession.IsAdminUdruzenja);
        DodajCommand = new AsyncRelayCommand(DodajAsync, CanManageUdruzenja);
        IzmeniCommand = new AsyncRelayCommand(IzmeniAsync, () => CanManageUdruzenja() && SelectedUdruzenje != null);
        ObrisiCommand = new AsyncRelayCommand(ObrisiAsync, () => CanManageUdruzenja() && SelectedUdruzenje != null);
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);
        PrikaziZivotinjeCommand = new AsyncRelayCommand<Udruzenje>(PrikaziZivotinjeAsync); // uvijek dostupno

        _ = OsveziAsync();
    }
    
    private async Task PregledPrijavaAsync()
    {
        if (AppSession.CurrentUser?.UdruzenjeId == null) return;
        var vm = new PregledPrijavaViewModel(AppSession.CurrentUser.UdruzenjeId.Value);
        var window = new PregledPrijavaWindow { DataContext = vm };
        vm.SetOwnerWindow(_ownerWindow); 
        await window.ShowDialog(_ownerWindow);
    }

    private async Task PrikaziZivotinjeAsync(Udruzenje? udruzenje)
    {
        if (udruzenje == null || _ownerWindow == null) return;

        var vm = new ZivotinjeViewModel(udruzenje.Id);
        var window = new ZivotinjeWindow();
        window.SetViewModel(vm);
        await window.ShowDialog(_ownerWindow);
    }
    private async Task InitAsync()
    {
        try
        {
            await OsveziAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Greška pri učitavanju udruženja: {ex}");
        }
    }

    partial void OnSelectedUdruzenjeChanged(Udruzenje? value)
    {
        (IzmeniCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        (ObrisiCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
    }
    public void SetOwnerWindow(Window owner) => _ownerWindow = owner;

    private async Task OsveziAsync()
    {
        try
        {
            var lista = await Task.Run(() => _service.GetAll());
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Udruzenja.Clear();
                foreach (var item in lista)
                    Udruzenja.Add(item);
            });
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Greška",
                ex.Message,
                ButtonEnum.Ok,
                Icon.Error);
            await box.ShowWindowDialogAsync(_ownerWindow);
        }

    }

    private async Task DodajAsync()
    {
        var editorVm = new UdruzenjeEditorViewModel();
        var window = new UdruzenjeEditorWindow { DataContext = editorVm };

        var result = await window.ShowDialog<bool?>(_ownerWindow);
        if (result == true)
        {
            var dto = new UdruzenjeCreateDto
            {
                Naziv = editorVm.Naziv,
                Opis = editorVm.Opis,
                DatumOsnivanja = editorVm.DatumOsnivanja?.DateTime ?? DateTime.Today,
                Telefon = editorVm.Telefon,
                Email = editorVm.Email,
                Adresa = editorVm.Adresa,
                Admin = new AdminDto
                {
                    KorisnickoIme = editorVm.KorisnickoIme,
                    Lozinka = editorVm.Lozinka,
                    Ime = editorVm.Ime,
                    Prezime = editorVm.Prezime,
                    Email = editorVm.AdminEmail,
                    Telefon = editorVm.AdminTelefon,
                    Adresa = editorVm.AdminAdresa
                }
            };

            try
            {
                await Task.Run(() => _service.Create(dto));
                await OsveziAsync();
            }
            catch (InvalidOperationException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Greška",
                    ex.Message,
                    ButtonEnum.Ok,
                    Icon.Error);
                await box.ShowWindowDialogAsync(_ownerWindow);
            }
        }
    }

    private async Task IzmeniAsync()
    {
        if (SelectedUdruzenje == null) return;

        var editorVm = new UdruzenjeEditorViewModel
        {
            Id = SelectedUdruzenje.Id,
            Naziv = SelectedUdruzenje.Naziv,
            Opis = SelectedUdruzenje.Opis,
            DatumOsnivanja = new DateTimeOffset(SelectedUdruzenje.DatumOsnivanja, TimeSpan.Zero), 
            Telefon = SelectedUdruzenje.Telefon,
            Email = SelectedUdruzenje.Email,
            Adresa = SelectedUdruzenje.Adresa,
            IsEditMode = true 
        };

        var window = new UdruzenjeEditorWindow
        {
            DataContext = editorVm,
            Title = "Izmeni udruženje"
        };

        var result = await window.ShowDialog<bool?>(_ownerWindow);
        if (result == true)
        {
            var dto = new UdruzenjeUpdateDto
            {
                Id = editorVm.Id,
                Naziv = editorVm.Naziv,
                Opis = editorVm.Opis,
                DatumOsnivanja = editorVm.DatumOsnivanja?.DateTime ?? DateTime.Today,
                Telefon = editorVm.Telefon,
                Email = editorVm.Email,
                Adresa = editorVm.Adresa
            };

            await Task.Run(() => _service.Update(dto));
            await OsveziAsync();
        }
    }

    private async Task ObrisiAsync()
    {
        if (SelectedUdruzenje == null || _ownerWindow == null) return;

        var result = await MessageBoxManager
            .GetMessageBoxStandard(
                "Potvrda brisanja",
                $"Da li ste sigurni da želite obrisati udruženje '{SelectedUdruzenje.Naziv}'?",
                ButtonEnum.YesNo,
                Icon.Warning)
            .ShowWindowDialogAsync(_ownerWindow);

        if (result == ButtonResult.Yes)
        {
            await Task.Run(() => _service.Delete(SelectedUdruzenje.Id));
            await OsveziAsync();
        }
    }
}