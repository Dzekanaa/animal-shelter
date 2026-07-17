using Avalonia.Controls;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class ZivotinjeWindow : Window
{
    public ZivotinjeWindow()
    {
        InitializeComponent();
    }

    public void SetViewModel(ZivotinjeViewModel vm)
    {
        DataContext = vm;
        vm.SetOwnerWindow(this);
        
        // Kreiraj novi ZivotinjeView i postavi ga kao Content
        var view = new ZivotinjeView();
        view.DataContext = vm;
        Content = view;
    }
}