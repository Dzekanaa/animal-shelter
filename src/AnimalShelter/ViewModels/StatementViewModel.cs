using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnimalShelter.Models;
using AnimalShelter.DataBase.Services;

namespace AnimalShelter.ViewModels;

public partial class StatementViewModel : ObservableObject
{
    private readonly StatementService _service = new();

    [ObservableProperty]
    private ObservableCollection<RacunStavka> _stavke = new();

    [ObservableProperty]
    private bool _isLoading;

    public ICommand UcitajCommand { get; }

    public StatementViewModel()
    {
        UcitajCommand = new AsyncRelayCommand(UcitajAsync);
        _ = UcitajAsync(); // automatski učitaj na otvaranju
    }

    private async Task UcitajAsync()
    {
        IsLoading = true;
        try
        {
            // Putanja do datoteke – relativna u odnosu na izvršni folder
            var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "Statements", "izvodi.txt");
            var lista = await Task.Run(() => _service.ImportFromFile(filePath));
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Stavke.Clear();
                foreach (var item in lista)
                    Stavke.Add(item);
            });
        }
        catch (System.Exception ex)
        {
            // opciono: prikaži poruku o grešci
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // možemo koristiti MessageBox
            });
        }
        finally
        {
            IsLoading = false;
        }
    }
}