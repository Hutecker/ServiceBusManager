using ServiceBusManager.ViewModels;

namespace ServiceBusManager.Views;

public partial class ConnectionModal : ContentView
{
    public ConnectionModal()
    {
        InitializeComponent();
    }
    
    public ConnectionModal(ConnectionModalViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 