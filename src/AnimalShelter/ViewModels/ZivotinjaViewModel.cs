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

namespace AnimalShelter.ViewModels;

public partial class ZivotinjeViewModel : ObservableObject
{
    private readonly ZivotinjaService _service;
    private readonly int _udruzenjeId;

    [ObservableProperty]
    private ObservableCollection<Zivotinja> _zivotinje = new();

    [ObservableProperty]
    private Zivotinja? _selectedZivotinja;

    public ICommand DodajCommand { get; }
    public ICommand IzmeniCommand { get; }
    public ICommand ObrisiCommand { get; }
    public ICommand OsveziCommand { get; }

    private Window? _ownerWindow;

    public ZivotinjeViewModel(int udruzenjeId)
    {
        _udruzenjeId = udruzenjeId;
        _service = new ZivotinjaService();

        DodajCommand = new AsyncRelayCommand(DodajAsync);
        IzmeniCommand = new AsyncRelayCommand(IzmeniAsync, () => SelectedZivotinja != null);
        ObrisiCommand = new AsyncRelayCommand(ObrisiAsync, () => SelectedZivotinja != null);
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);

        _ = OsveziAsync();
    }
    partial void OnSelectedZivotinjaChanged(Zivotinja? value)
    {
        (IzmeniCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        (ObrisiCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
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