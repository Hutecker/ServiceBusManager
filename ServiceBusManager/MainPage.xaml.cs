using ServiceBusManager.ViewModels;

namespace ServiceBusManager;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
