using System;
using System.Collections.ObjectModel;
using System.IO;
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
    private readonly int _udruzenjeId;  // ID udruženja admina

    [ObservableProperty]
    private ObservableCollection<RacunStavka> _stavke = new();

    [ObservableProperty]
    private bool _isLoading;

    public ICommand UcitajCommand { get; }

    // Konstruktor prima ID udruženja
    public StatementViewModel(int udruzenjeId)
    {
        _udruzenjeId = udruzenjeId;
        UcitajCommand = new AsyncRelayCommand(UcitajAsync);
        _ = UcitajAsync();
    }

    private async Task UcitajAsync()
    {
        IsLoading = true;
        try
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "Statements", "izvodi.txt");
            var sveStavke = await Task.Run(() => _service.ImportFromFile(filePath));

            // Filtriraj samo one čiji je ID jednak ID-ju udruženja
            var filtrirane = sveStavke.FindAll(s => s.Id == _udruzenjeId);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Stavke.Clear();
                foreach (var item in filtrirane)
                    Stavke.Add(item);
            });
        }
        catch (Exception ex)
        {
            // opciono: prikaži poruku o grešci
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // npr. MessageBox
            });
        }
        finally
        {
            IsLoading = false;
        }
    }
}