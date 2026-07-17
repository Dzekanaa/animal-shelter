using Avalonia.Controls;
using Avalonia.Interactivity;
using AnimalShelter.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AnimalShelter.Views;

public partial class ZivotinjeEditorWindow : Window
{
    public ZivotinjeEditorWindow()
    {
        InitializeComponent();
    }

    private void OnOdustaniClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async void OnSacuvajClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ZivotinjeEditorViewModel vm)
            return;

        // Obavezna polja: Naziv
        if (string.IsNullOrWhiteSpace(vm.Naziv))
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(
                "Greška",
                "Polje 'Naziv' je obavezno.",
                ButtonEnum.Ok);
            await msg.ShowWindowDialogAsync(this);
            return;
        }

        Close(true);
    }
}