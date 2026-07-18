using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using AnimalShelter.Models;
using AnimalShelter.DataBase.Services;

namespace AnimalShelter.ViewModels;

public partial class UdomiteljiViewModel : ObservableObject
{
    private readonly KorisnikService _service = new();

    [ObservableProperty]
    private ObservableCollection<Korisnik> _udomitelji = new();

    public UdomiteljiViewModel()
    {
        _ = LoadUdomiteljiAsync();
    }

    public async Task LoadUdomiteljiAsync()
    {
        var lista = await Task.Run(() => _service.GetAllUdomitelji());
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Udomitelji.Clear();
            foreach (var item in lista)
                Udomitelji.Add(item);
        });
    }
}