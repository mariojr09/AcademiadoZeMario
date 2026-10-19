
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiadoZE.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Presentation.Pages;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace AcademiaDoZe.Presentation;

// Mario Cesar Alves Júnior
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

        static string ObterConnectionString()
        {
            string senha;
            try
            {
                senha = SecureStorage.Default.GetAsync("DbSenha").GetAwaiter().GetResult() ?? string.Empty;
            }
            catch
            {
                senha = string.Empty;
            }

            var builder = new MySqlConnectionStringBuilder
            {
                Server = Preferences.Default.Get("DbServidor", "127.0.0.1"),
                Port = 3306,
                Database = Preferences.Default.Get("DbBanco", "db_academia_do_ze"),
                UserID = Preferences.Default.Get("DbUsuario", string.Empty),
                Password = senha,
                CharacterSet = "utf8mb4"
            };

            return builder.ConnectionString;
        }

        // O aplicativo utiliza exclusivamente MySQL; as credenciais são informadas
        // localmente na tela de configurações e nunca ficam gravadas no código-fonte.
        builder.Services.AddTransient<ILogradouroRepository>(_ =>
            new LogradouroRepository(ObterConnectionString(), DatabaseType.MySql));

        builder.Services.AddTransient<ILogradouroService, LogradouroService>();

        // Repositório de Colaboradores
        builder.Services.AddTransient<IColaboradorRepository>(_ =>
            new ColaboradorRepository(ObterConnectionString(), DatabaseType.MySql));

        builder.Services.AddTransient<IColaboradorService, ColaboradorService>();

        builder.Services.AddTransient<IAlunoRepository>(_ =>
            new AlunoRepository(ObterConnectionString(), DatabaseType.MySql));

        builder.Services.AddTransient<IAlunoService, AlunoService>();

        // Páginas do aplicativo
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<LogradourosPage>();
        builder.Services.AddTransient<ColaboradorListPage>();
        builder.Services.AddTransient<ColaboradorPage>();
        builder.Services.AddTransient<AlunoListPage>();
        builder.Services.AddTransient<AlunoPage>();
        builder.Services.AddTransient<LogradouroFormPage>();
        builder.Services.AddSingleton<ConfigPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
