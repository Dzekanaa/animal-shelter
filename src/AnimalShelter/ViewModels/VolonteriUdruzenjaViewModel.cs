using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnimalShelter.Models;
using AnimalShelter.DataBase.Services;

namespace AnimalShelter.ViewModels;

public partial class VolonteriUdruzenjaViewModel : ObservableObject
{
    private readonly KorisnikService _service = new();
    private readonly int _udruzenjeId;

    [ObservableProperty]
    private ObservableCollection<Korisnik> _volonteri = new();

    [ObservableProperty]
    private string _filter = string.Empty;

    public ICommand OsveziCommand { get; }
    public ICommand PretraziCommand { get; }

    public VolonteriUdruzenjaViewModel(int udruzenjeId)
    {
        _udruzenjeId = udruzenjeId;
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);
        PretraziCommand = new AsyncRelayCommand(OsveziAsync);
        _ = OsveziAsync();
    }

    private async Task OsveziAsync()
    {
        var lista = await Task.Run(() => _service.GetVolonteriByUdruzenje(_udruzenjeId));
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Volonteri.Clear();
            foreach (var item in lista)
            {
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    var filterLower = Filter.ToLower();
                    if (item.Ime?.ToLower().Contains(filterLower) == true ||
                        item.Prezime?.ToLower().Contains(filterLower) == true ||
                        item.Email?.ToLower().Contains(filterLower) == true ||
                        item.KorisnickoIme?.ToLower().Contains(filterLower) == true)
                    {
                        Volonteri.Add(item);
                    }
                }
                else
                {
                    Volonteri.Add(item);
                }
            }
        });
    }
}