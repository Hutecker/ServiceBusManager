using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public string title = "Welcome to .NET MAUI Community Toolkit!";

    [ObservableProperty]
    public string message = "This is a sample application to demonstrate the .NET MAUI Community Toolkit.";

    [ObservableProperty]
    public string buttonText = "Click me!";

    private int clickCount = 0;

    [RelayCommand]
    public void Click()
    {
        clickCount++;
        ButtonText = $"Clicked {clickCount} times";
    }
}
