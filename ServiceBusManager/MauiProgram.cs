using Microsoft.Extensions.Logging;
using ServiceBusManager.ViewModels;
using ServiceBusManager.Services;
using ServiceBusManager.Models;
using ServiceBusManager.Views;

namespace ServiceBusManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.otf", "InterRegular");
                fonts.AddFont("Inter-Bold.otf", "InterBold");
                fonts.AddFont("Font-Awesome-6-Free-Solid-900.otf", "FontAwesomeSolid");
            });

        // Register services first
        builder.Services.AddSingleton<IServiceBusService, ServiceBusService>();
        builder.Services.AddSingleton<ILoggingService, LoggingService>();
        builder.Services.AddSingleton<IConnectionStorageService, ConnectionStorageService>();

        // Register ViewModels in dependency order
        builder.Services.AddSingleton<ConnectionModalViewModel>();
        builder.Services.AddSingleton<DetailsViewModel>();
        builder.Services.AddSingleton<ExplorerViewModel>();
        builder.Services.AddSingleton<LogsViewModel>();
        builder.Services.AddSingleton<MainViewModel>();

        // Register Views
        builder.Services.AddSingleton<ConnectionModal>();
        builder.Services.AddSingleton<DetailsView>();
        builder.Services.AddSingleton<ExplorerView>();
        builder.Services.AddSingleton<LogsView>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
