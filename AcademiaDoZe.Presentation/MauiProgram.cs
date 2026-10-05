using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiadoZE.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Presentation.Pages;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation;
//Mario Cesar Alves Júnior

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
   
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIconsRound-Regular.otf", "MaterialIcons");
            });

        builder.Services.AddTransient<ILogradouroRepository>(_ =>
        {
            var servidor = Preferences.Default.Get(
                "DbServidor",
                "127.0.0.1");

            var banco = Preferences.Default.Get(
                "DbBanco",
                "db_academia_do_ze");

            var usuario = Preferences.Default.Get(
                "DbUsuario",
                "mario");

            var senha = Preferences.Default.Get(
                "DbSenha",
                "Mario@12345");

            var connectionString =
                $"Server={servidor};Port=3306;Database={banco};User Id={usuario};Password={senha};";

            return new LogradouroRepository(
                connectionString,
                DatabaseType.MySql);
        });
        builder.Services.AddTransient<ILogradouroService, LogradouroService>();

        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<LogradourosPage>();
        builder.Services.AddTransient<LogradouroFormPage>();
        builder.Services.AddSingleton<ConfigPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
