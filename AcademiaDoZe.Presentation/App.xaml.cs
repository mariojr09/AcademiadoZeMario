using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;

        AplicarTemaSalvo();
    }

    private void AplicarTemaSalvo()
    {
        var tema = Preferences.Default.Get(
            "AppTema",
            "Sistema");

        UserAppTheme = tema switch
        {
            "Claro" => AppTheme.Light,
            "Escuro" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new ContentPage
        {
            Content = new ActivityIndicator
            {
                IsRunning = true,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        });

        _ = AbrirJanelaAsync(window);
        return window;
    }

    private async Task AbrirJanelaAsync(Window window)
    {
        try
        {
            await MauiProgram.CarregarSenhaBancoAsync();
            window.Page = _serviceProvider.GetRequiredService<AppShell>();
        }
        catch (Exception ex)
        {
            window.Page = new ContentPage
            {
                Title = "Erro ao iniciar",
                Content = new Label
                {
                    Text = $"Não foi possível abrir o aplicativo: {ex.Message}",
                    Margin = new Thickness(24),
                    VerticalOptions = LayoutOptions.Center
                }
            };
        }
    }
}
