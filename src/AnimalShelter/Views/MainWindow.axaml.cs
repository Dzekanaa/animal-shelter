using Avalonia.Controls;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class MainWindow : Window
{
    private LoginViewModel? _loginVm;

    public MainWindow()
    {
        InitializeComponent();

        // Prikazujemo LoginView na startu
        ShowLoginView();
    }

    private void ShowLoginView()
    {
        _loginVm = new LoginViewModel();
        _loginVm.LoginSucceeded += OnLoginSucceeded;

        var loginView = new LoginView
        {
            DataContext = _loginVm
        };

        MainContent.Content = loginView;
        Title = "Animal Shelter - Prijava";
    }

    private void OnLoginSucceeded(object? sender, Models.Korisnik? user)
    {
        // Nakon uspješne prijave, prelazimo na UdruzenjaView
        if (user == null) return;

        // Opciono: možemo čuvati prijavljenog korisnika negdje (npr. App.Current.Properties)
        // Ovdje samo prelazimo na listu udruženja
        var udruzenjaVm = new UdruzenjaViewModel();
        udruzenjaVm.SetOwnerWindow(this);

        var udruzenjaView = new UdruzenjaView
        {
            DataContext = udruzenjaVm
        };

        MainContent.Content = udruzenjaView;
        Title = "Animal Shelter - Upravljanje udruženjima";

        // Opciono: odjavimo LoginSucceeded da ne bi slučajno ponovo reagovao
        if (_loginVm != null)
            _loginVm.LoginSucceeded -= OnLoginSucceeded;
    }
}