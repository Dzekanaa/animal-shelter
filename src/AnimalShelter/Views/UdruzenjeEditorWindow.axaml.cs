using Avalonia.Controls;
using Avalonia.Interactivity;
using AnimalShelter.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AnimalShelter.Views;

public partial class UdruzenjeEditorWindow : Window
{
    public UdruzenjeEditorWindow()
    {
        InitializeComponent();
    }

    private void OnOdustaniClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async void OnSacuvajClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UdruzenjeEditorViewModel vm)
            return;

        bool isValid;

        if (vm.IsEditMode)
        {
            // U edit modu provjeravamo samo Naziv (i eventualno druga polja udruženja)
            isValid = !string.IsNullOrWhiteSpace(vm.Naziv);
        }
        else
        {
            // U create modu provjeravamo sva obavezna polja (uključujući admin)
            isValid = !string.IsNullOrWhiteSpace(vm.Naziv) &&
                      !string.IsNullOrWhiteSpace(vm.KorisnickoIme) &&
                      !string.IsNullOrWhiteSpace(vm.Lozinka) &&
                      !string.IsNullOrWhiteSpace(vm.Ime) &&
                      !string.IsNullOrWhiteSpace(vm.Prezime) &&
                      !string.IsNullOrWhiteSpace(vm.AdminAdresa);
        }

        if (!isValid)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                "Greška",
                "Sva obavezna polja moraju biti popunjena.",
                ButtonEnum.Ok);
            await messageBox.ShowWindowDialogAsync(this);
            return;
        }

        Close(true);
    }
}