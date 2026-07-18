using Avalonia.Controls;
using Avalonia.Interactivity;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class AdoptionRequestEditorWindow : Window
{
    public AdoptionRequestEditorWindow()
    {
        InitializeComponent();
        if (DataContext is AdoptionRequestEditorViewModel vm)
        {
            vm.RequestClose += (s, e) =>
            {
                if (e is AdoptionRequestEditorViewModel.DialogResultEventArgs args)
                    Close(args.Result);
                else
                    Close(false);
            };
        }
    }

    private void OnPosaljiClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is AdoptionRequestEditorViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Ime) || string.IsNullOrWhiteSpace(vm.Prezime))
                return;
            Close(true);
        }
    }

    private void OnOdustaniClick(object sender, RoutedEventArgs e) => Close(false);
}