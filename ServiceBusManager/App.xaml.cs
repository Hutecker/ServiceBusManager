namespace ServiceBusManager;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Force the app into light or dark mode for debugging
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}