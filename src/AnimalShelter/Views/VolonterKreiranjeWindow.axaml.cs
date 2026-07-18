using Avalonia.Controls;
using Avalonia.Interactivity;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class VolonterKreiranjeWindow : Window
{
    public VolonterKreiranjeWindow()
    {
        InitializeComponent();
        if (DataContext is VolonterKreiranjeViewModel vm)
        {
            vm.RequestClose += (s, e) =>
            {
                if (e is VolonterKreiranjeViewModel.DialogResultEventArgs args)
                    Close(args.Result);
                else
                    Close(false);
            };
        }
    }

    private void OnKreirajClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is VolonterKreiranjeViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.KorisnickoIme) || 
                string.IsNullOrWhiteSpace(vm.Lozinka) || 
                vm.Lozinka != vm.LozinkaPotvrda)
                return;
            Close(true);
        }
    }

    private void OnOdustaniClick(object sender, RoutedEventArgs e) => Close(false);
}