﻿using Microsoft.Extensions.Logging;
using ServiceBusManager.ViewModels;

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
                fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FontAwesomeSolid");
            });

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
