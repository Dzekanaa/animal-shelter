using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnimalShelter.Models;
using AnimalShelter.DataBase.Services;

namespace AnimalShelter.ViewModels;

public partial class MojeZivotinjeViewModel : ObservableObject
{
    private readonly ZivotinjaService _service = new();
    private readonly int _volonterId;

    [ObservableProperty]
    private ObservableCollection<Zivotinja> _zivotinje = new();

    [ObservableProperty]
    private Zivotinja? _selectedZivotinja;

    public ICommand OsveziCommand { get; }

    public MojeZivotinjeViewModel(int volonterId)
    {
        _volonterId = volonterId;
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);
        _ = OsveziAsync();
    }

    private async Task OsveziAsync()
    {
        var lista = await Task.Run(() => _service.GetByVolonterId(_volonterId));
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Zivotinje.Clear();
            foreach (var item in lista)
                Zivotinje.Add(item);
        });
    }
}