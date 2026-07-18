using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnimalShelter.DataBase.Dtos;
using AnimalShelter.DataBase.Services;

namespace AnimalShelter.ViewModels;

public partial class SviKorisniciViewModel : ObservableObject
{
    private readonly KorisnikService _service = new();

    [ObservableProperty]
    private ObservableCollection<KorisnikDto> _korisnici = new();

    [ObservableProperty]
    private string _filter = string.Empty;

    public ICommand OsveziCommand { get; }
    public ICommand PretraziCommand { get; }

    public SviKorisniciViewModel()
    {
        OsveziCommand = new AsyncRelayCommand(OsveziAsync);
        PretraziCommand = new AsyncRelayCommand(OsveziAsync);
        _ = OsveziAsync();
    }

    private async Task OsveziAsync()
    {
        var lista = await Task.Run(() => _service.GetAllUsers());
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Korisnici.Clear();
            foreach (var item in lista)
            {
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    var filterLower = Filter.ToLower();
                    if (item.KorisnickoIme?.ToLower().Contains(filterLower) == true ||
                        item.Ime?.ToLower().Contains(filterLower) == true ||
                        item.Prezime?.ToLower().Contains(filterLower) == true ||
                        item.Email?.ToLower().Contains(filterLower) == true)
                    {
                        Korisnici.Add(item);
                    }
                }
                else
                {
                    Korisnici.Add(item);
                }
            }
        });
    }
}