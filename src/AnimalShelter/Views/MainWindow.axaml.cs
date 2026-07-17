using Avalonia.Controls;
using AnimalShelter.ViewModels;

namespace AnimalShelter.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Postavljamo DataContext na ViewModel koji koristi UdruzenjaView
        DataContext = new UdruzenjaViewModel();
        // Postavljamo Content na novi UserControl koji koristi isti DataContext
        Content = new UdruzenjaView { DataContext = DataContext };
        
        var vm = DataContext as UdruzenjaViewModel;
        vm?.SetOwnerWindow(this);
    }
}