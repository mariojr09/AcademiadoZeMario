
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
    private static string _senhaBanco = string.Empty;

    public static async Task CarregarSenhaBancoAsync()
    {
        try
        {
            _senhaBanco = await SecureStorage.Default.GetAsync("DbSenha") ?? string.Empty;
        }
        catch
        {
            _senhaBanco = string.Empty;
        }
    }

    private static string ObterConnectionString()
    {
        return new MySqlConnectionStringBuilder
        {
            Server = Preferences.Default.Get("DbServidor", "127.0.0.1"),
            Port = 3306,
            Database = Preferences.Default.Get("DbBanco", "db_academia_do_ze"),
            UserID = Preferences.Default.Get("DbUsuario", string.Empty),
            Password = _senhaBanco,
            CharacterSet = "utf8mb4"
        }.ConnectionString;
    }

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

        // Repositório de Logradouros
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

        builder.Services.AddTransient<IMatriculaRepository>(_ =>
            new MatriculaRepository(ObterConnectionString(), DatabaseType.MySql));

        builder.Services.AddTransient<IMatriculaService, MatriculaService>();

        builder.Services.AddTransient<IAcessoAlunoRepository>(_ =>
            new AcessoAlunoRepository(ObterConnectionString(), DatabaseType.MySql));

        builder.Services.AddTransient<IAcessoColaboradorRepository>(_ =>
            new AcessoColaboradorRepository(ObterConnectionString(), DatabaseType.MySql));

        builder.Services.AddTransient<IAcessoService, AcessoService>();

        // Páginas do aplicativo
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<LogradourosPage>();
        builder.Services.AddSingleton<ColaboradorListPage>();
        builder.Services.AddSingleton<AlunoListPage>();
        builder.Services.AddSingleton<MatriculaListPage>();
        builder.Services.AddSingleton<AcessoPage>();
        builder.Services.AddTransient<LogradouroFormPage>();
        builder.Services.AddTransient<ColaboradorPage>();
        builder.Services.AddTransient<AlunoPage>();
        builder.Services.AddTransient<MatriculaPage>();
        builder.Services.AddSingleton<ConfigPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
