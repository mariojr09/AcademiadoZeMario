using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiadoZE.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Presentation.Pages;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        const string mySqlConnectionString =
            "Server=127.0.0.1;Port=3306;Database=db_academia_do_ze;User Id=mario;Password=Mario@12345;";

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<ILogradouroRepository>(_ =>
            new LogradouroRepository(mySqlConnectionString, DatabaseType.MySql));
        builder.Services.AddTransient<ILogradouroService, LogradouroService>();

        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<LogradourosPage>();
        builder.Services.AddTransient<LogradouroFormPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
