using ServiceBusManager.Models;
using ServiceBusManager.ViewModels;

namespace ServiceBusManager.Views;

public partial class ExplorerView : ContentView
{
    private ExplorerViewModel? ViewModel => BindingContext as ExplorerViewModel;

    // Default constructor for XAML
    public ExplorerView()
    {
        InitializeComponent();
    }

    // Constructor for DI
    public ExplorerView(ExplorerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ServiceBusResourceItem selectedItem && ViewModel != null)
        {
            ViewModel.SelectResourceCommand.Execute(selectedItem);
        }
    }
} 