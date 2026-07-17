using Avalonia.Controls;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class UdruzenjaWindow : Window
{
    public UdruzenjaWindow()
    {
        InitializeComponent();
        if (DataContext is UdruzenjaViewModel vm)
            vm.SetOwnerWindow(this);
    }
}