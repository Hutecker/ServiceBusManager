using ServiceBusManager.ViewModels;

namespace ServiceBusManager.Views;

public partial class LogsView : ContentView
{
    // Default constructor for XAML
    public LogsView()
    {
        InitializeComponent();
    }

    // Constructor for DI
    public LogsView(LogsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 