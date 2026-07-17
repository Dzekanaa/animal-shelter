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
        // Postavi trenutnog korisnika u sesiju
        AppSession.CurrentUser = user;  // ako je null, gost
        // Udalji login event
        if (_loginVm != null)
            _loginVm.LoginSucceeded -= OnLoginSucceeded;

        // Pripremi UdruzenjaViewModel i prikaži
        var udruzenjaVm = new UdruzenjaViewModel();
        udruzenjaVm.SetOwnerWindow(this);

        var udruzenjaView = new UdruzenjaView
        {
            DataContext = udruzenjaVm
        };

        MainContent.Content = udruzenjaView;
        Title = user == null ? "Animal Shelter - Gost" : $"Animal Shelter - {user.Ime} {user.Prezime}";
    }
}