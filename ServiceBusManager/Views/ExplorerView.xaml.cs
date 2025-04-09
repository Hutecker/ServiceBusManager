using ServiceBusManager.ViewModels;

namespace ServiceBusManager.Views;

public partial class ExplorerView : ContentView
{
    // Default constructor for XAML
    public ExplorerView()
    {
        InitializeComponent();
        // No BindingContext set here - will be set externally
    }

    // Constructor for DI
    public ExplorerView(ExplorerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 