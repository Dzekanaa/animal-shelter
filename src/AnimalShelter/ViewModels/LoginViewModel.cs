using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnimalShelter.DataBase.Services;
using AnimalShelter.Models;

namespace AnimalShelter.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly KorisnikService _korisnikService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    // NOVO: svojstvo koje određuje da li je moguće prijaviti se
    public bool CanLogin => !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    public ICommand LoginCommand { get; }
    public ICommand OdustaniCommand { get; }

    public event EventHandler<Korisnik?>? LoginSucceeded;

    public LoginViewModel()
    {
        _korisnikService = new KorisnikService();
        // Koristimo CanLogin svojstvo za CanExecute
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => CanLogin);
        OdustaniCommand = new RelayCommand(Odustani);
    }

    // Osiguravamo da se CanLogin osvježi kada se promijene Username, Password ili IsLoading
    partial void OnUsernameChanged(string value)
    {
        OnPropertyChanged(nameof(CanLogin));
        (LoginCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        OnPropertyChanged(nameof(CanLogin));
        (LoginCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanLogin));
        (LoginCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
    }

    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var user = await Task.Run(() => _korisnikService.Authenticate(Username, Password));
            if (user != null)
            {
                LoginSucceeded?.Invoke(this, user);
            }
            else
            {
                ErrorMessage = "Pogrešno korisničko ime ili lozinka.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Odustani()
    {
        Environment.Exit(0);
    }
}