using Avalonia.Controls;
using Avalonia.Interactivity;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class PrijavaVolontiranjaEditorWindow : Window
{
    public PrijavaVolontiranjaEditorWindow()
    {
        InitializeComponent();
        if (DataContext is PrijavaVolontiranjaEditorViewModel vm)
        {
            vm.RequestClose += (s, e) =>
            {
                if (e is PrijavaVolontiranjaEditorViewModel.DialogResultEventArgs args)
                    Close(args.Result);
                else
                    Close(false);
            };
        }
    }

    private void OnPosaljiClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is PrijavaVolontiranjaEditorViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Ime) || string.IsNullOrWhiteSpace(vm.Prezime))
            {
                // opcionalno: poruka
                return;
            }
            Close(true);
        }
    }

    private void OnOdustaniClick(object sender, RoutedEventArgs e) => Close(false);
}