using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using AnimalShelter.Models;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private bool _isLoggedIn;
    private string _windowTitle = "Animal Shelter - Prijava";

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set { _isLoggedIn = value; OnPropertyChanged(); }
    }

    public string WindowTitle
    {
        get => _windowTitle;
        set { _windowTitle = value; OnPropertyChanged(); }
    }

    private LoginViewModel? _loginVm;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this; // Važno: postavljamo DataContext za bindings
        ShowLoginView();
    }

    private void ShowLoginView()
    {
        _loginVm = new LoginViewModel();
        _loginVm.LoginSucceeded += OnLoginSucceeded;

        var loginView = new LoginView { DataContext = _loginVm };
        MainContent.Content = loginView;
        IsLoggedIn = false;
        WindowTitle = "Animal Shelter - Prijava";
    }

    private void OnLoginSucceeded(object? sender, Korisnik? user)
    {
        // Postavi trenutnog korisnika
        AppSession.CurrentUser = user;

        // Ako je korisnik null -> gost, nije prijavljen
        IsLoggedIn = user != null;
        WindowTitle = user == null 
            ? "Animal Shelter - Gost" 
            : $"Animal Shelter - {user.Ime} {user.Prezime}";

        // Odjavi event da ne bi ponovo reagovao
        if (_loginVm != null)
            _loginVm.LoginSucceeded -= OnLoginSucceeded;

        // Pripremi UdruzenjaView i prikaži
        var udruzenjaVm = new UdruzenjaViewModel();
        udruzenjaVm.SetOwnerWindow(this);

        var udruzenjaView = new UdruzenjaView { DataContext = udruzenjaVm };
        MainContent.Content = udruzenjaView;
    }

    private void OnLogoutClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Čisti sesiju i vraća na login
        AppSession.CurrentUser = null;
        IsLoggedIn = false;
        ShowLoginView();
    }

    // Implementacija INotifyPropertyChanged
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}