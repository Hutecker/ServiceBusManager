using ServiceBusManager.ViewModels;

namespace ServiceBusManager.Views;

public partial class DetailsView : ContentView
{
    // Default constructor for XAML
    public DetailsView()
    {
        InitializeComponent();
        // No BindingContext set here - will be set externally
    }

    // Constructor for DI
    public DetailsView(DetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 